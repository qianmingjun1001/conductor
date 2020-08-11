using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly Action<IServiceProvider, string> _registryDynamicRouteCallback;
        private readonly IFlowDefinitionService _flowDefinitionService;
        private readonly IWorkflowLoader _loader;
        private readonly IWorkflowRegistry _workflowRegistry;
        private readonly ILogger _logger;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public SqlServerClusterBackplane(
            [NotNull] string connectionString,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] Action<IServiceProvider, string> registryDynamicRouteCallback,
            [NotNull] IFlowDefinitionService flowDefinitionService,
            [NotNull] IWorkflowLoader loader,
            [NotNull] IWorkflowRegistry workflowRegistry,
            [NotNull] ILoggerFactory logFactory)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _serviceProvider = serviceProvider;
            _registryDynamicRouteCallback = registryDynamicRouteCallback;
            _flowDefinitionService = flowDefinitionService;
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
                        var commands = GetNewDefinitionCommands(version);
                        foreach (var command in commands)
                        {
                            if (command.Originator != _nodeId)
                            {
                                var flowDefinition = await _flowDefinitionService.GetFlowByIdAndVersion(command.DefinitionId, command.Version);
                                if (flowDefinition != null)
                                {
                                    if (_workflowRegistry.IsRegistered(command.DefinitionId, command.Version))
                                    {
                                        _workflowRegistry.DeregisterWorkflow(command.DefinitionId, command.Version);
                                    }

                                    _loader.LoadDefinition(JsonUtils.Deserialize<Definition>(flowDefinition.Definition));
                                    _logger.LogInformation($"id: {command.DefinitionId}, version: {command.Version} definition loaded");

                                    _registryDynamicRouteCallback?.Invoke(_serviceProvider, flowDefinition.EntryPointPath);
                                    _logger.LogInformation($"Route: {flowDefinition.EntryPointPath} is registered");
                                }
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

        public Task Stop()
        {
            _cancellationTokenSource.Cancel();
            _task.Wait();
            _task = null;

            _logger.LogInformation("SqlServer Backplane stopped");

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

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(
                    "INSERT INTO dbo.DefinitionCommand(Originator, DefinitionId, Version) VALUES (@Originator, @DefinitionId, @Version)", data);
            }
        }

        private int GetLastVersion(int version)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<int>(
                    "SELECT top 1 sys_change_version FROM CHANGETABLE(CHANGES dbo.DefinitionCommand, @version) AS ct order by sys_change_version desc",
                    new {version = version == 0 ? 0 : version - 1});
            }
        }

        private List<NewDefinitionCommand> GetNewDefinitionCommands(int version)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<NewDefinitionCommand>(
                        @"SELECT dc.Originator, dc.DefinitionId, dc.Version FROM dbo.DefinitionCommand AS dc 
                            RIGHT JOIN CHANGETABLE(CHANGES dbo.DefinitionCommand, @version) AS ct ON ct.ID = dc.ID",
                        new {version})
                    .ToList();
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
					FROM sys.change_tracking_tables WHERE OBJECT_NAME ( object_id ) = 'DefinitionCommand' ) ALTER TABLE dbo.DefinitionCommand ENABLE CHANGE_TRACKING WITH(TRACK_COLUMNS_UPDATED = ON)";

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
                            [ID] INT IDENTITY(1, 1) PRIMARY KEY,
	                        [Originator] [UNIQUEIDENTIFIER] NOT NULL,
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