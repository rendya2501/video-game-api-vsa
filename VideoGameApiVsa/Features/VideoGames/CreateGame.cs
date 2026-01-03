using FluentValidation;
using MediatR;
using VideoGameApiVsa.Data;
using VideoGameApiVsa.Entities;

namespace VideoGameApiVsa.Features.VideoGames;

/// <summary>
/// 「ゲーム作成」機能の垂直スライス
/// </summary>
/// <remarks>
/// <para>
/// Request / Command / Validator / Handler / Endpoint を1ファイルに集約。
/// このファイルだけでゲーム作成機能の全体像を把握できる。
/// </para>
/// <para>
/// <strong>処理フロー:</strong><br/>
/// 1. Endpoint が HTTP リクエストを受信<br/>
/// 2. Request → Command へ変換<br/>
/// 3. ValidationBehavior が Validator を実行<br/>
/// 4. Handler がビジネスロジックを実行<br/>
/// 5. Response を HTTP 201 Created で返却
/// </para>
/// </remarks>
public static class CreateGame
{
    /// <summary>
    /// ゲーム作成リクエスト（外部APIインターフェース）
    /// </summary>
    /// <param name="Title">ゲームタイトル（最大100文字）</param>
    /// <param name="Genre">ゲームジャンル（最大50文字）</param>
    /// <param name="ReleaseYear">リリース年（1950年以降）</param>
    /// <remarks>
    /// OpenAPI/Scalarでドキュメント化される公開API契約。
    /// 内部のCommandとは意図的に分離し、API仕様の独立性を保つ。
    /// </remarks>
    public record CreateGameRequest(string Title, string Genre, int ReleaseYear);

    /// <summary>
    /// ゲーム作成コマンド（内部処理用）
    /// </summary>
    /// <param name="Title">ゲームタイトル</param>
    /// <param name="Genre">ゲームジャンル</param>
    /// <param name="ReleaseYear">リリース年</param>
    /// <remarks>
    /// MediatR経由で処理されるアプリケーション内部のメッセージ。
    /// ValidationBehaviorにより自動的にValidatorが適用される。
    /// </remarks>
    public record CreateGameCommand(string Title, string Genre, int ReleaseYear) : IRequest<CreateGameResponse>;

    /// <summary>
    /// ゲーム作成レスポンス
    /// </summary>
    /// <param name="Id">作成されたゲームのID</param>
    /// <param name="Title">ゲームタイトル</param>
    /// <param name="Genre">ゲームジャンル</param>
    /// <param name="ReleaseYear">リリース年</param>
    /// <remarks>
    /// Entityを直接公開せず、API専用のDTOとして定義。
    /// 将来的なEntity変更がAPIに影響しないよう分離している。
    /// </remarks>
    public record CreateGameResponse(int Id, string Title, string Genre, int ReleaseYear);

    /// <summary>
    /// コマンド検証ルール
    /// </summary>
    /// <remarks>
    /// MediatR Pipelineで自動実行される。
    /// 検証失敗時はValidationExceptionをスローし、
    /// グローバル例外ハンドラでProblemDetails形式に変換される。
    /// </remarks>
    public class Validator : AbstractValidator<CreateGameCommand>
    {
        public Validator()
        {
            // タイトルは必須 & 最大100文字
            RuleFor(x => x.Title)
                .NotEmpty()// .WithMessage("Title is required.")
                .MaximumLength(100);// .WithMessage("Length is Max100.");

            // ジャンルは必須 & 最大50文字
            RuleFor(x => x.Genre)
                .NotEmpty()
                .MaximumLength(50);

            // リリース年は現実的な範囲に制限
            RuleFor(x => x.ReleaseYear)
                .InclusiveBetween(1950, DateTime.Now.Year);
        }
    }

    /// <summary>
    /// コマンドハンドラ（ビジネスロジック実行）
    /// </summary>
    public class Handler(VideoGameDbContext dbContext) : IRequestHandler<CreateGameCommand, CreateGameResponse>
    {
        /// <summary>
        /// ゲーム作成処理を実行
        /// </summary>
        /// <param name="command">作成コマンド</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>作成されたゲーム情報</returns>
        public async Task<CreateGameResponse> Handle(CreateGameCommand command, CancellationToken ct)
        {
            // Command → Entity への変換
            var videoGame = new VideoGame
            {
                Title = command.Title,
                Genre = command.Genre,
                ReleaseYear = command.ReleaseYear
            };

            // EF Core による永続化
            dbContext.VideoGames.Add(videoGame);
            await dbContext.SaveChangesAsync(ct);

            // Entity → Response への変換
            return new CreateGameResponse(
                videoGame.Id,
                videoGame.Title,
                videoGame.Genre,
                videoGame.ReleaseYear
            );
        }
    }

    /// <summary>
    /// HTTPエンドポイント（ゲーム作成）
    /// </summary>
    /// <param name="sender">MediatR送信インターフェース</param>
    /// <param name="request">HTTPリクエストボディ</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>HTTP 201 Created + Location ヘッダ</returns>
    /// <remarks>
    /// Minimal API/Carterから呼び出される薄いレイヤー。
    /// HTTPの詳細を隠蔽し、CommandへのマッピングとMediatR呼び出しのみを担当。
    /// </remarks>
    public static async Task<IResult> Endpoint(ISender sender, CreateGameRequest request, CancellationToken ct)
    {
        // 外部入力 DTO → 内部 Command へ変換
        var command = new CreateGameCommand(
            request.Title,
            request.Genre,
            request.ReleaseYear
        );

        // MediatR 経由で処理を実行
        var result = await sender.Send(command, ct);

        // 201 Created + Location ヘッダ付きレスポンス
        return Results.CreatedAtRoute(
            routeName: VideoGameRouteNames.GetById,
            routeValues: new { id = result.Id },
            value: result);
    }
}
