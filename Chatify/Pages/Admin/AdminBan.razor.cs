using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.Admin;

public partial class AdminBan
{
    [Parameter]
    public string Id { get; set; }

    private CreateBanModel ban = new();
    private UserModel loggedInUser;
    private UserModel user;
    private string userId = "";
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        user = await userData.GetUserAsync(Id);
        ban.BannedUntil = DateTime.UtcNow;
    }

    private void ClosePage()
    {
        navManager.NavigateTo($"/UserDetails/{user.Id}");
    }

    private async Task CreateBan()
    {
        BanModel b = new()
        {
            UserBanned = new BasicUserModel(user),
            BannedAt = DateTime.UtcNow,
            BannedUntil = ban.BannedUntil,
            Reason = ban.Reason,
            Admin = new BasicUserModel(loggedInUser),
            IsActive = true,
        };
        await banData.CreateBan(b);
        ban = new();
        ClosePage();
    }
}