using System;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Conductor.Domain.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class JsonUtils
    {
        private static readonly JsonSerializerSettings _settings;

        static JsonUtils()
        {
            _settings = new JsonSerializerSettings {DateFormatString = "yyyy-MM-dd HH:mm:ss", ContractResolver = new CamelCasePropertyNamesContractResolver()};
            _settings.Converters.Add(new AnyObjectConverter());
        }

        /// <summary>
        /// 将对象序列化成 json 字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Serialize<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return JsonConvert.SerializeObject(obj, _settings);
        }

        /// <summary>
        /// 格式化返回字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string PrettySerialize<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var serializer = new JsonSerializer {DateFormatString = "yyyy-MM-dd HH:mm:ss", ContractResolver = new CamelCasePropertyNamesContractResolver()};

            using (var textWriter = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                })
                {
                    serializer.Serialize(jsonWriter, obj);
                    return textWriter.ToString();
                }
            }
        }

        /// <summary>
        /// 将 json 字符串反序列化成对象
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Deserialize<T>(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object Deserialize([NotNull] string json, [NotNull] Type type)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            if (type == null) throw new ArgumentNullException(nameof(type));

            return JsonConvert.DeserializeObject(json, type);
        }

        internal static object DeserializeToObject([NotNull] this JToken token, [NotNull] Type type)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (type == null) throw new ArgumentNullException(nameof(type));

            var serializer = JsonSerializer.CreateDefault(_settings);
            return token.ToObject(type, serializer);
        }

        internal static bool MoveToContent([NotNull] this JsonReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            JsonToken t = reader.TokenType;
            while (t == JsonToken.None || t == JsonToken.Comment)
            {
                if (!reader.Read())
                {
                    return false;
                }

                t = reader.TokenType;
            }

            return true;
        }
        
        internal static bool IsPrimitiveToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return true;
                default:
                    return false;
            }
        }
    }
}