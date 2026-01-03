using MediatR;
using Microsoft.EntityFrameworkCore;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

/// <summary>
/// 「ゲーム一覧取得」機能の垂直スライス
/// </summary>
/// <remarks>
/// <para>
/// データベースに登録されている全ゲームを取得する。
/// 検索条件やページネーションは含まない基本的な一覧取得。
/// </para>
/// <para>
/// <strong>処理フロー:</strong><br/>
/// 1. Endpoint が HTTP GET リクエストを受信<br/>
/// 2. パラメータなしの Query を生成<br/>
/// 3. Handler が全ゲームをデータベースから取得<br/>
/// 4. Response DTOのリストとして返却
/// </para>
/// <para>
/// <strong>注意:</strong><br/>
/// 大量データが存在する場合はページネーションの実装を推奨。
/// </para>
/// </remarks>
public static class GetAllGames
{
    /// <summary>
    /// ゲーム一覧取得クエリ
    /// </summary>
    /// <remarks>
    /// パラメータを持たないシンプルなクエリ。
    /// 将来的にフィルタリングやソート機能を追加する場合は、
    /// プロパティを追加して拡張可能。
    /// </remarks>
    public record GetAllGamesQuery : IRequest<IEnumerable<GetAllGamesResponse>>;

    /// <summary>
    /// ゲーム情報レスポンス（一覧用）
    /// </summary>
    /// <param name="Id">ゲームID</param>
    /// <param name="Title">ゲームタイトル</param>
    /// <param name="Genre">ゲームジャンル</param>
    /// <param name="ReleaseYear">リリース年</param>
    /// <remarks>
    /// 一覧表示に必要な最小限のフィールドのみを含む。
    /// 詳細情報が必要な場合はGetByIdを使用。
    /// </remarks>
    public record GetAllGamesResponse(int Id, string Title, string Genre, int ReleaseYear);

    /// <summary>
    /// クエリハンドラ（一覧取得処理実行）
    /// </summary>
    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<GetAllGamesQuery, IEnumerable<GetAllGamesResponse>>
    {
        /// <summary>
        /// 全ゲーム情報を取得
        /// </summary>
        /// <param name="query">一覧取得クエリ</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>全ゲーム情報のコレクション</returns>
        /// <remarks>
        /// ToListAsync()により、データベースへの1回のクエリで全データを取得。
        /// その後、メモリ上でEntity→Response DTOへのマッピングを実行。
        /// </remarks>
        public async Task<IEnumerable<GetAllGamesResponse>> Handle(GetAllGamesQuery query, CancellationToken ct)
        {
            var videoGames = await dbContext.VideoGames.ToListAsync(ct);
            return videoGames.Select(vg => new GetAllGamesResponse(vg.Id, vg.Title, vg.Genre, vg.ReleaseYear));
        }
    }

    /// <summary>
    /// HTTPエンドポイント（ゲーム一覧取得）
    /// </summary>
    /// <param name="sender">MediatR送信インターフェース</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>HTTP 200 OK + ゲーム一覧</returns>
    /// <remarks>
    /// クエリパラメータを受け取らないシンプルなGETエンドポイント。
    /// </remarks>
    public static async Task<IResult> Endpoint(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetAllGamesQuery(), ct);
        return Results.Ok(result);
    }
}
