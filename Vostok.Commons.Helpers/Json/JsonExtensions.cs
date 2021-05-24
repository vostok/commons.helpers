using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vostok.Commons.Helpers.Json
{
    [PublicAPI]
    internal static class JsonExtensions
    {
        private static readonly IList<JsonConverter> Converters = new List<JsonConverter>
        {
            new StringEnumConverter(),
            new VersionConverter(),
            new ComplexDictionaryJsonConverter(),
        };

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = Converters,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        private static readonly ThreadLocal<bool> HasDeserializationError = new ThreadLocal<bool>(() => false);

        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault(Settings);

        public static string ToPrettyJson(this object @object) => JsonConvert.SerializeObject(@object, Newtonsoft.Json.Formatting.Indented, Settings);

        public static string ToJson(this object @object) => JsonConvert.SerializeObject(@object, Settings);

        [CanBeNull]
        public static T FromJson<T>(this string serialized) => JsonConvert.DeserializeObject<T>(serialized, Settings);

        [CanBeNull]
        public static object FromJson(this string serialized, Type type) => JsonConvert.DeserializeObject(serialized, type, Settings);

        [CanBeNull]
        public static object FromJson(this string serialized) => JsonConvert.DeserializeObject(serialized, Settings);
    }
}