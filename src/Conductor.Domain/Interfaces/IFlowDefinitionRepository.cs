using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conductor.Domain.Entities;

namespace Conductor.Domain.Interfaces
{
    public interface IFlowDefinitionRepository : IRepository<FlowDefinition>
    {
    }
}