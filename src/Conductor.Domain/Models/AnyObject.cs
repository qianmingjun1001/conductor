using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Conductor.Domain.Models
{
    public class AnyObject : DynamicObject
    {
        private Dictionary<string, object> _propertiesDictionary;

        private Dictionary<string, object> Properties => _propertiesDictionary = _propertiesDictionary ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public object this[string name]
        {
            get => Properties.TryGetValue(name, out var obj) ? obj : null;
            set => Properties[name] = value;
        }


        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Properties.TryGetValue(binder.Name, out result);
            return true;
        }


        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Properties[binder.Name] = value;
            return true;
        }
    }
}