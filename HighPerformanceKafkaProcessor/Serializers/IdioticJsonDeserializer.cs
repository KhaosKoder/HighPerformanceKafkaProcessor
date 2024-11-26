using KafkaFlow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Logging;

namespace HighPerformanceKafkaProcessor.Serializers
{
    /// <summary>
    /// This class deserializes Json Text into a NewtonSoft JObject.
    /// It then traverses the object, looking for String-properties that starts 
    /// with a square-bracket (indicating a json array) or a curly-bracket 
    /// (indicating a json object). If it finds either of those - it attempts to 
    /// parse the string and it replaces the string-property with the array or 
    /// object. 
    /// </summary>

    internal class IdioticJsonDeserializer : IDeserializer
    {
        private ILogger<IdioticJsonDeserializer> _logger;

        public IdioticJsonDeserializer(ILogger<IdioticJsonDeserializer> logger)
        {
            _logger = logger;
        }

        public async Task<object> DeserializeAsync(Stream input, Type type, ISerializerContext context)
        {
            try
            {
                using var reader = new StreamReader(input, Encoding.UTF8);
                string jsonString = await reader.ReadToEndAsync();

                var jObject = JObject.Parse(jsonString);

                ProcessStringProperties(jObject);

                return jObject;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void ProcessStringProperties(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var property in ((JObject)token).Properties().ToList())
                    {
                        if (property.Value.Type == JTokenType.String)
                        {
                            TryParseStringProperty(property);
                        }
                        else
                        {
                            ProcessStringProperties(property.Value);
                        }
                    }
                    break;

                case JTokenType.Array:
                    foreach (var item in ((JArray)token))
                    {
                        ProcessStringProperties(item);
                    }
                    break;
            }
        }

        private void TryParseStringProperty(JProperty property)
        {
            var stringValue = property.Value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(stringValue)) return;

            var firstChar = stringValue[0];
            if (firstChar != '{' && firstChar != '[') return;

            try
            {
                var parsedJson = JToken.Parse(stringValue);
                property.Value = parsedJson;
            }
            catch (JsonReaderException)
            {
                // Not valid JSON, keep original string value
            }
        }
    }
}
