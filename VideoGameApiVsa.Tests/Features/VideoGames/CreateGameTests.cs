using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VideoGameApiVsa.Data;
using VideoGameApiVsa.Features.VideoGames;

namespace VideoGameApiVsa.Tests.Features.VideoGames;

/// <summary>
/// CreateGame機能のテストクラス
/// </summary>
public class CreateGameTests
{
    /// <summary>
    /// 有効なコマンドの場合、ゲームを作成することを確認するテスト
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateGame_WhenValidCommand()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<VideoGameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new VideoGameDbContext(options);
        var handler = new CreateGame.Handler(dbContext);
        var command = new CreateGame.CreateGameCommand("New Game", "Action", 2023);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("New Game");
        result.Genre.Should().Be("Action");
        result.ReleaseYear.Should().Be(2023);

        var gameInDb = await dbContext.VideoGames.FindAsync([result.Id]);
        gameInDb.Should().NotBeNull();
        gameInDb!.Title.Should().Be("New Game");
    }

    /// <summary>
    /// タイトルが空の場合、バリデーションが失敗することを確認するテスト
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var command = new CreateGame.CreateGameCommand("", "Action", 2023);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    /// <summary>
    /// 境界値テスト：タイトルちょうど100文字
    /// </summary>
    [Fact]
    public void Validator_ShouldPass_WhenTitleIsExactly100Characters()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var title = new string('A', 100); // ちょうど100文字
        var command = new CreateGame.CreateGameCommand(title, "Action", 2023);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// タイトルが最大長を超える場合、バリデーションが失敗することを確認するテスト
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var longTitle = new string('A', 101); // 101文字
        var command = new CreateGame.CreateGameCommand(longTitle, "Action", 2023);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    /// <summary>
    /// ジャンルが空の場合、バリデーションが失敗することを確認するテスト
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenGenreIsEmpty()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var command = new CreateGame.CreateGameCommand("Test Game", "", 2023);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Genre");
    }

    /// <summary>
    /// ジャンルの最大長（50文字）超過テスト
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenGenreExceedsMaxLength()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var longGenre = new string('A', 51); // 51文字
        var command = new CreateGame.CreateGameCommand("Test Game", longGenre, 2023);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Genre");
    }

    /// <summary>
    /// リリース年が1950年より前の場合、バリデーションが失敗することを確認するテスト
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenReleaseYearIsBefore1950()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var command = new CreateGame.CreateGameCommand("Test Game", "Action", 1949);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ReleaseYear");
    }

    /// <summary>
    /// 境界値テスト：リリース年が1950年（境界値）
    /// </summary>
    [Fact]
    public void Validator_ShouldPass_WhenReleaseYearIs1950()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var command = new CreateGame.CreateGameCommand("Test Game", "Action", 1950);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }


    /// <summary>
    /// 境界値テスト：リリース年が今年（境界値）
    /// </summary>
    [Fact]
    public void Validator_ShouldPass_WhenReleaseYearIsCurrentYear()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var command = new CreateGame.CreateGameCommand("Test Game", "Action", DateTime.Now.Year);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// リリース年が未来の場合、バリデーションが失敗することを確認するテスト
    /// </summary>
    [Fact]
    public void Validator_ShouldFail_WhenReleaseYearIsInFuture()
    {
        // Arrange
        var validator = new CreateGame.Validator();
        var futureYear = DateTime.Now.Year + 1;
        var command = new CreateGame.CreateGameCommand("Test Game", "Action", futureYear);

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
        var validator = new CreateGame.Validator();
        var command = new CreateGame.CreateGameCommand("Valid Game", "RPG", 2020);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

