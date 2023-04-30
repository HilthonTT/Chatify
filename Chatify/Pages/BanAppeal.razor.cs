using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages;

public partial class BanAppeal
{
    [Parameter]
    public string Id { get; set; }

    private UserModel loggedInUser;
    private CreateBanAppealModel appeal = new();
    private BanModel ban;
    private BanAppealModel banAppeal;
    private string banAppealId = "";
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        ban = await banData.GetBanAsync(Id);
        if (ban is not null)
        {
            banAppeal = await appealData.GetBanAppealFromBan(ban);
        }

        if (ban?.UserBanned.Id != loggedInUser?.Id)
            navManager.NavigateTo("/");
    }

    private async Task CreateBanAppeal()
    {
        BanAppealModel b = new()
        {
            Ban = ban,
            AppealingUser = new BasicUserModel(loggedInUser),
            AppealReason = appeal.AppealReason,
            IsApproved = false
        };
        if (ban is null)
            return;
        await appealData.CreateBanAppeal(b);
        appeal = new();
        ClosePage();
    }

    private async Task ReCreateBanAppeal()
    {
        await appealData.DeleteAppeal(banAppeal);
        banAppeal = null;
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/");
    }
}