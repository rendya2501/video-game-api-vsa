using VideoGameApiVsa.Entities;

namespace VideoGameApiVsa.Data;

/// <summary>
/// VideoGameDbContextのシードデータを初期化するクラス
/// </summary>
public static class VideoGameDbContextSeed
{
    /// <summary>
    /// データベースに初期データをシードします。
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task SeedAsync(VideoGameDbContext context)
    {
        if (!context.VideoGames.Any())
        {
            await context.VideoGames.AddRangeAsync(
                new VideoGame { Id = 1, Genre = "Action", Title = "The Legend of Zelda: Breath of the Wild", ReleaseYear = 2017 },
                new VideoGame { Id = 2, Genre = "RPG", Title = "The Witcher 3: Wild Hunt", ReleaseYear = 2015 },
                new VideoGame { Id = 3, Genre = "Shooter", Title = "DOOM Eternal", ReleaseYear = 2020 },
                new VideoGame { Id = 4, Genre = "Adventure", Title = "Red Dead Redemption 2", ReleaseYear = 2018 },
                new VideoGame { Id = 5, Genre = "Strategy", Title = "Civilization VI", ReleaseYear = 2016 }
            );
            await context.SaveChangesAsync();
        }
    }
}