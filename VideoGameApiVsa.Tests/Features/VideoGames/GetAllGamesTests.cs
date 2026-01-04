using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VideoGameApiVsa.Data;
using VideoGameApiVsa.Entities;
using VideoGameApiVsa.Features.VideoGames;

namespace VideoGameApiVsa.Tests.Features.VideoGames;

/// <summary>
/// GetAllGames機能のテストクラス
/// </summary>
public class GetAllGamesTests
{
    /// <summary>
    /// ゲームが存在する場合、すべてのゲームを返すことを確認するテスト
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllGames_WhenGamesExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VideoGameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new VideoGameDbContext(options);
        dbContext.VideoGames.AddRange(
            new VideoGame { Id = 1, Title = "Game 1", Genre = "Action", ReleaseYear = 2020 },
            new VideoGame { Id = 2, Title = "Game 2", Genre = "RPG", ReleaseYear = 2021 }
        );
        await dbContext.SaveChangesAsync();

        var handler = new GetAllGames.Handler(dbContext);
        var query = new GetAllGames.GetAllGamesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(g => g.Title == "Game 1" && g.Genre == "Action");
        result.Should().Contain(g => g.Title == "Game 2" && g.Genre == "RPG");
    }

    /// <summary>
    /// ゲームが存在しない場合、空のリストを返すことを確認するテスト
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoGamesExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VideoGameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new VideoGameDbContext(options);
        var handler = new GetAllGames.Handler(dbContext);
        var query = new GetAllGames.GetAllGamesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// 大量データのテスト（パフォーマンス確認）
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllGames_WhenManyGamesExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VideoGameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new VideoGameDbContext(options);

        // 100件のゲームを追加
        var games = Enumerable.Range(1, 100).Select(i => new VideoGame
        {
            Id = i,
            Title = $"Game {i}",
            Genre = "Action",
            ReleaseYear = 2020
        });
        dbContext.VideoGames.AddRange(games);
        await dbContext.SaveChangesAsync();

        var handler = new GetAllGames.Handler(dbContext);
        var query = new GetAllGames.GetAllGamesQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(100);
    }
}

