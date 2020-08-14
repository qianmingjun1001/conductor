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

                var valueType = GetJTokenType(value) ?? typeof(object);

                if (value is JObject o)
                {
                    var (dynamicType, dynamicClass) = GenerateDynamicClass(o);
                    props.Add(new DynamicProperty(key, dynamicType));
                    propValues.Add(key, dynamicClass);
                }
                else if (value is JArray array)
                {
                    GenerateArray(key, array, props, propValues);
                }
                else
                {
                    props.Add(new DynamicProperty(key, valueType));
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

        private static void GenerateArray(string key, JArray value, List<DynamicProperty> props, Dictionary<string, object> propValues)
        {
            var list = new List<object>();
            foreach (var item in value)
            {
                if (item is JObject o)
                {
                    var (_, dynamicClass) = GenerateDynamicClass(o);
                    list.Add(dynamicClass);
                }
                else if (item is JArray array)
                {
                    list.Add(array);
                }
                else
                {
                    list.Add(item.ToObject(GetJTokenType(item)));
                }
            }

            //list 中类型都一致时返回 ElementType，否则返回 null
            var elementType = GetElementType(list);
            if (elementType == null)
            {
                props.Add(new DynamicProperty(key, typeof(List<object>)));
                propValues.Add(key, list);
            }
            else
            {
                props.Add(new DynamicProperty(key, typeof(List<>).MakeGenericType(elementType)));
                var castMethod = CastMethod.MakeGenericMethod(typeof(object), elementType);
                propValues.Add(key, castMethod.Invoke(null, new object[] {list}));
            }
        }

        private static Type GetElementType(List<object> list)
        {
            object first = null;
            foreach (var item in list)
            {
                first = first ?? item;
                if (first?.GetType() != item?.GetType())
                {
                    return null;
                }
            }

            return first?.GetType();
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
                case JTokenType.None:
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;
                default:
                    return jToken.GetType();
            }
        }

        private static readonly MethodInfo CastMethod = typeof(JObjectExtension).GetMethod(nameof(Cast), BindingFlags.NonPublic | BindingFlags.Static);

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