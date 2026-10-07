using Microsoft.EntityFrameworkCore;
using Social.Api.Contracts;
using Social.Api.Endpoint;
using Social.Application.Abstractions;
using Social.Application.Comments;
using Social.Application.Posts;
using Social.Infrastructure.Identity;
using Social.Infrastructure.Persistence;
using Social.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("PostgreSQL_Database") ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:PostgreSQL_Database.");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SocialDbContext>(options => options.UseNpgsql(connectionString));
builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<SocialDbContext>();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<CommentService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevelopmentEndpoints();
}

app.MapAuthEndpoints();
app.MapPostEndpoints();
app.MapCommentEndpoints();

app.Run();