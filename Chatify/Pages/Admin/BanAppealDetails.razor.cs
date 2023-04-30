using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;

namespace Chatify.Pages.Admin;

public partial class BanAppealDetails
{
    [Parameter]
    public string Id { get; set; }

    private UserModel loggedInUser;
    private BanModel ban;
    private BanAppealModel appeal;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        appeal = await appealData.GetBanAppealAsync(Id);
        if (appeal is not null)
        {
            ban = await banData.GetBanAsync(appeal.Ban.Id);
        }
    }

    private string CreateWebPath(string relativePath)
    {
        return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/BanAppeals");
    }

    private async Task ApproveAppeal()
    {
        appeal.IsApproved = true;
        appeal.ApprovedAt = DateTime.UtcNow;
        appeal.ApprovedAdmin = new BasicUserModel(loggedInUser);
        if (ban is not null)
        {
            ban.IsActive = false;
            await banData.UpdateBan(ban);
        }

        await appealData.UpdateAppeal(appeal);
        ClosePage();
    }

    private async Task DisapproveAppeal()
    {
        appeal.IsApproved = false;
        appeal.DisapprovedAdmin = new BasicUserModel(loggedInUser);
        appeal.DisapprovedAt = DateTime.UtcNow;
        await appealData.UpdateAppeal(appeal);
        if (ban is not null)
        {
            ban.IsActive = true;
            await banData.UpdateBan(ban);
        }

        ClosePage();
    }
}