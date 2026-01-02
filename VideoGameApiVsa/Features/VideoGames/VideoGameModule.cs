using Carter;

namespace VideoGameApiVsa.Features.VideoGames;

public sealed class VideoGamesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/Games")
            .WithTags("Games");

        group.MapGet("/", GetAllGames.Endpoint);
        group.MapGet("/{id:int}", GetGameById.Endpoint).WithName("GetGameById");
        group.MapPost("/", CreateGame.Endpoint);
        group.MapPut("/{id:int}", UpdateGame.Endpoint);
        group.MapDelete("/{id:int}", DeleteGame.Endpoint);
    }
}
