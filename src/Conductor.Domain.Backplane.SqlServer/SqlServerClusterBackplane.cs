using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Services;
using Conductor.Domain.Utils;
using Dapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;

namespace Conductor.Domain.Backplane.SqlServer
{
    public class SqlServerClusterBackplane : IClusterBackplane
    {
        private readonly Guid _nodeId = Guid.NewGuid();
        private readonly string _connectionString;

        [NotNull]
        private readonly IDefinitionRepository _definitionRepository;

        [NotNull]
        private readonly IWorkflowLoader _loader;

        [NotNull]
        private readonly IWorkflowRegistry _workflowRegistry;

        [NotNull]
        private readonly ILogger _logger;

        private static readonly string GetLastVersionSql =
            "SELECT top 1 sys_change_version FROM CHANGETABLE(CHANGES dbo.DefinitionCommand, @version) AS ct order by sys_change_version desc";

        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public SqlServerClusterBackplane(
            [NotNull] string connectionString,
            [NotNull] IDefinitionRepository definitionRepository,
            [NotNull] IWorkflowLoader loader,
            [NotNull] IWorkflowRegistry workflowRegistry,
            [NotNull] ILoggerFactory logFactory)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _definitionRepository = definitionRepository;
            _loader = loader;
            _workflowRegistry = workflowRegistry;
            _logger = logFactory.CreateLogger<SqlServerClusterBackplane>();
        }

        public Task Start()
        {
            if (_task != null)
            {
                throw new InvalidOperationException("任务已开启");
            }

            EnsureChangeTrackingTableCreated();
            var builder = new SqlConnectionStringBuilder(_connectionString);
            EnsureDbCtOpened(builder.InitialCatalog);
            EnsureTableCtOpened();

            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Factory.StartNew(async () => await Do(), TaskCreationOptions.LongRunning);

            return Task.CompletedTask;
        }

        private async Task Do()
        {
            //首次获取最新版本
            var version = GetLastVersion(0);
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogInformation("Polling for cluster whether or not change");

                //10s 后再次轮询
                await Task.Delay(10 * 1000);

                var lastVersion = GetLastVersion(version);
                if (lastVersion != version)
                {
                    try
                    {
                        var evt = GetNewDefinitionCommand();
                        if (evt != null && evt.Originator != _nodeId)
                        {
                            var definition = _definitionRepository.Find(evt.DefinitionId, evt.Version);
                            if (definition != null)
                            {
                                if (_workflowRegistry.IsRegistered(evt.DefinitionId, evt.Version))
                                {
                                    _workflowRegistry.DeregisterWorkflow(evt.DefinitionId, evt.Version);
                                }

                                _loader.LoadDefinition(definition);
                            }
                        }

                        version = lastVersion;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            }
        }


        private int GetLastVersion(int version)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<int>(GetLastVersionSql, new {version = version == 0 ? 0 : version - 1});
            }
        }

        public Task Stop()
        {
            _cancellationTokenSource.Cancel();
            _task.Wait();
            _task = null;
            return Task.CompletedTask;
        }

        public void LoadNewDefinition(string id, int version)
        {
            var data = new NewDefinitionCommand()
            {
                Originator = _nodeId,
                DefinitionId = id,
                Version = version
            };

            var first = GetNewDefinitionCommand();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(
                    first == null
                        ? "INSERT INTO dbo.DefinitionCommand(Originator, DefinitionId, Version) VALUES (@Originator, @DefinitionId, @Version)"
                        : "UPDATE dbo.DefinitionCommand SET Originator=@Originator, DefinitionId=@DefinitionId, Version=@Version", data);
            }
        }

        private NewDefinitionCommand GetNewDefinitionCommand()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<NewDefinitionCommand>(
                    "SELECT TOP 1 Originator, DefinitionId, Version FROM dbo.DefinitionCommand");
            }
        }

        /// <summary>
        /// 确认主数据库CT已经打开
        /// </summary>
        /// <param name="dataBase"></param>
        private void EnsureDbCtOpened(string dataBase)
        {
            var sql = @"IF
	NOT EXISTS(	SELECT DB_NAME (database_id ) DataBaseName, is_auto_cleanup_on, retention_period, retention_period_units_desc 
				FROM sys.change_tracking_databases
				WHERE DB_NAME (database_id ) = '" + dataBase + "')ALTER DATABASE " + dataBase + " SET CHANGE_TRACKING = ON(CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON)";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql);
            }
        }

        /// <summary>
        /// 确认主库表的CT已经打开
        /// </summary>
        private void EnsureTableCtOpened()
        {
            var sql = @"IF NOT EXISTS ( SELECT OBJECT_NAME ( object_id ) TableName, is_track_columns_updated_on 
					FROM sys.change_tracking_tables WHERE OBJECT_NAME ( object_id ) = 'DefinitionCommand' ) ALTER TABLE dbo.DefinitionCommand ENABLE CHANGE_TRACKING ";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql);
            }
        }

        /// <summary>
        /// 确认CT表创建
        /// </summary>
        private void EnsureChangeTrackingTableCreated()
        {
            var sql = "IF(NOT EXISTS(SELECT * FROM sys.objects WHERE name='DefinitionCommand'))" +
                      @"CREATE TABLE [dbo].[DefinitionCommand](
	                        [Originator] [UNIQUEIDENTIFIER] NOT NULL PRIMARY KEY,
	                        [DefinitionId] [NVARCHAR](100) NOT NULL,
	                        [Version] [INT] NOT NULL
	                        )";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(sql);
            }
        }
    }
}