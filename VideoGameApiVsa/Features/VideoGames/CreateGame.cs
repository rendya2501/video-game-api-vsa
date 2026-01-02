using MediatR;
using VideoGameApiVsa.Data;
using VideoGameApiVsa.Entities;

namespace VideoGameApiVsa.Features.VideoGames;

public static class CreateGame
{
    /// <summary>
    /// リクエスト用DTO（OpenAPI/Scalarで表示される）
    /// </summary>
    /// <param name="Title"></param>
    /// <param name="Genre"></param>
    /// <param name="ReleaseYear"></param>
    public record Request(string Title, string Genre, int ReleaseYear);

    /// <summary>
    /// MediatRコマンド（内部使用のみ）
    /// </summary>
    /// <param name="Title"></param>
    /// <param name="Genre"></param>
    /// <param name="ReleaseYear"></param>
    public record Command(string Title, string Genre, int ReleaseYear) : IRequest<Response>;

    public record Response(int Id, string Title, string Genre, int ReleaseYear);

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command command, CancellationToken ct)
        {
            var videoGame = new VideoGame
            {
                Title = command.Title,
                Genre = command.Genre,
                ReleaseYear = command.ReleaseYear
            };

            dbContext.VideoGames.Add(videoGame);

            await dbContext.SaveChangesAsync(ct);

            return new Response(videoGame.Id, videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
        }
    }

    public static async Task<IResult> Endpoint(ISender sender, Request request, CancellationToken ct)
    {
        var command = new Command(request.Title, request.Genre, request.ReleaseYear);
        var result = await sender.Send(command, ct);
        return Results.CreatedAtRoute(
            routeName: "GetGameById",
            routeValues: new { id = result.Id },
            value: result);
    }
}
