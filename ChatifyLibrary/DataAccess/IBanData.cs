namespace ChatifyLibrary.DataAccess
{
    public interface IBanData
    {
        Task CreateBan(BanModel ban);
        Task<List<BanModel>> GetAllBansAsync();
        Task<BanModel> GetBanAsync(string id);
        Task<BanModel> GetUserBanActive(string userId);
        Task UpdateBan(BanModel ban);
    }
}