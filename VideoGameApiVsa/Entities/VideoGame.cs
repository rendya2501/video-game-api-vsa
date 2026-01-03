namespace VideoGameApiVsa.Entities;

public class VideoGame
{
    public int Id { get; set; }
    public required string Title { get; set; } = string.Empty;
    public required string Genre { get; set; } = string.Empty;
    public required int ReleaseYear { get; set; }
}
