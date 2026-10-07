using FriendSpace.Web.Components;
using FriendSpace.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddHttpClient("SocialApi", client =>
    {
        var baseUrl = builder.Configuration["SocialApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Chưa cấu hình SocialApi:BaseUrl.");

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(15);
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            UseCookies = false
        });

builder.Services.AddScoped<SocialApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
