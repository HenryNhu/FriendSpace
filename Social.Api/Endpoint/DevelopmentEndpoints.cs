using Social.Infrastructure.Persistence;

namespace Social.Api.Endpoint
{
    public static class DevelopmentEndpoints
    {
        public static void MapDevelopmentEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/dev/database", async (SocialDbContext db) =>
            {
                var connected = await db.Database.CanConnectAsync();

                if (connected)
                {
                    return Results.Ok(new
                    {
                        message = "Kết nối PostgreSQL thành công."
                    });
                }

                return Results.Problem(detail: "Chưa kết nối được PostgreSQL.", statusCode: 503);
            });
        }
    }
}
