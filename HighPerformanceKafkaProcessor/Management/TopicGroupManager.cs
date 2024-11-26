using System.Collections.Concurrent;
using KafkaFlow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HighPerformanceKafkaProcessor.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HighPerformanceKafkaProcessor.Middleware;
using HighPerformanceKafkaProcessor.Producers;
using HighPerformanceKafkaProcessor.Serializers;
using HighPerformanceKafkaProcessor.Handlers;
using Microsoft.Extensions.DependencyInjection;
using HighPerformanceKafkaProcessor.Models;
using Newtonsoft.Json.Linq;

namespace HighPerformanceKafkaProcessor.Management
{

    public class TopicGroupManager : ITopicGroupManager
    {
        private readonly ILogger<TopicGroupManager> _logger;
        private readonly KafkaOptions _options;
        private readonly ConcurrentDictionary<string, TopicGroupState> _groupStates;
        private readonly IServiceProvider _serviceProvider;

        private class TopicGroupState
        {
            public TopicGroupConfiguration Config { get; set; }
            public IKafkaBus Bus { get; set; }
            public bool IsRunning { get; set; }
            public DateTime? LastStartTime { get; set; }
            public DateTime? LastStopTime { get; set; }
            public string Error { get; set; }
        }

        public TopicGroupManager(
            IOptions<KafkaOptions> options,
            ILogger<TopicGroupManager> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _groupStates = new ConcurrentDictionary<string, TopicGroupState>();

            InitializeGroups();
        }

        private void InitializeGroups()
        {
            if (_options.TopicGroups == null) return;

            foreach (var groupConfig in _options.TopicGroups)
            {
                _groupStates[groupConfig.GroupName] = new TopicGroupState
                {
                    Config = groupConfig,
                    IsRunning = false
                };
            }
        }

        public async Task StartEnabledGroupsAsync()
        {
            foreach (var state in _groupStates.Values)
            {
                if (state.Config.EnabledOnStartup)
                {
                    await StartGroupInternalAsync(state);
                }
            }
        }

        public async Task<bool> StartGroupAsync(string groupName)
        {
            if (!_groupStates.TryGetValue(groupName, out var state))
            {
                _logger.LogWarning("Attempted to start non-existent group: {GroupName}", groupName);
                return false;
            }

            return await StartGroupInternalAsync(state);
        }

        private async Task<bool> StartGroupInternalAsync(TopicGroupState state)
        {
            try
            {
                if (state.IsRunning)
                {
                    _logger.LogInformation("Group {GroupName} is already running", state.Config.GroupName);
                    return true;
                }

                if (state.Bus == null)
                {
                    state.Bus = CreateKafkaBus(state.Config);
                }

                await state.Bus.StartAsync();

                state.IsRunning = true;
                state.LastStartTime = DateTime.UtcNow;
                state.Error = null;

                _logger.LogInformation("Successfully started group: {GroupName}", state.Config.GroupName);
                return true;
            }
            catch (Exception ex)
            {
                state.Error = ex.Message;
                _logger.LogError(ex, "Failed to start group: {GroupName}", state.Config.GroupName);
                return false;
            }
        }

        public async Task<bool> StopGroupAsync(string groupName)
        {
            if (!_groupStates.TryGetValue(groupName, out var state))
            {
                _logger.LogWarning("Attempted to stop non-existent group: {GroupName}", groupName);
                return false;
            }

            try
            {
                if (!state.IsRunning)
                {
                    _logger.LogInformation("Group {GroupName} is already stopped", state.Config.GroupName);
                    return true;
                }

                await state.Bus.StopAsync();
                state.IsRunning = false;
                state.LastStopTime = DateTime.UtcNow;
                state.Error = null;

                _logger.LogInformation("Successfully stopped group: {GroupName}", state.Config.GroupName);
                return true;
            }
            catch (Exception ex)
            {
                state.Error = ex.Message;
                _logger.LogError(ex, "Failed to stop group: {GroupName}", state.Config.GroupName);
                return false;
            }
        }

        public async Task StopAllGroupsAsync()
        {
            foreach (var groupName in _groupStates.Keys)
            {
                await StopGroupAsync(groupName);
            }
        }

        public Task<TopicGroupStatus> GetGroupStatusAsync(string groupName)
        {
            if (!_groupStates.TryGetValue(groupName, out var state))
            {
                return Task.FromResult<TopicGroupStatus>(null);
            }

            return Task.FromResult(CreateStatusFromState(state));
        }

        public Task<IEnumerable<TopicGroupStatus>> GetAllGroupStatusesAsync()
        {
            var statuses = _groupStates.Values.Select(CreateStatusFromState);
            return Task.FromResult(statuses);
        }

        private TopicGroupStatus CreateStatusFromState(TopicGroupState state)
        {
            return new TopicGroupStatus
            {
                GroupName = state.Config.GroupName,
                IsEnabled = state.Config.EnabledOnStartup,
                IsRunning = state.IsRunning,
                LastStartTime = state.LastStartTime,
                LastStopTime = state.LastStopTime,
                Topics = state.Config.Topics,
                Error = state.Error
            };
        }
        private IKafkaBus CreateKafkaBus(TopicGroupConfiguration config)
        {
            var services = new ServiceCollection();

            // Configure KafkaFlow
            services.AddKafka(kafka =>
            {
                kafka.AddCluster(cluster =>
                {
                    // Configure brokers
                    var clusterConfig = cluster.WithBrokers(config.BootstrapServers);

                    // Configure SSL if enabled
                    if (config.UseSsl)
                    {
                        clusterConfig.WithSecurityInformation(ssl =>
                        {
                            ssl.SslCertificateLocation = config.SslCertificateLocation;
                            ssl.SslCaLocation = config.SslCaLocation;
                        });
                    }

                    // Configure consumer
                    clusterConfig.AddConsumer(consumer => consumer
                        .Topics(config.Topics)
                        .WithGroupId(config.ConsumerGroup)
                        .WithBufferSize(config.BufferSize)
                        .WithWorkersCount(config.WorkersCount)
                        .AddMiddlewares(middlewares =>
                        {
                            // Add error handling with group information
                            middlewares.Add(resolver =>
                                new ErrorHandlingMiddleware(
                                    _serviceProvider.GetRequiredService<ILogger<ErrorHandlingMiddleware>>(),
                                    _serviceProvider.GetRequiredService<DeadLetterProducer>(),
                                    config.GroupName
                                )
                            );

                            // Add processing determination
                            middlewares.Add(resolver =>
                                new DetermineShouldProcessMiddleware(
                                    _serviceProvider.GetRequiredService<ILogger<DetermineShouldProcessMiddleware>>()
                                )
                            );

                            // Add the appropriate deserializer based on configuration
                            switch (config.SerializerType?.ToLowerInvariant())
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
                            middlewares.AddTypedHandlers(handlers =>
                            {
                                handlers
                                    .AddHandler<JObjectToEventMessageMappingHandler>()
                                    .AddHandler<PreprocessingHandler>()
                                    .AddHandler<ProcessingHandler>()
                                    .AddHandler<PostprocessingHandler>()
                                    .AddHandler<PersistenceHandler>()
                                    .AddHandler<PublishAlertHandler>();
                            });
                        })
                    );
                });
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.CreateKafkaBus();
        }






    }
}