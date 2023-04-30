using Microsoft.AspNetCore.Components;
using ChatifyLibrary.Models;
using Chatify.Helpers;

namespace Chatify.Pages.User;

public partial class Users
{
    [Parameter]
    public string SearchText { get; set; }

    private UserModel loggedInUser;
    private List<UserModel> users;
    private BanModel ban;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await FilterUsers();
            StateHasChanged();
        }
    }

    private async Task FilterUsers()
    {
        var output = await userData.GetAllUsersCachedAsync();
        if (string.IsNullOrWhiteSpace(SearchText)is false)
        {
            output = output.Where(u => u.DisplayName.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        users = output;
    }

    private async Task OnSearchInput(string searchInput)
    {
        SearchText = searchInput;
        await FilterUsers();
    }

    private void OpenDetails(UserModel user)
    {
        if (loggedInUser?.Id == user.Id)
        {
            navManager.NavigateTo("/Profile");
            return;
        }

        navManager.NavigateTo($"/UserDetails/{user.Id}");
    }
}