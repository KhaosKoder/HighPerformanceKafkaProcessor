# High Performance Kafka Processor

A high-performance .NET library for processing Kafka messages with support for middleware pipelines, flexible JSON deserialization, and robust error handling.

## Features

- **Configurable Message Processing Pipeline**: Pre-built middleware components for common processing needs
- **Smart JSON Deserialization**: Two deserialization strategies:
  - Simple: Direct JSON parsing
  - Idiotic: Advanced parsing that handles nested JSON strings within JSON objects
- **Performance Optimization**: Selective message processing with configurable selection
- **Error Handling**: Built-in dead letter queue support for failed messages
- **SSL Support**: Secure communication with Kafka brokers
- **Flexible Processing Pipeline**: Pre/post processing handlers with audit trail support
- **Multi-worker Support**: Configurable number of worker threads for parallel processing

## Installation

Add the package to your .NET project:

```bash
dotnet add package HighPerformanceKafkaProcessor
```

## Configuration

Configure the processor using the `KafkaOptions` class:

```csharp
services.Configure<KafkaOptions>(options => {
    options.Topics = new[] { "your-topic" };
    options.ConsumerGroup = "your-consumer-group";
    options.BootstrapServers = new[] { "localhost:9092" };
    options.BufferSize = 100;                    // Default: 100
    options.WorkersCount = 4;                    // Default: 1
    options.UseSsl = true;                       // Optional SSL config
    options.SslCertificateLocation = "path/to/cert";
    options.SslCaLocation = "path/to/ca";
    options.DeadLetterTopic = "dlq-topic";       // Default: "dead-letter-topic"
    options.SerializerType = "Idiotic";          // or "Simple"
});
```

## Usage

### Basic Setup

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register required services
        services.AddSingleton<IDeadLetterProducer, DeadLetterProducer>();
        services.AddScoped<KafkaProcessor>();
        
        // Configure options
        services.Configure<KafkaOptions>(Configuration.GetSection("Kafka"));
    }
}
```

### Starting the Processor

```csharp
public class YourService
{
    private readonly KafkaProcessor _processor;

    public YourService(KafkaProcessor processor)
    {
        _processor = processor;
    }

    public async Task StartProcessing()
    {
        await _processor.StartAsync();
    }

    public async Task StopProcessing()
    {
        await _processor.StopAsync();
    }
}
```

## Message Processing Pipeline

The library implements a sequential processing pipeline:

1. **Error Handling Middleware**: Catches exceptions and routes failed messages to DLQ
2. **Message Processing Determination**: Decides if a message should be processed (configurable sampling)
3. **Deserialization**: Converts message to JObject using selected deserializer
4. **Message Mapping**: Maps JObject to EventMessage
5. **Processing Stages**:
   - Preprocessing
   - Main Processing
   - Postprocessing
   - Persistence
   - Alert Publishing

## Custom Message Handlers

Implement custom handlers by implementing `IMessageHandler<T>`:

```csharp
public class CustomHandler : IMessageHandler<EventMessage>
{
    private readonly ILogger<CustomHandler> _logger;

    public CustomHandler(ILogger<CustomHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(IMessageContext context, EventMessage message)
    {
        // Your custom handling logic
        return Task.CompletedTask;
    }
}
```

## Performance Considerations

- The `DetermineShouldProcessMiddleware` allows for message sampling (default 30%)
- Buffer size and worker count are configurable for parallel processing
- The "Idiotic" deserializer handles nested JSON automatically but with performance overhead
- Use the "Simple" deserializer for better performance when nested JSON parsing isn't needed

## Error Handling

Failed messages are automatically:
1. Logged with full exception details
2. Published to the configured dead letter topic
3. Tracked in the processing audit trail

## Monitoring

The library uses standard .NET logging with structured logging support:
- Processing decisions
- Deserialization events
- Pipeline stage completion
- Error conditions
- Performance metrics (processing time in ticks)

## Contributing

We welcome contributions! Please submit pull requests with:
- Unit tests for new features
- Documentation updates
- Clear commit messages
- Code following existing style patterns

## License

[Your License Here]