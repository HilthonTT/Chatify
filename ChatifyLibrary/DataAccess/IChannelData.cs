namespace ChatifyLibrary.DataAccess
{
    public interface IChannelData
    {
        Task CreateChannel(ChannelModel channel);
        Task<List<ChannelModel>> GetAllChannelsAsync();
        Task<ChannelModel> GetChannelAsync(string id);
        Task UpdateChannel(ChannelModel channel);
    }
}