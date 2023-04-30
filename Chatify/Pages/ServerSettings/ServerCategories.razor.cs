using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;
using Chatify.Helpers;
using Chatify.Models;

namespace Chatify.Pages.ServerSettings;

public partial class ServerCategories
{
    [Parameter]
    public string Id { get; set; }

    private CreateChannelCategoryModel editingCategory = new();
    private ServerModel server;
    private UserModel loggedInUser;
    private BanModel ban;
    private ChannelCategoryModel selectedCategory;
    private ChannelCategoryModel archivingCategory;
    private RoleModel role;
    private List<ChannelCategoryModel> categories;
    private string searchText = "";
    private bool isSortedByNew = true;
    protected override async Task OnInitializedAsync()
    {
        server = await serverData.GetServerAsync(Id);
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        if (server is not null)
        {
            categories = await channelCategoryData.GetServerCategoriesAsync(server);
        }

        if (loggedInUser is not null)
        {
            ban = await banData.GetUserBanActive(loggedInUser.Id);
        }

        if (loggedInUser is not null && server is not null)
        {
            role = await roleData.GetUserServerRoleAsync(loggedInUser, server);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadFilterState();
            if (server is not null)
            {
                await FilterChannelCategories();
            }

            StateHasChanged();
        }
    }

    private async Task LoadFilterState()
    {
        var boolResults = await sessionStorage.GetAsync<bool>(nameof(isSortedByNew));
        isSortedByNew = boolResults.Success ? boolResults.Value : true;
        var stringResults = await sessionStorage.GetAsync<string>(nameof(searchText));
        searchText = stringResults.Success ? stringResults.Value : "";
    }

    private async Task SaveFilterState()
    {
        await sessionStorage.SetAsync(nameof(isSortedByNew), isSortedByNew);
        await sessionStorage.SetAsync(nameof(searchText), searchText);
    }

    private async Task FilterChannelCategories()
    {
        var output = await channelCategoryData.GetServerCategoriesAsync(server);
        if (string.IsNullOrWhiteSpace(searchText)is false)
        {
            output = output.Where(c => c.CategoryName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || c.CategoryDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        if (isSortedByNew)
        {
            output = output.OrderByDescending(c => c.DateCreated).ToList();
        }
        else
        {
            output = output.OrderBy(c => c.DateCreated).ToList();
        }

        categories = output;
        await SaveFilterState();
    }

    private async Task OnSearchInput(string searchInput)
    {
        searchText = searchInput;
        await FilterChannelCategories();
    }

    private async Task SortedByNew(bool isNew)
    {
        isSortedByNew = isNew;
        await FilterChannelCategories();
    }

    private async Task CloseModal()
    {
        await JSRuntime.InvokeVoidAsync("closeModal", "categoryModal");
    }

    private async Task UpdateCategory()
    {
        string oldCategoryName = selectedCategory.CategoryName;
        string oldCategoryDescription = selectedCategory.CategoryDescription;
        selectedCategory.CategoryName = editingCategory.CategoryName;
        selectedCategory.CategoryDescription = editingCategory.CategoryDescription;
        AuditLogModel a = new()
        {
            Name = "Update Channel",
            Description = $"{loggedInUser.DisplayName} has updated channel {oldCategoryName} to {selectedCategory.CategoryName}",
            OldValues = $"Channel Name: {oldCategoryName}, Channel Description: {oldCategoryName}",
            NewValues = $"Channel Name: {selectedCategory.CategoryName}, Channel Description: {selectedCategory.CategoryDescription}",
            User = new BasicUserModel(loggedInUser),
            Server = new BasicServerModel(server),
            DateModified = DateTime.UtcNow,
        };
        await channelCategoryData.UpdateCategory(selectedCategory);
        await auditLogData.CreateAuditLog(a);
        selectedCategory = null;
        editingCategory = new();
        await CloseModal();
    }

    private async Task ArchiveCategory()
    {
        archivingCategory.Archived = true;
        await channelCategoryData.UpdateCategory(archivingCategory);
        categories.Remove(categories.FirstOrDefault(c => c.Id == archivingCategory.Id));
        await CloseModal();
    }

    private void ClosePage()
    {
        navManager.NavigateTo($"/ServerSettings/{server.Id}");
    }

    private void OpenDetails(ChannelCategoryModel model)
    {
        navManager.NavigateTo($"/ServerSettings/ChannelCategory/{model.Id}");
    }

    private void LoadSelectedCategory(ChannelCategoryModel model)
    {
        selectedCategory = model;
        editingCategory.CategoryName = model.CategoryName;
        editingCategory.CategoryDescription = model.CategoryDescription;
    }

    private string SortedByNewClass(bool isNew)
    {
        if (isSortedByNew == isNew)
        {
            return "btn-secondary";
        }

        return "btn-outline-secondary";
    }

    private bool IsUserOwner()
    {
        if (loggedInUser is null || server is null)
        {
            return false;
        }

        if (loggedInUser.Id == server.Owner.Id)
        {
            return true;
        }

        return false;
    }

    private bool CanCreateChannel()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (role is not null && role.CanCreateChannel)
        {
            return true;
        }

        return false;
    }

    private bool HasAnyPermissions()
    {
        if (IsUserOwner())
        {
            return true;
        }

        if (CanCreateChannel())
        {
            return true;
        }

        return false;
    }
}