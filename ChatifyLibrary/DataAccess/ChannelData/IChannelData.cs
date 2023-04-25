namespace ChatifyLibrary.DataAccess.ChannelData;

public interface IChannelData
{
    Task CreateChannel(ChannelModel channel);
    Task<ChannelModel> CreateChannelAndReturn(ChannelModel channel);
    Task<List<ChannelModel>> GetAllChannelsAsync();
    Task<List<ChannelModel>> GetAllChannelsCategoryAsync(ChannelCategoryModel category);
    Task<List<ChannelModel>> GetAllChannelsServerAsync(ServerModel server);
    Task<ChannelModel> GetChannelAsync(string id);
    Task<ChannelModel> GetChannelObjectIdAsync(string objectId);
    Task UpdateChannel(ChannelModel channel);
}