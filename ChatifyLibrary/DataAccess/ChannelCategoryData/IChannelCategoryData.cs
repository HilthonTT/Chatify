namespace ChatifyLibrary.DataAccess.ChannelCategoryData;

public interface IChannelCategoryData
{
    Task CreateCategory(ChannelCategoryModel category);
    Task<ChannelCategoryModel> CreateCategoryAndReturn(ChannelCategoryModel category);
    Task<List<ChannelCategoryModel>> GetAllCategoriesAsync();
    Task<ChannelCategoryModel> GetCategoryAsync(string id);
    Task<ChannelCategoryModel> GetCategoryObjectIdAsync(string objectId);
    Task<List<ChannelCategoryModel>> GetServerCategoriesAsync(ServerModel server);
    Task UpdateCategory(ChannelCategoryModel category);
}