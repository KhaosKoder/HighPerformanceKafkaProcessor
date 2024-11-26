using KafkaFlow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text;

namespace HighPerformanceKafkaProcessor.Serializers
{
    /// <summary>
    /// This deserializer will simply load the json text into the JObject.
    /// Nothing fancy - just load it. 
    /// </summary>
    internal class SimpleJsonDeserializer : IDeserializer
    {
        private ILogger<SimpleJsonDeserializer> _logger;

        public SimpleJsonDeserializer(ILogger<SimpleJsonDeserializer> logger)
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

                return jObject;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
