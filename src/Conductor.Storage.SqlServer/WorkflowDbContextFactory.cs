using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Conductor.Storage.SqlServer
{
    public class WorkflowDbContextFactory
    {
        private readonly string _connectionString;

        public WorkflowDbContextFactory([NotNull] string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public WorkflowDbContext Build()
        {
            return new WorkflowDbContext(_connectionString);
        }
    }
}