using ChatifyLibrary.DataAccess.UserData;
using ChatifyLibrary.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Chatify.Helpers;

public static class AuthenticationStateProviderHelpers
{
    public static async Task<UserModel> GetUserFromAuth(
            this AuthenticationStateProvider provider,
            IUserData userData)
    {
        var authState = await provider.GetAuthenticationStateAsync();
        string objectId = authState.User.Claims.FirstOrDefault(c => c.Type.Contains("objectidentifier"))?.Value;
        return await userData.GetUserFromAuthenticationAsync(objectId);
    }
}
