using MediatR;

namespace VideoGameApiVsa.Features.WeatherForecast;

/// <summary>
/// 「天気予報取得」機能の垂直スライス
/// </summary>
/// <remarks>
/// <para>
/// 5日間の天気予報データを生成して返却するデモ機能。
/// 実際の気象APIとの連携は行わず、ランダムデータを生成する。
/// </para>
/// <para>
/// <strong>処理フロー:</strong><br/>
/// 1. Endpoint が HTTP GET リクエストを受信<br/>
/// 2. パラメータなしの Query を生成<br/>
/// 3. Handler がランダムな天気予報データを5日分生成<br/>
/// 4. Response DTOのリストとして返却
/// </para>
/// <para>
/// <strong>設計意図:</strong><br/>
/// - VSAのサンプルとして、ビデオゲーム機能とは独立した別ドメインの実装例を提供<br/>
/// - 外部API連携のプレースホルダーとして機能（本番環境では実際の気象APIを呼び出す想定）<br/>
/// - データベースを使用しない軽量な機能の実装パターンを示す
/// </para>
/// <para>
/// <strong>本番環境への拡張:</strong><br/>
/// - IWeatherServiceインターフェースを定義し、外部API呼び出しロジックを注入<br/>
/// - キャッシング機能を追加してAPI呼び出し回数を削減<br/>
/// - 地域パラメータを追加して地域別の天気予報に対応
/// </para>
/// </remarks>
public static class GetWeatherForecast
{
    /// <summary>
    /// 天気の状態を表す定数
    /// </summary>
    /// <remarks>
    /// 気温に応じた天気の表現を定義。
    /// 本番環境では外部APIからの応答をマッピングする際に使用。
    /// </remarks>
    private static readonly string[] Summaries =
    [
        "Freezing",    // 氷点下
        "Bracing",     // 身を切るような寒さ
        "Chilly",      // 肌寒い
        "Cool",        // 涼しい
        "Mild",        // 穏やか
        "Warm",        // 暖かい
        "Balmy",       // 快適
        "Hot",         // 暑い
        "Sweltering",  // うだるような暑さ
        "Scorching"    // 焼けつくような暑さ
    ];

    /// <summary>
    /// 天気予報取得クエリ
    /// </summary>
    /// <remarks>
    /// パラメータを持たないシンプルなクエリ。
    /// 将来的に以下のパラメータを追加可能:
    /// - Location（地域指定）
    /// - Days（予報日数）
    /// - Units（温度単位: 摂氏/華氏）
    /// </remarks>
    public record WeatherForecastQuery() : IRequest<IEnumerable<WeatherForecastResponse>>;

    /// <summary>
    /// 天気予報のレスポンスDTO
    /// </summary>
    /// <param name="Date">予報日</param>
    /// <param name="TemperatureC">摂氏温度</param>
    /// <param name="TemperatureF">華氏温度</param>
    /// <param name="Summary">天気の概要（Freezing, Hot など）</param>
    /// <remarks>
    /// 摂氏と華氏の両方を含めることで、国際的なAPIとしての利便性を向上。
    /// Summaryはnullable（?）だが、現在の実装では必ず値が設定される。
    /// </remarks>
    public record WeatherForecastResponse(DateOnly Date, int TemperatureC, int TemperatureF, string? Summary);

    /// <summary>
    /// クエリハンドラ（天気予報データ生成処理）
    /// </summary>
    /// <remarks>
    /// 本クラスはデータベースやHTTPクライアントを必要としないため、
    /// コンストラクタパラメータを持たない。
    /// 本番環境では IWeatherService を注入して実装する想定。
    /// </remarks>
    public class Handler : IRequestHandler<WeatherForecastQuery, IEnumerable<WeatherForecastResponse>>
    {
        /// <summary>
        /// 5日間の天気予報データを生成
        /// </summary>
        /// <param name="query">天気予報取得クエリ</param>
        /// <param name="ct">キャンセルトークン（現在の実装では未使用）</param>
        /// <returns>5日分の天気予報データ</returns>
        /// <remarks>
        /// <para>
        /// <strong>現在の実装:</strong><br/>
        /// - Random.Sharedを使用してスレッドセーフなランダムデータ生成<br/>
        /// - 気温範囲: -20°C ～ 55°C（現実的な地球上の気温）<br/>
        /// - 華氏変換式: F = 32 + (C / 0.5556)
        /// </para>
        /// <para>
        /// <strong>本番環境での実装例:</strong><br/>
        /// <code>
        /// var weatherData = await _weatherService.GetForecastAsync(location, days: 5, ct);
        /// return weatherData.Select(d => new WeatherForecastResponse(...));
        /// </code>
        /// </para>
        /// </remarks>
        public Task<IEnumerable<WeatherForecastResponse>> Handle(WeatherForecastQuery query, CancellationToken ct)
        {
            // 5日分の天気予報を生成
            var forecast = Enumerable.Range(1, 5).Select(index =>
            {
                // ランダムな摂氏温度を生成（-20°C ～ 55°C）
                var temperatureC = Random.Shared.Next(-20, 55);

                return new WeatherForecastResponse(
                    Date: DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC: temperatureC,
                    // 摂氏から華氏への変換: F = 32 + (C × 9/5)
                    TemperatureF: 32 + (int)(temperatureC / 0.5556),
                    // ランダムな天気概要を選択
                    Summary: Summaries[Random.Shared.Next(Summaries.Length)]
                );
            });

            // 同期処理だが、IRequestHandlerのインターフェース要件を満たすためTask.FromResultで包む
            return Task.FromResult(forecast);
        }
    }

    /// <summary>
    /// HTTPエンドポイント（天気予報取得）
    /// </summary>
    /// <param name="sender">MediatR送信インターフェース</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>HTTP 200 OK + 5日分の天気予報</returns>
    /// <remarks>
    /// GET /api/weather-forecast の形式で呼び出される。
    /// クエリパラメータを受け取らないシンプルなエンドポイント。
    /// </remarks>
    public static async Task<IResult> Endpoint(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new WeatherForecastQuery(), ct);
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
