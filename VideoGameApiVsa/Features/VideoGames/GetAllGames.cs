using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

public static class GetAllGames
{
    public record Query : IRequest<IEnumerable<Response>>;

    public record Response(int Id, string Title, string Genre, int ReleaseYear);

    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<Query, IEnumerable<Response>>
    {
        public async Task<IEnumerable<Response>> Handle(Query query, CancellationToken ct)
        {
            var videoGamses = await dbContext.VideoGames.ToListAsync(ct);
            return videoGamses.Select(vg => new Response(vg.Id, vg.Title, vg.Genre, vg.ReleaseYear));
        }
    }

    public static async Task<IResult> Endpoint(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new Query(), ct);
        return Results.Ok(result);
    }
}
