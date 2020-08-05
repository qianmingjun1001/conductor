using System;
using System.Collections.Generic;
using System.Text;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;

namespace Conductor.Domain.ReplacedServices
{
    public class ReplacedCustomStepService : ICustomStepService
    {
        public void SaveStepResource(Resource resource)
        {
        }

        public Resource GetStepResource(string name)
        {
            return null;
        }

        public void Execute(Resource resource, IDictionary<string, object> scope)
        {
        }
    }
}