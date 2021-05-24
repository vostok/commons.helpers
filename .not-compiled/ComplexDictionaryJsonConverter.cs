using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vostok.Commons.Helpers.Json
{
    [PublicAPI]
    internal class ComplexDictionaryJsonConverter : JsonConverter
    {
        private const string KeyFieldName = "Key";
        private const string ValueFieldName = "Value";

        private static readonly MethodInfo WriteJsonInternalMethod = typeof(ComplexDictionaryJsonConverter).GetMethod(nameof(WriteJsonInternal), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo ReadJsonInternalMethod = typeof(ComplexDictionaryJsonConverter).GetMethod(nameof(ReadJsonInternal), BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly HashSet<Type> PrimitiveKeyTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(char),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(string),
            typeof(Guid)
        };

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                JValue.CreateNull().WriteTo(writer);

            var args = FindDictionaryGenericArguments(value.GetType()) ?? throw new InvalidOperationException();
            var serialized = (JArray)WriteJsonInternalMethod.MakeGenericMethod(args.Key, args.Value).Invoke(null, new[] {value, serializer});

            serialized.WriteTo(writer);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var array = JArray.Load(reader);

            var args = FindDictionaryGenericArguments(objectType) ?? throw new InvalidOperationException();
            return ReadJsonInternalMethod.MakeGenericMethod(args.Key, args.Value).Invoke(null, new object[] {array, serializer, objectType});
        }

        public override bool CanConvert(Type objectType)
        {
            var arguments = FindDictionaryGenericArguments(objectType);
            return arguments != null && !PrimitiveKeyTypes.Contains(arguments.Value.Key);
        }

        private static JArray WriteJsonInternal<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> dict, JsonSerializer serializer)
        {
            var result = new JArray();

            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                
                var pair = new JObject
                {
                    [KeyFieldName] = ReferenceEquals(key, null) ? JValue.CreateNull() : JToken.FromObject(key, serializer),
                    [ValueFieldName] = ReferenceEquals(value, null) ? JValue.CreateNull() : JToken.FromObject(value, serializer)
                };
                result.Add(pair);
            }

            return result;
        }

        private static object ReadJsonInternal<TKey, TValue>(JArray array, JsonSerializer serializer)
        {
            return ReadJsonDict(new Dictionary<TKey, TValue>(), (dict, key, value) => dict.Add(key, value));

            TDict ReadJsonDict<TDict>(TDict emptyDict, Action<TDict, TKey, TValue> addPair)
                where TDict : class
            {
                var result = emptyDict;

                foreach (var pair in array.Select(p => (JObject)p))
                {
                    if (pair[KeyFieldName] == null || pair[ValueFieldName] == null)
                        continue;
                    
                    var key = pair[KeyFieldName].ToObject<TKey>(serializer);
                    var value = pair[ValueFieldName].ToObject<TValue>(serializer);
                    addPair(result, key, value);
                }

                return result;
            }
        }

        private (Type Key, Type Value)? FindDictionaryGenericArguments(Type type)
        {
            var genericDictionaryInterface = IsDictionaryInterface(type)
                ? type
                : type.GetInterfaces().FirstOrDefault(IsDictionaryInterface);

            if (genericDictionaryInterface == null)
                return null;

            var arguments = genericDictionaryInterface.GetGenericArguments();
            return (arguments[0], arguments[1]);
        }

        private bool IsDictionaryInterface(Type @interface)
        {
            if (!@interface.IsInterface || !@interface.IsGenericType)
                return false;

            var genericDefinition = @interface.GetGenericTypeDefinition();
            return genericDefinition == typeof(IDictionary<,>) || genericDefinition == typeof(IReadOnlyDictionary<,>);
        }
    }
}