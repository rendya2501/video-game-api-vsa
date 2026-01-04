using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using VideoGameApiVsa.Features.VideoGames;

namespace VideoGameApiVsa.Tests.Features.VideoGames;

/// <summary>
/// VideoGamesの統合テストクラス
/// </summary>
/// <typeparam name="Program"></typeparam>
public class VideoGamesIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    /// <summary>
    /// 有効なリクエストの場合、ゲームを作成することを確認するテスト
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateGame_ShouldReturn201_WhenValidRequest()
    {
        // Arrange
        var request = new CreateGame.CreateGameRequest("Test Game", "Action", 2023);

        // Act
        var response = await _client.PostAsJsonAsync("/api/games", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    /// <summary>
    /// 無効なリクエストの場合、400 Bad Requestを返すことを確認するテスト
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CreateGame_ShouldReturn400_WhenInvalidRequest()
    {
        // Arrange
        var request = new CreateGame.CreateGameRequest("", "Action", 2023); // タイトル空

        // Act
        var response = await _client.PostAsJsonAsync("/api/games", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// 有効なリクエストの場合、ゲーム一覧を取得することを確認するテスト
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetAllGames_ShouldReturn200_WithGames()
    {
        // Act
        var response = await _client.GetAsync("/api/games");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var games = await response.Content.ReadFromJsonAsync<List<GetAllGames.GetAllGamesResponse>>();
        games.Should().NotBeNull();
        games.Should().HaveCountGreaterThan(0); // 初期シードデータ
    }

    /// <summary>
    /// 有効なリクエストの場合、ゲームを取得することを確認するテスト
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetGameById_ShouldReturn404_WhenGameNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/games/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// 有効なリクエストの場合、ゲームを削除することを確認するテスト
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DeleteGame_ShouldReturn404_WhenGameNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/games/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}