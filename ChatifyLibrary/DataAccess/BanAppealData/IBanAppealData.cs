namespace ChatifyLibrary.DataAccess.BanAppealData;

public interface IBanAppealData
{
    Task CreateBanAppeal(BanAppealModel appeal);
    Task DeleteAppeal(BanAppealModel appeal);
    Task<List<BanAppealModel>> GetAllBanAppealsAsync();
    Task<BanAppealModel> GetBanAppealAsync(string id);
    Task<BanAppealModel> GetBanAppealFromBan(BanModel ban);
    Task UpdateAppeal(BanAppealModel appeal);
}