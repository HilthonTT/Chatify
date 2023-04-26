using Chatify.Helpers;
using ChatifyLibrary.DataAccess;
using ChatifyLibrary.DataAccess.AuditLogData;
using ChatifyLibrary.DataAccess.BanAppealData;
using ChatifyLibrary.DataAccess.BanData;
using ChatifyLibrary.DataAccess.CategoryData;
using ChatifyLibrary.DataAccess.ChannelData;
using ChatifyLibrary.DataAccess.ConversationData;
using ChatifyLibrary.DataAccess.FriendRequestData;
using ChatifyLibrary.DataAccess.MessageData;
using ChatifyLibrary.DataAccess.PrivateConversationData;
using ChatifyLibrary.DataAccess.RoleData;
using ChatifyLibrary.DataAccess.ChannelCategoryData;
using ChatifyLibrary.DataAccess.ServerData;
using ChatifyLibrary.DataAccess.UserData;
using ChatifyLibrary.Helper;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Chatify;

public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor().AddMicrosoftIdentityConsentHandler();
        builder.Services.AddMemoryCache();
        builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

        builder.Services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = true;
            o.MaximumReceiveMessageSize = null;
        });

        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });
        });

        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
            {
                policy.RequireClaim("jobTitle", "Admin");
            });
        });

        builder.Services.AddSingleton<IDbConnection, DbConnection>();
        builder.Services.AddSingleton<IConversationData, MongoConversationData>();
        builder.Services.AddSingleton<IMessageData, MongoMessageData>();
        builder.Services.AddSingleton<IUserData, MongoUserData>();
        builder.Services.AddSingleton<ICategoryData, MongoCategoryData>();
        builder.Services.AddSingleton<IFriendRequestData, MongoFriendRequestData>();
        builder.Services.AddSingleton<IPrivateConversationData, MongoPrivateConversationData>();
        builder.Services.AddSingleton<IBanData, MongoBanData>();
        builder.Services.AddSingleton<IBanAppealData, MongoBanAppealData>();
        builder.Services.AddSingleton<ICachingHelper, CachingHelper>();
        builder.Services.AddSingleton<IServerData, MongoServerData>();
        builder.Services.AddSingleton<IChannelData, MongoChannelData>();
        builder.Services.AddSingleton<IRoleData, MongoRoleData>();
        builder.Services.AddSingleton<IAuditLogData, MongoAuditLogData>();
        builder.Services.AddSingleton<IChannelCategoryData, MongoChannelCategoryData>();

        builder.Services.AddSingleton<IOidGenerator, OidGenerator>();
        builder.Services.AddSingleton<OidGenerator>();
    }
}
