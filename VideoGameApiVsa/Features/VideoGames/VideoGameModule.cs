using Carter;

namespace VideoGameApiVsa.Features.VideoGames;

/// <summary>
/// Video Gamesに関連するすべてのエンドポイントを管理するモジュール
/// </summary>
public sealed class VideoGamesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/games")
            .WithTags("Games");

        // GetAll 
        group.MapGet("/", GetAllGames.Endpoint)
            .WithName(VideoGameRouteNames.GetAll)
            //.WithSummary("Get all video games")
            .WithDescription("Retrieves a list of all video games in the database")
            .Produces<IEnumerable<GetAllGames.Response>>(StatusCodes.Status200OK);

        // GetByID
        group.MapGet("/{id:int}", GetGameById.Endpoint)
            .WithName(VideoGameRouteNames.GetById)
            //.WithSummary("Get a video game by ID")
            .WithDescription("Retrieves a specific video game by its ID")
            .Produces<GetGameById.Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // Create
        group.MapPost("/", CreateGame.Endpoint)
            .WithName(VideoGameRouteNames.Create)
            //.WithSummary("Create a new video game")
            .WithDescription("Creates a new video game entry in the database")
            .Produces<CreateGame.Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status400BadRequest);

        // Update
        group.MapPut("/{id:int}", UpdateGame.Endpoint)
            .WithName(VideoGameRouteNames.Update)
            //.WithSummary("Update an existing video game")
            .WithDescription("Updates an existing video game by its ID")
            .Produces<UpdateGame.Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status400BadRequest);

        // Delete
        group.MapDelete("/{id:int}", DeleteGame.Endpoint)
            .WithName(VideoGameRouteNames.Delete)
            //.WithSummary("Delete a video game")
            .WithDescription("Deletes a video game by its ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
