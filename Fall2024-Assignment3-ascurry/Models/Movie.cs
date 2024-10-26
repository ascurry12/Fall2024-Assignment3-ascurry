namespace Fall2024_Assignment3_ascurry.Models;

public class Movie
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public string? IMDBLink { get; set; }

    public string? Genre { get; set; }

    public int? ReleaseYear { get; set; }

    public byte[]? Poster { get; set; }

}

