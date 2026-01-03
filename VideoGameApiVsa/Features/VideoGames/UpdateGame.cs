using FluentValidation;
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
    public record UpdateGameRequest(string Title, string Genre, int ReleaseYear);

    /// <summary>
    /// MediatRコマンド（内部使用のみ）
    /// </summary>
    /// <param name="Title"></param>
    /// <param name="Genre"></param>
    /// <param name="ReleaseYear"></param>
    public record UpdateGameCommand(int Id, string Title, string Genre, int ReleaseYear) : IRequest<UpdateGameResponse?>;

    public record UpdateGameResponse(int Id, string Title, string Genre, int ReleaseYear);

    public class Validator : AbstractValidator<UpdateGameCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(100);
            RuleFor(x => x.Genre)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(x => x.ReleaseYear)
                .InclusiveBetween(1950, DateTime.Now.Year);
        }
    }

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<UpdateGameCommand, UpdateGameResponse?>
    {
        public async Task<UpdateGameResponse?> Handle(UpdateGameCommand command, CancellationToken ct)
        {
            var videoGame = await dbContext.VideoGames.FindAsync([command.Id], ct);

            if (videoGame is null)
                return null;

            videoGame.Title = command.Title;
            videoGame.Genre = command.Genre;
            videoGame.ReleaseYear = command.ReleaseYear;

            await dbContext.SaveChangesAsync(ct);
            return new UpdateGameResponse(videoGame.Id, videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
        }
    }

    public static async Task<IResult> Endpoint(ISender sender, int id, UpdateGameRequest request, CancellationToken ct)
    {
        var command = new UpdateGameCommand(id, request.Title, request.Genre, request.ReleaseYear);
        var result = await sender.Send(command with { Id = id }, ct);

        if (result is null)
            return Results.NotFound($"Video game with id {id} not found.");

        return Results.Ok(result);
    }
}
