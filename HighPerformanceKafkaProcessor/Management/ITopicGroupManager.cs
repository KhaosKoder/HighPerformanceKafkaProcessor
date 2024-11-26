using HighPerformanceKafkaProcessor.Configuration;

namespace HighPerformanceKafkaProcessor.Management
{
    public interface ITopicGroupManager
    {
        Task<bool> StartGroupAsync(string groupName);
        Task<bool> StopGroupAsync(string groupName);
        Task<TopicGroupStatus> GetGroupStatusAsync(string groupName);
        Task<IEnumerable<TopicGroupStatus>> GetAllGroupStatusesAsync();
        Task StartEnabledGroupsAsync();
        Task StopAllGroupsAsync();
    }
}