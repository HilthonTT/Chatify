using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using ChatifyLibrary.Models;
using Chatify.Helpers;

namespace Chatify.Pages.Admin;

public partial class Bans
{
    private UserModel loggedInUser;
    private List<UserModel> users;
    private List<BanModel> bans;
    private string searchBanText = "";
    private string searchUserText = "";
    private string selectedUsername = "";
    private bool isSortedByNew = true;
    private bool showUsernames = false;
    protected override async Task OnInitializedAsync()
    {
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        bans = await banData.GetAllBansAsync();
        users = await userData.GetAllUsersCachedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            await FilterBans();
            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchBanText));
        searchBanText = stringResults.Success ? stringResults.Value : "";
        stringResults = await sessionStorage.GetAsync<string>(nameof(searchUserText));
        searchUserText = stringResults.Success ? stringResults.Value : "";
        stringResults = await sessionStorage.GetAsync<string>(nameof(selectedUsername));
        selectedUsername = stringResults.Success ? stringResults.Value : "All";
        var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
        isSortedByNew = boolResults.Success ? boolResults.Value : true;
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(selectedUsername), selectedUsername);
        await sessionStorage.SetAsync(nameof(searchBanText), searchBanText);
        await sessionStorage.SetAsync(nameof(searchUserText), searchUserText);
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
    }

    private async Task FilterBans()
    {
        var output = await banData.GetAllBansAsync();
        if (selectedUsername != "All")
        {
            output = output.Where(b => b.UserBanned?.DisplayName == selectedUsername).ToList();
        }

        if (string.IsNullOrWhiteSpace(searchBanText)is false)
        {
            output = output.Where(b => b.Reason.Contains(searchBanText, StringComparison.InvariantCultureIgnoreCase) || b.UserBanned.DisplayName.Contains(searchBanText, StringComparison.InvariantCultureIgnoreCase) || b.UserBanned.Id.Contains(searchBanText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(a => a.BannedAt).ToList();
        }
        else
        {
            output = output.OrderByDescending(a => a.IsActive).ThenByDescending(a => a.BannedAt).ToList();
        }

        bans = output;
        await SaveFilterState();
    }

    private async Task FilterUsers()
    {
        var output = await userData.GetAllUsersCachedAsync();
        if (string.IsNullOrWhiteSpace(searchUserText)is false)
        {
            output = output.Where(u => u.DisplayName.Contains(searchUserText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        users = output;
        await SaveFilterState();
    }

    private async Task OnUsernameClick(string username = "All")
    {
        selectedUsername = username;
        showUsernames = false;
        await FilterBans();
    }

    private async Task OrderByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterBans();
    }

    private async Task OnBanSearchInput(string searchInput)
    {
        searchBanText = searchInput;
        await FilterBans();
    }

    private async Task OnUserSearchInput(string searchInput)
    {
        searchUserText = searchInput;
        await FilterUsers();
    }

    private void OpenDetails(BanModel ban)
    {
        navManager.NavigateTo($"/BanDetails/{ban.Id}");
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isNew == isSortedByNew)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private string SelectedUsernameClass(string username = "All")
    {
        if (username == selectedUsername)
        {
            return "bg-secondary";
        }

        return "bg-dark";
    }
}