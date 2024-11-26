using KafkaFlow;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using HighPerformanceKafkaProcessor.Models;

namespace HighPerformanceKafkaProcessor.Handlers
{
    public class JObjectToEventMessageMappingHandler : IMessageHandler<JObject>
    {
        private readonly ILogger<JObjectToEventMessageMappingHandler> _logger;

        public JObjectToEventMessageMappingHandler(ILogger<JObjectToEventMessageMappingHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(IMessageContext context, JObject message)
        {
            _logger.LogInformation("Mapping message");
            try
            {
                // Create new EventMessage
                var eventMessage = new EventMessage();

                // Get first 3 properties
                var properties = message.Properties().Take(3).ToList();

                // Copy them to the new EventMessage
                foreach (var prop in properties)
                {
                    eventMessage[prop.Name] = prop.Value;
                    _logger.LogDebug("Copied property: {PropertyName} = {PropertyValue}", prop.Name, prop.Value);
                }

                // Store the continue processing flag
                context.Items.TryAdd("ContinueProcessing", true);

                // Update the message in the context
                context.SetMessage(context.Message.Key, eventMessage);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping JObject to EventMessage");
                context.Items.TryAdd("ContinueProcessing", false);
                return Task.CompletedTask;
            }
        }
    }
}