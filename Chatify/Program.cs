using Chatify;
using Chatify.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.ConfigureServices();

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<ChatHub>("/chathub");
app.MapHub<TestHub>("/testhub");

app.MapFallbackToPage("/_Host");

app.Run();
