using HighPerformanceKafkaProcessor;
using HighPerformanceKafkaProcessor.Configuration;
using HighPerformanceKafkaProcessor.Handlers;
using HighPerformanceKafkaProcessor.Producers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Configure Options
        services.Configure<KafkaOptions>(
            context.Configuration.GetSection("KafkaOptions"));

        // Register Services
        services.AddSingleton<DeadLetterProducer>();

        // Register Handlers
        services.AddTransient<JObjectToEventMessageMappingHandler>();
        services.AddTransient<PreprocessingHandler>();
        services.AddTransient<ProcessingHandler>();
        services.AddTransient<PostprocessingHandler>();
        services.AddTransient<PersistenceHandler>();
        services.AddTransient<PublishAlertHandler>();

        // Register KafkaProcessor
        services.AddSingleton<KafkaProcessor>();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();

        // Add console logging
        logging.AddConsole();

        // Configure minimum log levels
        logging.SetMinimumLevel(LogLevel.Information);

        // Configure category-specific log levels
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("System", LogLevel.Warning);
        logging.AddFilter("HighPerformanceKafkaProcessor", LogLevel.Debug);
    });

// Build and run the host
var host = builder.Build();

try
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting application");

    // Get the KafkaProcessor and start it
    var kafkaProcessor = host.Services.GetRequiredService<KafkaProcessor>();
    await kafkaProcessor.StartAsync();

    logger.LogInformation("KafkaProcessor started successfully");

    // Wait for the host to shutdown
    await host.RunAsync();
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}