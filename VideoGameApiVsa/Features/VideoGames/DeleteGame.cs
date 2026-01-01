using Carter;
using MediatR;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

public static class DeleteGame
{
    public record Command(int Id) : IRequest<bool>;

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var videoGame = await dbContext.VideoGames.FindAsync([request.Id], cancellationToken);
            if (videoGame is null)
            {
                return false;
            }

            dbContext.VideoGames.Remove(videoGame);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public class EndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/games/{id}", async (ISender sender, int id, CancellationToken cancellationToken) =>
                await sender.Send(new Command(id), cancellationToken) ? Results.NoContent()
                    : Results.NotFound($"Video game with id {id} not found."));
        }
    }
}

//[ApiController]
//[Route("api/games")]
//public class DeleteGameController(ISender sender) : ControllerBase
//{
//    [HttpDelete("{id}")]
//    public async Task<ActionResult<bool>> DeleteGame(int id, CancellationToken cancellationToken)
//    {
//        var response = await sender.Send(new DeleteGame.Command(id), cancellationToken);
//        if (!response)
//        {
//            return NotFound("Video game with given ID not found");
//        }

//        return NoContent();
//    }
//}
