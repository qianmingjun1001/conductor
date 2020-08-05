using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Conductor.Domain.Models;
using Newtonsoft.Json;

namespace Conductor.Domain.Utils
{
    /// <summary>
    /// Converts an <see cref="AnyObject"/> to and from JSON.
    /// </summary>
    public class AnyObjectConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // can write is set to false
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadValue(reader);
        }

        private object ReadValue(JsonReader reader)
        {
            if (!reader.MoveToContent())
            {
                throw new JsonSerializationException("Unexpected end when reading AnyObject.");
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadList(reader);
                default:
                    if (JsonUtils.IsPrimitiveToken(reader.TokenType))
                    {
                        return reader.Value;
                    }

                    throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Unexpected token when converting ExpandoObject: {0}", reader.TokenType));
            }
        }

        private object ReadList(JsonReader reader)
        {
            IList<object> list = new List<object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    default:
                        object v = ReadValue(reader);

                        list.Add(v);
                        break;
                    case JsonToken.EndArray:
                        return list;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading AnyObject.");
        }

        private object ReadObject(JsonReader reader)
        {
            var expandoObject = new AnyObject();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException("Unexpected end when reading AnyObject.");
                        }

                        object v = ReadValue(reader);

                        expandoObject[propertyName] = v;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return expandoObject;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading AnyObject.");
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(AnyObject));
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite => false;
    }
}