using Microsoft.EntityFrameworkCore;
using Social.Api.Authentication;
using Social.Api.Contracts;
using Social.Api.Endpoint;
using Social.Application.Abstractions;
using Social.Application.Comments;
using Social.Application.PostLikes;
using Social.Application.Posts;
using Social.Application.UserProfiles;
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

builder.Services.AddFriendSpaceAuthentication();
builder.Services.AddBrowserSecurity();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<IPostLikeRepository, PostLikeRepository>();
builder.Services.AddScoped<PostLikeService>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<UserProfileService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseBrowserCsrf();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevelopmentEndpoints();
}

// Add Endpoint 
app.MapAuthEndpoints();
app.MapPostEndpoints();
app.MapCommentEndpoints();
app.MapPostLikeEndpoints();
app.MapUserProfileEndpoints();

app.Run();