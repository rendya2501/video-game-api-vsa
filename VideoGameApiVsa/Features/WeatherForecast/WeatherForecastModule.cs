using Carter;

namespace VideoGameApiVsa.Features.WeatherForecast;

/// <summary>
/// WeatherForecastに関連するすべてのエンドポイントを管理するモジュール
/// </summary>
public sealed class WeatherForecastModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/weather-forecast")
            .WithTags("Weather");

        group.MapGet("/", GetWeatherForecast.Endpoint)
            .WithName("GetWeatherForecast")
            .WithDescription("Retrieves a 5-day weather forecast with temperature and conditions")
            .Produces<IEnumerable<GetWeatherForecast.WeatherForecastResponse>>(StatusCodes.Status200OK);
    }
}
