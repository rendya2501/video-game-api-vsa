using MediatR;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

public static class GetGameById
{
    public record Query(int Id) : IRequest<Response?>;

    public record Response(int Id, string Title, string Genre, int ReleaseYear);

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query query, CancellationToken ct)
        {
            var videoGame = await dbContext.VideoGames.FindAsync([query.Id], ct);
            if (videoGame is null)
            {
                return null;
            }

            return new Response(videoGame.Id, videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
        }
    }

    public static async Task<IResult> Endpoint(ISender sender, int id, CancellationToken ct)
    {
        var result = await sender.Send(new Query(id), ct);

        if (result is null)
            return Results.NotFound($"Video game with id {id} not found.");

        return Results.Ok(result);
    }
}
