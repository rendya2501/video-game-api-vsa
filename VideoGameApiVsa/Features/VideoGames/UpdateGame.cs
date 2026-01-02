using MediatR;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

public static class UpdateGame
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
    public record Command(int Id, string Title, string Genre, int ReleaseYear) : IRequest<Response?>;

    public record Response(int Id, string Title, string Genre, int ReleaseYear);

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<Command, Response?>
    {
        public async Task<Response?> Handle(Command command, CancellationToken ct)
        {
            var videoGame = await dbContext.VideoGames.FindAsync([command.Id], ct);

            if (videoGame is null)
                return null;

            videoGame.Title = command.Title;
            videoGame.Genre = command.Genre;
            videoGame.ReleaseYear = command.ReleaseYear;

            await dbContext.SaveChangesAsync(ct);
            return new Response(videoGame.Id, videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
        }
    }

    public static async Task<IResult> Endpoint(ISender sender, int id, Request request, CancellationToken ct)
    {
        var command = new Command(id, request.Title, request.Genre, request.ReleaseYear);
        var result = await sender.Send(command with { Id = id }, ct);

        if (result is null)
            return Results.NotFound($"Video game with id {id} not found.");

        return Results.Ok(result);
    }
}
