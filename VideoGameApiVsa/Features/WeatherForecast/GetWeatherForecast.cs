using MediatR;

namespace VideoGameApiVsa.Features.WeatherForecast;

/// <summary>
/// 天気予報を取得する機能
/// </summary>
public static class GetWeatherForecast
{
    /// <summary>
    /// 天気の状態を表す定数
    /// </summary>
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <summary>
    /// 天気予報取得クエリ（パラメータなし）
    /// </summary>
    public record Query() : IRequest<IEnumerable<WeatherForecastResponse>>;

    /// <summary>
    /// 天気予報のレスポンスDTO
    /// </summary>
    /// <param name="Date"></param>
    /// <param name="TemperatureC"></param>
    /// <param name="TemperatureF"></param>
    /// <param name="Summary"></param>
    public record WeatherForecastResponse(DateOnly Date, int TemperatureC, int TemperatureF, string? Summary);

    /// <summary>
    /// クエリハンドラー：天気予報データを生成
    /// </summary>
    public class Handler : IRequestHandler<Query, IEnumerable<WeatherForecastResponse>>
    {
        public Task<IEnumerable<WeatherForecastResponse>> Handle(Query query, CancellationToken ct)
        {
            // 5日分の天気予報を生成
            var forecast = Enumerable.Range(1, 5).Select(index =>
            {
                var temperatureC = Random.Shared.Next(-20, 55);
                return new WeatherForecastResponse(
                    Date: DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC: temperatureC,
                    TemperatureF: 32 + (int)(temperatureC / 0.5556),
                    Summary: Summaries[Random.Shared.Next(Summaries.Length)]
                );
            });
            return Task.FromResult(forecast);
        }
    }

    /// <summary>
    /// エンドポイント：天気予報を取得
    /// </summary>
    public static async Task<IResult> Endpoint(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new Query(), ct);
        return Results.Ok(result);
    }
}

//    [ApiController]
//    [Route("[controller]")]
//    public class WeatherForecastController : ControllerBase
//    {
//        private static readonly string[] Summaries =
//        [
//            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//        ];
//
//        [HttpGet(Name = "GetWeatherForecast")]
//        public IEnumerable<WeatherForecast> Get()
//        {
//            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
//            {
//                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//                TemperatureC = Random.Shared.Next(-20, 55),
//                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
//            })
//            .ToArray();
//        }
//    }
//
//    public class WeatherForecast
//    {
//        public DateOnly Date { get; set; }
//        public int TemperatureC { get; set; }
//        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//        public string? Summary { get; set; }
//    }
