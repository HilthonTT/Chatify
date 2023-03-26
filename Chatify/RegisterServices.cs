using ChatifyLibrary.DataAccess;
using Microsoft.AspNetCore.ResponseCompression;

namespace Chatify;

public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddMemoryCache();
        builder.Services.AddControllersWithViews();

        builder.Services.AddSignalR();

        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });
        });

        builder.Services.AddSingleton<IDbConnection, DbConnection>();
        builder.Services.AddSingleton<IConversationData, MongoConversationData>();
        builder.Services.AddSingleton<IMessageData, MongoMessageData>();
        builder.Services.AddSingleton<IUserData, MongoUserData>();
        builder.Services.AddSingleton<ICategoryData, MongoCategoryData>();
        builder.Services.AddSingleton<IFriendRequestData, MongoFriendRequestData>();
    }
}
