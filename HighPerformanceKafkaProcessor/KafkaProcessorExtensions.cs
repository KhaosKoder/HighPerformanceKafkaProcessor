using Microsoft.Extensions.DependencyInjection;
using HighPerformanceKafkaProcessor.Configuration;
using HighPerformanceKafkaProcessor.Management;
using HighPerformanceKafkaProcessor.Producers;
using HighPerformanceKafkaProcessor.Handlers;
using Microsoft.AspNetCore.Builder;

namespace HighPerformanceKafkaProcessor
{
    public static class KafkaProcessorExtensions
    {
        public static IServiceCollection AddKafkaProcessor(
            this IServiceCollection services,
            Action<KafkaOptions> configureOptions)
        {
            // Add options
            services.Configure(configureOptions);

            // Register core services
            services.AddSingleton<DeadLetterProducer>();
            services.AddSingleton<ITopicGroupManager, TopicGroupManager>();

            // Register handlers
            services.AddScoped<JObjectToEventMessageMappingHandler>();
            services.AddScoped<PreprocessingHandler>();
            services.AddScoped<ProcessingHandler>();
            services.AddScoped<PostprocessingHandler>();
            services.AddScoped<PersistenceHandler>();
            services.AddScoped<PublishAlertHandler>();

            return services;
        }

        // Extension method to help with startup
        public static async Task UseKafkaProcessor(this IApplicationBuilder app)
        {
            // Get the topic group manager and start enabled groups
            var groupManager = app.ApplicationServices.GetRequiredService<ITopicGroupManager>();
            await groupManager.StartEnabledGroupsAsync();
        }

        // Extension method to help with application shutdown
        public static async Task UseKafkaProcessorShutdown(this IApplicationBuilder app)
        {
            var groupManager = app.ApplicationServices.GetRequiredService<ITopicGroupManager>();
            await groupManager.StopAllGroupsAsync();
        }
    }
}