using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using JetBrains.Annotations;

namespace Conductor.Domain.Utils
{
    public static class ExpandoObjectExtension
    {
        public static DynamicClass ToDynamicClass(this ExpandoObject expandoObject)
        {
            if (expandoObject == null)
            {
                return null;
            }

            return GenerateDynamicClass(expandoObject).dynamicClass;
        }

        private static (Type dynamicType, DynamicClass dynamicClass) GenerateDynamicClass(ExpandoObject expandoObject)
        {
            var props = new List<DynamicProperty>();
            var propValues = new Dictionary<string, object>();

            foreach (var pair in expandoObject)
            {
                var key = pair.Key;
                var value = pair.Value;
                var valueType = value?.GetType();
                if (value is ExpandoObject)
                {
                    var (dynamicType, dynamicClass) = GenerateDynamicClass((ExpandoObject) value);
                    props.Add(new DynamicProperty(key, dynamicType));
                    propValues.Add(key, dynamicClass);
                }
                else if (!(value is string) && value is IEnumerable items)
                {
                    var elementType = valueType.GetElementType() ?? valueType.GetGenericArguments()[0];
                    if (elementType == typeof(ExpandoObject))
                    {
                        Type newElementType = null;
                        var list = new List<DynamicClass>();
                        foreach (var expando in items.Cast<ExpandoObject>())
                        {
                            if (expando == null)
                            {
                                list.Add(null);
                                continue;
                            }

                            var (dynamicType, dynamicClass) = GenerateDynamicClass(expando);
                            newElementType = newElementType ?? dynamicType;
                            list.Add(dynamicClass);
                        }

                        props.Add(new DynamicProperty(key, typeof(List<>).MakeGenericType(newElementType)));

                        var castMethod = typeof(ExpandoObjectExtension).GetMethod(nameof(Cast)).MakeGenericMethod(typeof(DynamicClass), newElementType);
                        propValues.Add(key, castMethod.Invoke(null, new object[] {list}));
                    }
                    else
                    {
                        props.Add(new DynamicProperty(key, valueType));
                        propValues.Add(key, value);
                    }
                }
                else
                {
                    props.Add(new DynamicProperty(key, valueType ?? typeof(object)));
                    propValues.Add(key, value);
                }
            }

            var type = DynamicClassFactory.CreateType(props);
            var clazz = Activator.CreateInstance(type) as DynamicClass;
            foreach (var pair in propValues)
            {
                clazz.SetDynamicPropertyValue(pair.Key, pair.Value);
            }

            return (type, clazz);
        }

        private static List<TResult> Cast<TSource, TResult>(IEnumerable<TSource> sources)
        {
            var list = new List<TResult>();
            foreach (var item in sources)
            {
                if (item is TResult result)
                {
                    list.Add(result);
                }
            }

            return list;
        }
    }
}