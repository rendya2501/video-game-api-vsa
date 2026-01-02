using MediatR;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

public static class DeleteGame
{
    public record Command(int Id) : IRequest<bool>;

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<Command, bool>
    {
        public async Task<bool> Handle(Command command, CancellationToken ct)
        {
            var videoGame = await dbContext.VideoGames.FindAsync([command.Id], ct);
            if (videoGame is null)
            {
                return false;
            }

            dbContext.VideoGames.Remove(videoGame);
            await dbContext.SaveChangesAsync(ct);

            return true;
        }
    }

    public static async Task<IResult> Endpoint(ISender sender, int id, CancellationToken ct)
    {
        var deleted = await sender.Send(new Command(id), ct);
        return deleted
            ? Results.NoContent()
            : Results.NotFound($"Video game with id {id} not found.");
    }
}
