using KafkaFlow;
using KafkaFlow.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Configuration;
using HighPerformanceKafkaProcessor.Handlers;
using HighPerformanceKafkaProcessor.Middleware;
using HighPerformanceKafkaProcessor.Producers;
using HighPerformanceKafkaProcessor.Serializers;
using Microsoft.Extensions.Options;

namespace HighPerformanceKafkaProcessor
{
    public static class KafkaProcessorExtensions
    {
        public static IServiceCollection AddKafkaProcessor(
            this IServiceCollection services,
            Action<KafkaOptions> configureOptions)
        {
            // Add options configuration
            services.Configure(configureOptions);

            // Add DeadLetterProducer as singleton
            services.AddSingleton<DeadLetterProducer>();

            // Configure KafkaFlow
            services.AddKafka(kafka =>
            {
                kafka.AddCluster(cluster =>
                {
                    // Get options
                    var options = services.BuildServiceProvider()
                        .GetRequiredService<IOptions<KafkaOptions>>().Value;

                    // Configure brokers
                    var clusterConfig = cluster.WithBrokers(options.BootstrapServers);

                    // Configure SSL if enabled
                    if (options.UseSsl)
                    {
                        clusterConfig.WithSecurityInformation(ssl =>
                        {
                            ssl.SslCertificateLocation = options.SslCertificateLocation;
                            ssl.SslCaLocation = options.SslCaLocation;
                        });
                    }

                    // Configure consumer
                    clusterConfig.AddConsumer(consumer => consumer
                        .Topics(options.Topics)
                        .WithGroupId(options.ConsumerGroup)
                        .WithBufferSize(options.BufferSize)
                        .WithWorkersCount(options.WorkersCount)
                        .AddMiddlewares(middlewares =>
                        {
                            // Add common middlewares
                            middlewares.Add<ErrorHandlingMiddleware>()
                                     .Add<DetermineShouldProcessMiddleware>();

                            // Add the appropriate deserializer based on configuration
                            switch (options.SerializerType?.ToLowerInvariant())
                            {
                                case "simple":
                                    middlewares.AddSingleTypeDeserializer<string, SimpleJsonDeserializer>();
                                    break;

                                case "idiotic":
                                default:
                                    middlewares.AddSingleTypeDeserializer<string, IdioticJsonDeserializer>();
                                    break;
                            }

                            // Add handlers in processing order
                            middlewares.AddTypedHandlers(handlers => handlers
                                .AddHandler<JObjectToEventMessageMappingHandler>()
                                .AddHandler<PreprocessingHandler>()
                                .AddHandler<ProcessingHandler>()
                                .AddHandler<PostprocessingHandler>()
                                .AddHandler<PersistenceHandler>()
                                .AddHandler<PublishAlertHandler>()
                            );
                        })
                    );
                });
            });

            // Add KafkaProcessor as singleton
            services.AddSingleton<KafkaProcessor>();

            return services;
        }
    }
}