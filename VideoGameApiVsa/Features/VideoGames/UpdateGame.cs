using Carter;
using MediatR;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

public static class UpdateGame
{
    public record Command(int Id, string Title, string Genre, int ReleaseYear) : IRequest<Response?>;

    public record Response(int Id, string Title, string Genre, int ReleaseYear);

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<Command, Response?>
    {
        public async Task<Response?> Handle(Command request, CancellationToken cancellationToken)
        {
            var videoGame = await dbContext.VideoGames.FindAsync([request.Id], cancellationToken);
            if (videoGame is null)
            {
                return null;
            }

            videoGame.Title = request.Title;
            videoGame.Genre = request.Genre;
            videoGame.ReleaseYear = request.ReleaseYear;

            await dbContext.SaveChangesAsync(cancellationToken);
            return new Response(videoGame.Id, videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
        }
    }

    public class EndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("api/games/{id}", async (ISender sender, Command command, int id, CancellationToken cancellationToken) =>
            {
                var updatedGame = await sender.Send(command with { Id = id }, cancellationToken);
                return updatedGame is not null ? Results.Ok(updatedGame) 
                    : Results.NotFound($"Video game with id {id} not found.");
            });
        }
    }
}

//[ApiController]
//[Route("api/games")]
//public class UpdateGameController(ISender sender) : ControllerBase
//{
//    [HttpPut("{id}")]
//    public async Task<ActionResult<UpdateGame.Response>> UpdateGame(int id, UpdateGame.Command command, CancellationToken cancellationToken)
//    {
//        if (id != command.Id)
//        {
//            return BadRequest("ID in URL does not match ID in request body.");
//        }

//        var response = await sender.Send(command, cancellationToken);
//        if (response == null)
//        {
//            return NotFound("Video game with given In not found");
//        }

//        return Ok(response);
//    }
//}
