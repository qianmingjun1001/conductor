using System;
using Conductor.Domain.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Conductor.Storage.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkflowDbContext : DbContext
    {
        private readonly string _connectionString;

        public WorkflowDbContext([NotNull] string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<FlowDefinition> FlowDefinitions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}