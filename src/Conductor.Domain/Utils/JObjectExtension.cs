using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Conductor.Domain.Utils
{
    public static class JObjectExtension
    {
        public static DynamicClass ToDynamicClass(this JObject jObject)
        {
            if (jObject == null) return null;

            return GenerateDynamicClass(jObject).dynamicClass;
        }

        private static (Type dynamicType, DynamicClass dynamicClass) GenerateDynamicClass(JObject jObject)
        {
            var props = new List<DynamicProperty>();
            var propValues = new Dictionary<string, object>();

            foreach (var pair in jObject)
            {
                var key = pair.Key;
                var value = pair.Value;

                var valueType = GetJTokenType(value);
                if (value is JObject o)
                {
                    var (dynamicType, dynamicClass) = GenerateDynamicClass(o);
                    props.Add(new DynamicProperty(key, dynamicType));
                    propValues.Add(key, dynamicClass);
                }
                else if (value is JArray array)
                {
                    //如果是数组，需要保证所有元素都是一致的，否则不做处理
                    if (AllIsJObject(array))
                    {
                        Type elementType = null;
                        var list = new List<DynamicClass>();
                        foreach (var item in array.Cast<JObject>())
                        {
                            if (item == null)
                            {
                                list.Add(null);
                                continue;
                            }

                            var (dynamicType, dynamicClass) = GenerateDynamicClass(item);
                            elementType = elementType ?? dynamicType;
                            list.Add(dynamicClass);
                        }

                        props.Add(new DynamicProperty(key, typeof(List<>).MakeGenericType(elementType)));

                        var castMethod = typeof(JObjectExtension).GetMethod(nameof(Cast), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(typeof(DynamicClass), elementType);
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
                    propValues.Add(key, value.ToObject(valueType));
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

        private static Type GetJTokenType(JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.Integer:
                    return typeof(int);
                case JTokenType.Float:
                    return typeof(float);
                case JTokenType.String:
                    return typeof(string);
                case JTokenType.Boolean:
                    return typeof(bool);
                case JTokenType.Date:
                    return typeof(DateTime);
                case JTokenType.Bytes:
                    return typeof(byte[]);
                case JTokenType.Guid:
                    return typeof(Guid);
                case JTokenType.TimeSpan:
                    return typeof(TimeSpan);
                default:
                    return jToken.GetType();
            }
        }

        private static bool AllIsJObject(JArray jArray)
        {
            foreach (var item in jArray)
            {
                if (item != null && item.GetType() != typeof(JObject))
                {
                    return false;
                }
            }

            return true;
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