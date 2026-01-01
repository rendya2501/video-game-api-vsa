using Carter;
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
        public async Task<IEnumerable<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var videoGamses = await dbContext.VideoGames.ToListAsync(cancellationToken);
            return videoGamses.Select(vg => new Response(vg.Id, vg.Title, vg.Genre, vg.ReleaseYear));
        }
    }

    public class EndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/games/", async (ISender sender, CancellationToken cancellationToken) =>
                 await sender.Send(new Query(), cancellationToken));
        }
    }
}

//[ApiController]
//[Route("api/games")]
//public class GetAllGamsesController(ISender sender) : ControllerBase
//{
//    [HttpGet]
//    public async Task<ActionResult<IEnumerable<GetAllGames.Response>>> GetAllGames(CancellationToken cancellationToken)
//    {
//        var response = await sender.Send(new GetAllGames.Query(), cancellationToken);
//        return Ok(response);
//    }
//}
