using MediatR;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

/// <summary>
/// 「ゲーム詳細取得」機能の垂直スライス
/// </summary>
/// <remarks>
/// <para>
/// IDを指定して特定のゲーム情報を取得する。
/// 存在しないIDの場合は404 Not Foundを返却。
/// </para>
/// <para>
/// <strong>処理フロー:</strong><br/>
/// 1. Endpoint が HTTP GET リクエストを受信<br/>
/// 2. ルートパラメータのIDからQueryを生成<br/>
/// 3. Handler がデータベースから該当エンティティを検索<br/>
/// 4. 存在時は 200 OK + 詳細情報、不在時は 404 Not Found
/// </para>
/// <para>
/// <strong>設計上の注意:</strong><br/>
/// Response型にnullableを使用することで、
/// 「データが見つからない」状態をドメインレイヤーで表現している。
/// </para>
/// </remarks>
public static class GetGameById
{
    /// <summary>
    /// ゲーム詳細取得クエリ
    /// </summary>
    /// <param name="Id">取得対象のゲームID</param>
    /// <remarks>
    /// 戻り値がnullableであり、データ不在時はnullを返す設計。
    /// </remarks>
    public record GetGameByIdQuery(int Id) : IRequest<GetGameByIdResponse?>;

    /// <summary>
    /// ゲーム詳細レスポンス
    /// </summary>
    /// <param name="Id">ゲームID</param>
    /// <param name="Title">ゲームタイトル</param>
    /// <param name="Genre">ゲームジャンル</param>
    /// <param name="ReleaseYear">リリース年</param>
    /// <remarks>
    /// 現状はGetAllGamesResponseと同一構造だが、
    /// 将来的に詳細情報（説明文、評価など）を追加する余地を残している。
    /// </remarks>
    public record GetGameByIdResponse(int Id, string Title, string Genre, int ReleaseYear);

    /// <summary>
    /// クエリハンドラ（詳細取得処理実行）
    /// </summary>
    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<GetGameByIdQuery, GetGameByIdResponse?>
    {
        /// <summary>
        /// 指定IDのゲーム情報を取得
        /// </summary>
        /// <param name="query">詳細取得クエリ</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>ゲーム詳細情報、または存在しない場合はnull</returns>
        /// <remarks>
        /// FindAsync()は主キーによる高速検索を実行。
        /// 存在チェックをハンドラ内で行い、nullを返すことで
        /// Endpointでの404判定を可能にしている。
        /// </remarks>
        public async Task<GetGameByIdResponse?> Handle(GetGameByIdQuery query, CancellationToken ct)
        {
            var videoGame = await dbContext.VideoGames.FindAsync([query.Id], ct);
            if (videoGame is null)
                return null;

            return new GetGameByIdResponse(videoGame.Id, videoGame.Title, videoGame.Genre, videoGame.ReleaseYear);
        }
    }

    /// <summary>
    /// HTTPエンドポイント（ゲーム詳細取得）
    /// </summary>
    /// <param name="sender">MediatR送信インターフェース</param>
    /// <param name="id">取得対象のゲームID（ルートパラメータ）</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>HTTP 200 OK + 詳細情報 または 404 Not Found</returns>
    /// <remarks>
    /// RESTful設計に従い、リソースの存在有無をHTTPステータスで表現。
    /// </remarks>
    public static async Task<IResult> Endpoint(ISender sender, int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetGameByIdQuery(id), ct);

        if (result is null)
            return Results.NotFound($"Video game with id {id} not found.");

        return Results.Ok(result);
    }
}
