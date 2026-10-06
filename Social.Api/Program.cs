using Microsoft.EntityFrameworkCore;
using Social.Api.Contracts;
using Social.Api.Endpoint;
using Social.Application.Abstractions;
using Social.Application.Posts;
using Social.Infrastructure.Persistence;
using Social.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("PostgreSQL_Database") ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:PostgreSQL_Database.");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SocialDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<PostService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapDevelopmentEndpoints();
}

app.UseHttpsRedirection();

app.MapPostEndpoints();

app.Run();