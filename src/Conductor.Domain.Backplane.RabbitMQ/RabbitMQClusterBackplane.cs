using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Conductor.Domain.Interfaces;

namespace Conductor.Domain.Backplane.RabbitMQ
{
    public class RabbitMQClusterBackplane : IClusterBackplane
    {
        public Task Start()
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }

        public void LoadNewDefinition(string id, int version)
        {
            throw new NotImplementedException();
        }
    }
}