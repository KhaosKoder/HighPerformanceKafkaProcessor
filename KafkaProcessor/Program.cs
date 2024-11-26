using HighPerformanceKafkaProcessor;
using HighPerformanceKafkaProcessor.Configuration;
using HighPerformanceKafkaProcessor.Handlers;
using HighPerformanceKafkaProcessor.Producers;
using HighPerformanceKafkaProcessor.Management;
using HighPerformanceKafkaProcessor.Api;
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

        // Register Core Services
        services.AddSingleton<DeadLetterProducer>();
        services.AddSingleton<ITopicGroupManager, TopicGroupManager>();

        // Register Handlers
        services.AddScoped<JObjectToEventMessageMappingHandler>();
        services.AddScoped<PreprocessingHandler>();
        services.AddScoped<ProcessingHandler>();
        services.AddScoped<PostprocessingHandler>();
        services.AddScoped<PersistenceHandler>();
        services.AddScoped<PublishAlertHandler>();

        // Optional: Add API support if enabled in configuration
        if (context.Configuration.GetValue<bool>("EnableKafkaManagementApi"))
        {
            services.AddKafkaManagementApi();
        }
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.Configure(app =>
        {
            // Only configure API middleware if enabled
            if (app.ApplicationServices.GetRequiredService<IConfiguration>()
                .GetValue<bool>("EnableKafkaManagementApi"))
            {
                app.UseRouting();
                app.UseKafkaManagementApi(); // This includes Swagger setup
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
        });
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

    // Get the TopicGroupManager and start enabled groups
    var topicGroupManager = host.Services.GetRequiredService<ITopicGroupManager>();
    await topicGroupManager.StartEnabledGroupsAsync();

    logger.LogInformation("Kafka topic groups started successfully");

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
    // Ensure all groups are stopped
    try
    {
        var topicGroupManager = host.Services.GetRequiredService<ITopicGroupManager>();
        await topicGroupManager.StopAllGroupsAsync();
    }
    catch (Exception ex)
    {
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error stopping Kafka topic groups");
    }

    Log.CloseAndFlush();
}