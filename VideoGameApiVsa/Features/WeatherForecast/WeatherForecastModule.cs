using Carter;

namespace VideoGameApiVsa.Features.WeatherForecast;

/// <summary>
/// WeatherForecast機能のエンドポイント定義モジュール
/// </summary>
/// <remarks>
/// <para>
/// Carterライブラリを使用してエンドポイントをモジュール化。
/// 天気予報に関連するすべてのHTTPルートを一箇所で管理する。
/// </para>
/// <para>
/// <strong>モジュール化の利点:</strong><br/>
/// - Program.csがシンプルになり、エンドポイント定義が分散しない<br/>
/// - 機能ごとにルート定義を独立させ、保守性を向上<br/>
/// - OpenAPI/Scalarドキュメントでタグごとにグループ化される
/// </para>
/// <para>
/// <strong>拡張例:</strong><br/>
/// 将来的に以下のエンドポイントを追加可能:
/// <code>
/// group.MapGet("/{location}", GetWeatherForecastByLocation.Endpoint)
///     .WithName("GetWeatherForecastByLocation");
/// 
/// group.MapGet("/current/{location}", GetCurrentWeather.Endpoint)
///     .WithName("GetCurrentWeather");
/// </code>
/// </para>
/// </remarks>
public sealed class WeatherForecastModule : ICarterModule
{
    /// <summary>
    /// エンドポイントルートを登録
    /// </summary>
    /// <param name="app">エンドポイントルートビルダー</param>
    /// <remarks>
    /// <para>
    /// <strong>ルート構成:</strong><br/>
    /// - ベースパス: /api/weather-forecast<br/>
    /// - タグ: Weather（OpenAPI/Scalarでのグルーピング用）
    /// </para>
    /// <para>
    /// <strong>メタデータの役割:</strong><br/>
    /// - WithName: ルート名を定義（他のエンドポイントからの参照に使用）<br/>
    /// - WithDescription: OpenAPI/Scalarドキュメントに表示される説明文<br/>
    /// - Produces: 成功時のレスポンス型とHTTPステータスコードを定義
    /// </para>
    /// <para>
    /// <strong>RESTful設計:</strong><br/>
    /// /api/weather-forecast というリソース指向のURL構造を採用。
    /// 複数形ではなく weather-forecast（単数）としているのは、
    /// 「予報」という概念が単一のサービスを表すため。
    /// </para>
    /// </remarks>
    public void AddRoutes(IEndpointRouteBuilder app)
    {       
        // 天気予報エンドポイントグループを作成
        // すべてのルートに /api/weather-forecast プレフィックスが付与される
        var group = app.MapGroup("/api/weather-forecast")
            .WithTags("Weather");

        // GET /api/weather-forecast - 5日間の天気予報を取得
        group.MapGet("/", GetWeatherForecast.Endpoint)
            .WithName("GetWeatherForecast") // ルート名（他のエンドポイントから参照可能）
            .WithDescription("Retrieves a 5-day weather forecast with temperature and conditions")
            .Produces<IEnumerable<GetWeatherForecast.WeatherForecastResponse>>(StatusCodes.Status200OK);
    }
}
