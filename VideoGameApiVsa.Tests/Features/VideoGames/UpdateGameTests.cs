using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VideoGameApiVsa.Data;
using VideoGameApiVsa.Entities;
using VideoGameApiVsa.Features.VideoGames;

namespace VideoGameApiVsa.Tests.Features.VideoGames;

/// <summary>
/// UpdateGame機能のテストクラス
/// </summary>
public class UpdateGameTests
{
    /// <summary>
    /// ゲームが存在する場合、ゲームを更新することを確認するテスト
    /// </summary>
    [Fact]
    public async Task Handle_ShouldUpdateGame_WhenGameExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VideoGameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new VideoGameDbContext(options);
        var game = new VideoGame { Id = 1, Title = "Original Title", Genre = "Action", ReleaseYear = 2020 };
        dbContext.VideoGames.Add(game);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateGame.Handler(dbContext);
        var command = new UpdateGame.UpdateGameCommand(1, "Updated Title", "RPG", 2021);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Title.Should().Be("Updated Title");
        result.Genre.Should().Be("RPG");
        result.ReleaseYear.Should().Be(2021);

        var gameInDb = await dbContext.VideoGames.FindAsync([1]);
        gameInDb.Should().NotBeNull();
        gameInDb!.Title.Should().Be("Updated Title");
        gameInDb.Genre.Should().Be("RPG");
        gameInDb.ReleaseYear.Should().Be(2021);
    }

    /// <summary>
    /// ゲームが存在しない場合、nullを返すことを確認するテスト
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenGameDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VideoGameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new VideoGameDbContext(options);
        var handler = new UpdateGame.Handler(dbContext);
        var command = new UpdateGame.UpdateGameCommand(999, "Updated Title", "RPG", 2021);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// タイトルが空の場合、バリデーションが失敗することを確認するテスト
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var validator = new UpdateGame.Validator();
        var command = new UpdateGame.UpdateGameCommand(1, "", "Action", 2023);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    /// <summary>
    /// タイトル最大長超過
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var validator = new UpdateGame.Validator();
        var longTitle = new string('A', 101);
        var command = new UpdateGame.UpdateGameCommand(1, longTitle, "Action", 2023);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    /// <summary>
    /// ジャンル空のテスト 
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenGenreIsEmpty()
    {
        // Arrange
        var validator = new UpdateGame.Validator();
        var command = new UpdateGame.UpdateGameCommand(1, "Test Game", "", 2023);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Genre");
    }

    /// <summary>
    /// リリース年が未来
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenReleaseYearIsInFuture()
    {
        // Arrange
        var validator = new UpdateGame.Validator();
        var futureYear = DateTime.Now.Year + 1;
        var command = new UpdateGame.UpdateGameCommand(1, "Test Game", "Action", futureYear);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ReleaseYear");
    }

    /// <summary>
    /// すべてのフィールドが有効な場合、バリデーションが成功することを確認するテスト
    /// </summary>
    [Fact]
    public void Validator_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var validator = new UpdateGame.Validator();
        var command = new UpdateGame.UpdateGameCommand(1, "Valid Game", "RPG", 2020);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

