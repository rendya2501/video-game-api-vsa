# VideoGameApiVsa

ビデオゲーム管理用のRESTful API。垂直スライスアーキテクチャとCQRSパターンを採用したASP.NET Core 10.0アプリケーション。

## 📋 目次

- [概要](#概要)
- [技術スタック](#技術スタック)
- [アーキテクチャ](#アーキテクチャ)
- [機能](#機能)
- [セットアップ](#セットアップ)
- [実行方法](#実行方法)
- [API エンドポイント](#api-エンドポイント)
- [テスト](#テスト)
- [プロジェクト構成](#プロジェクト構成)
- [開発ガイドライン](#開発ガイドライン)

## 概要

VideoGameApiVsaは、ビデオゲーム情報を管理するためのRESTful APIです。以下の特徴があります：

- **垂直スライスアーキテクチャ**: 機能ごとにファイルを集約し、保守性を向上
- **CQRSパターン**: MediatRを使用したCommand/Query分離
- **自動バリデーション**: FluentValidationによる入力検証
- **構造化されたエラーハンドリング**: RFC 7807準拠のProblemDetails形式
- **包括的なテスト**: xUnitとFluentAssertionsによるテストスイート

## 技術スタック

### フレームワーク・ライブラリ

- **.NET 10.0**: 最新の.NETプラットフォーム
- **ASP.NET Core**: Web APIフレームワーク
- **Carter 10.0.0**: Minimal APIの拡張ライブラリ
- **MediatR 14.0.0**: CQRSパターン実装
- **FluentValidation 12.1.1**: バリデーションライブラリ
- **Entity Framework Core 10.0.1**: ORM
- **Scalar.AspNetCore 2.11.10**: API ドキュメントUI

### データベース

- **Entity Framework Core InMemory**: 開発・テスト用インメモリデータベース

### テスト

- **xUnit 2.9.3**: テストフレームワーク
- **FluentAssertions 8.8.0**: アサーションライブラリ
- **Microsoft.AspNetCore.Mvc.Testing**: 統合テスト用

## アーキテクチャ

### 垂直スライスアーキテクチャ

各機能は1つのファイルに集約され、以下の要素を含みます：

```txt
Features/
  VideoGames/
    CreateGame.cs      # Request, Command, Validator, Handler, Endpoint
    GetAllGames.cs     # Query, Handler, Endpoint
    GetGameById.cs     # Query, Handler, Endpoint
    UpdateGame.cs      # Request, Command, Validator, Handler, Endpoint
    DeleteGame.cs      # Command, Handler, Endpoint
    VideoGameModule.cs # ルーティング定義
```

### 処理フロー

```txt
HTTP Request
    ↓
Endpoint (HTTP層)
    ↓
Command/Query (MediatR)
    ↓
ValidationBehavior (FluentValidation)
    ↓
Handler (ビジネスロジック)
    ↓
DbContext (データアクセス)
    ↓
Response (HTTP層)
```

### 設計原則

1. **関心の分離**: Request/Command/Responseを分離し、API契約と内部実装を独立
2. **単一責任**: 各クラスは1つの責任のみを持つ
3. **依存性逆転**: インターフェース経由で依存関係を管理
4. **テスト容易性**: 依存性注入により、テスト可能な設計

## 機能

### VideoGames API

ビデオゲーム情報のCRUD操作を提供：

- **GET /api/games**: 全ゲーム一覧取得
- **GET /api/games/{id}**: 特定ゲームの詳細取得
- **POST /api/games**: 新規ゲーム作成
- **PUT /api/games/{id}**: ゲーム情報更新
- **DELETE /api/games/{id}**: ゲーム削除

### データモデル

```csharp
public class VideoGame
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Genre { get; set; }
    public required int ReleaseYear { get; set; }
}
```

### バリデーションルール

- **Title**: 必須、最大100文字
- **Genre**: 必須、最大50文字
- **ReleaseYear**: 1950年から現在年まで

## セットアップ

### 前提条件

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 / Visual Studio Code / JetBrains Rider

### インストール手順

1. リポジトリをクローン

```bash
git clone <repository-url>
cd VideoGameApiVsa
```

2. 依存パッケージを復元

```bash
dotnet restore
```

3. プロジェクトをビルド

```bash
dotnet build
```

## 実行方法

### 開発環境での実行

```bash
cd VideoGameApiVsa
dotnet run
```

アプリケーションは以下のURLで起動します：

- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

### API ドキュメント

開発環境では、以下のURLでAPIドキュメントにアクセスできます：

- **Scalar UI**: `https://localhost:5001/scalar/v1`
- **OpenAPI JSON**: `https://localhost:5001/openapi/v1.json`

## API エンドポイント

### VideoGames

#### 全ゲーム一覧取得

```http
GET /api/games
```

**レスポンス例:**

```json
[
  {
    "id": 1,
    "title": "The Legend of Zelda: Breath of the Wild",
    "genre": "Action",
    "releaseYear": 2017
  },
  {
    "id": 2,
    "title": "The Witcher 3: Wild Hunt",
    "genre": "RPG",
    "releaseYear": 2015
  }
]
```

#### ゲーム詳細取得

```http
GET /api/games/{id}
```

**レスポンス例:**

```json
{
  "id": 1,
  "title": "The Legend of Zelda: Breath of the Wild",
  "genre": "Action",
  "releaseYear": 2017
}
```

**エラー:**

- `404 Not Found`: 指定されたIDのゲームが存在しない場合

#### ゲーム作成

```http
POST /api/games
Content-Type: application/json

{
  "title": "New Game",
  "genre": "Action",
  "releaseYear": 2023
}
```

**レスポンス:**

- `201 Created`: 作成成功（Locationヘッダに作成されたリソースのURLを含む）
- `400 Bad Request`: バリデーションエラー

**バリデーションエラーレスポンス例:**

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/games",
  "errors": {
    "Title": ["'Title' must not be empty."],
    "ReleaseYear": ["'Release Year' must be between 1950 and 2024 (inclusive)."]
  }
}
```

#### ゲーム更新

```http
PUT /api/games/{id}
Content-Type: application/json

{
  "title": "Updated Game Title",
  "genre": "RPG",
  "releaseYear": 2024
}
```

**レスポンス:**

- `200 OK`: 更新成功
- `400 Bad Request`: バリデーションエラー
- `404 Not Found`: 指定されたIDのゲームが存在しない場合

#### ゲーム削除

```http
DELETE /api/games/{id}
```

**レスポンス:**

- `204 No Content`: 削除成功
- `404 Not Found`: 指定されたIDのゲームが存在しない場合

### WeatherForecast (サンプル機能)

```http
GET /api/weather-forecast
```

5日間の天気予報データを返します（サンプル機能）。

## テスト

### テストの実行

```bash
dotnet test
```

### テストカバレッジ

プロジェクトには以下のテストが含まれています：

- **GetAllGamesTests**: 全ゲーム取得のテスト（2件）
- **GetGameByIdTests**: ID指定取得のテスト（2件）
- **CreateGameTests**: ゲーム作成のテスト（7件、バリデーション含む）
- **UpdateGameTests**: ゲーム更新のテスト（4件、バリデーション含む）
- **DeleteGameTests**: ゲーム削除のテスト（2件）

**合計: 17件のテスト**

### テストの特徴

- **InMemory Database**: 各テストで独立したデータベースを使用
- **FluentAssertions**: 読みやすいアサーション
- **包括的なカバレッジ**: 正常系・異常系・バリデーションを網羅

## プロジェクト構成

```txt
VideoGameApiVsa/
├── Behaviors/
│   └── ValidationBehavior.cs          # MediatR Pipeline Behavior（自動バリデーション）
├── Data/
│   └── VideoGameDbContext.cs          # Entity Framework DbContext
├── Entities/
│   └── VideoGame.cs                   # エンティティ定義
├── Features/
│   ├── VideoGames/                    # ビデオゲーム機能
│   │   ├── CreateGame.cs
│   │   ├── DeleteGame.cs
│   │   ├── GetAllGames.cs
│   │   ├── GetGameById.cs
│   │   ├── UpdateGame.cs
│   │   ├── VideoGameModule.cs
│   │   └── VideoGameRouteNames.cs
│   └── WeatherForecast/               # サンプル機能
│       ├── GetWeatherForecast.cs
│       └── WeatherForecastModule.cs
├── Properties/
│   └── launchSettings.json
├── Program.cs                          # アプリケーションエントリーポイント
└── VideoGameApiVsa.csproj

VideoGameApiVsa.Tests/
└── Features/
    └── VideoGames/                    # テストクラス
        ├── CreateGameTests.cs
        ├── DeleteGameTests.cs
        ├── GetAllGamesTests.cs
        ├── GetGameByIdTests.cs
        └── UpdateGameTests.cs
```

## 開発ガイドライン

### 新機能の追加

1. **Featuresフォルダに新しいファイルを作成**

```csharp
public static class NewFeature
{
    // Request DTO
    public record NewFeatureRequest(...);
    
    // Command/Query
    public record NewFeatureCommand(...) : IRequest<NewFeatureResponse>;
    
    // Response DTO
    public record NewFeatureResponse(...);
    
    // Validator (必要に応じて)
    public class Validator : AbstractValidator<NewFeatureCommand> { }
    
    // Handler
    public class Handler(...) : IRequestHandler<NewFeatureCommand, NewFeatureResponse> { }
    
    // Endpoint
    public static async Task<IResult> Endpoint(...) { }
}
```

2. **Moduleにルートを追加**

```csharp
group.MapPost("/new-feature", NewFeature.Endpoint)
    .WithName("NewFeature")
    .WithDescription("Description")
    .Produces<NewFeature.NewFeatureResponse>(StatusCodes.Status200OK);
```

3. **テストを追加**

### コーディング規約

- **命名規則**: PascalCase（クラス、メソッド）、camelCase（ローカル変数）
- **XMLコメント**: すべての公開メンバーにXMLコメントを追加
- **null安全性**: nullable参照型を有効化し、適切にnullチェック
- **非同期処理**: I/O操作は必ずasync/awaitを使用

### エラーハンドリング

- **バリデーションエラー**: FluentValidationが自動的に検出し、400 Bad Requestを返却
- **未処理例外**: グローバル例外ハンドラでキャッチし、適切なHTTPステータスコードを返却
- **ProblemDetails**: RFC 7807準拠のエラーレスポンス形式
