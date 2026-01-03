using MediatR;
using VideoGameApiVsa.Data;

namespace VideoGameApiVsa.Features.VideoGames;

/// <summary>
/// 「ゲーム削除」機能の垂直スライス
/// </summary>
/// <remarks>
/// <para>
/// IDを指定してゲームを削除する。
/// 存在しないIDの場合は404 Not Foundを返却。
/// </para>
/// <para>
/// <strong>処理フロー:</strong><br/>
/// 1. Endpoint が HTTP DELETE リクエストを受信<br/>
/// 2. ID から Command を生成<br/>
/// 3. Handler がデータベースからエンティティを削除<br/>
/// 4. 成功時は 204 No Content、失敗時は 404 Not Found
/// </para>
/// </remarks>
public static class DeleteGame
{
    /// <summary>
    /// ゲーム削除コマンド
    /// </summary>
    /// <param name="Id">削除対象のゲームID</param>
    /// <remarks>
    /// 削除成功時はtrue、対象が存在しない場合はfalseを返す。
    /// </remarks>
    public record DeleteGameCommand(int Id) : IRequest<bool>;

    /// <summary>
    /// コマンドハンドラ（削除処理実行）
    /// </summary>
    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<DeleteGameCommand, bool>
    {
        /// <summary>
        /// ゲーム削除処理を実行
        /// </summary>
        /// <param name="command">削除コマンド</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>削除成功時true、対象不在時false</returns>
        public async Task<bool> Handle(DeleteGameCommand command, CancellationToken ct)
        {
            var videoGame = await dbContext.VideoGames.FindAsync([command.Id], ct);
            if (videoGame is null)            
                return false;
            

            dbContext.VideoGames.Remove(videoGame);
            await dbContext.SaveChangesAsync(ct);

            return true;
        }
    }

    /// <summary>
    /// HTTPエンドポイント（ゲーム削除）
    /// </summary>
    /// <param name="sender">MediatR送信インターフェース</param>
    /// <param name="id">削除対象のゲームID（ルートパラメータ）</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>HTTP 204 No Content または 404 Not Found</returns>
    /// <remarks>
    /// RESTful設計に従い、削除成功時はボディなしの204を返却。
    /// </remarks>
    public static async Task<IResult> Endpoint(ISender sender, int id, CancellationToken ct)
    {
        var deleted = await sender.Send(new DeleteGameCommand(id), ct);

        if (deleted is false)
            return Results.NotFound($"Video game with id {id} not found.");

        return Results.NoContent();
    }
}
