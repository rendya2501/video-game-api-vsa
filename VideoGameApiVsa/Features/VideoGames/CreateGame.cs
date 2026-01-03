using FluentValidation;
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
    public record CreateGameRequest(string Title, string Genre, int ReleaseYear);

    /// <summary>
    /// MediatRコマンド（内部使用のみ）
    /// </summary>
    /// <param name="Title"></param>
    /// <param name="Genre"></param>
    /// <param name="ReleaseYear"></param>
    public record CreateGameCommand(string Title, string Genre, int ReleaseYear) : IRequest<CreateGameResponse>;

    public record CreateGameResponse(int Id, string Title, string Genre, int ReleaseYear);

    public class Validator : AbstractValidator<CreateGameCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()// .WithMessage("Title is required.")
                .MaximumLength(100);// .WithMessage("Length is Max100.");
            RuleFor(x => x.Genre)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(x => x.ReleaseYear)
                .InclusiveBetween(1950, DateTime.Now.Year);
        }
    }

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<CreateGameCommand, CreateGameResponse>
    {
        public async Task<CreateGameResponse> Handle(CreateGameCommand command, CancellationToken ct)
        {
            var videoGame = new VideoGame
            {
                Title = command.Title,
                Genre = command.Genre,
                ReleaseYear = command.ReleaseYear
            };

            dbContext.VideoGames.Add(videoGame);

            await dbContext.SaveChangesAsync(ct);

            return new CreateGameResponse(videoGame.Id, videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
        }
    }

    public static async Task<IResult> Endpoint(ISender sender, CreateGameRequest request, CancellationToken ct)
    {
        var command = new CreateGameCommand(request.Title, request.Genre, request.ReleaseYear);
        var result = await sender.Send(command, ct);
        return Results.CreatedAtRoute(
            routeName: VideoGameRouteNames.GetById,
            routeValues: new { id = result.Id },
            value: result);
    }
}
