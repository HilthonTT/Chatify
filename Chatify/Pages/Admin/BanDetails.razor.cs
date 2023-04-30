using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.Admin;

public partial class BanDetails
{
    [Parameter]
    public string Id { get; set; }

    private const string TrueString = "True";
    private const string FalseString = "False";
    private CreateBanModel model = new();
    private UserModel loggedInUser;
    private BanModel ban;
    private bool IsActive = false;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        ban = await banData.GetBanAsync(Id);
        if (ban is not null)
        {
            model.Reason = ban.Reason;
            model.BannedUntil = ban.BannedUntil;
            IsActive = ban.IsActive;
        }
    }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", "banModal");
    }

    private async Task UpdateBan()
    {
        ban.Reason = model.Reason;
        ban.BannedUntil = model.BannedUntil;
        ban.IsActive = IsActive;
        BanModel b = new()
        {
            Id = ban.Id,
            Reason = ban.Reason,
            UserBanned = ban.UserBanned,
            BannedAt = ban.BannedAt,
            BannedUntil = ban.BannedUntil.AddDays(1),
            Admin = ban.Admin,
            IsActive = ban.IsActive,
        };
        // Added an extra day on BannedUntil field
        // because MongoDB strangely takes the day before the selected one in the InputDate.
        await banData.UpdateBan(b);
        await CloseModal();
    }

    private void ClosePage()
    {
        navManager.NavigateTo("/Bans");
    }
}