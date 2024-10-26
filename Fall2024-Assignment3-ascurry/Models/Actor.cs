namespace Fall2024_Assignment3_ascurry.Models;

public enum Gender
{
    M, F
}

public class Actor
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public Gender? Gender { get; set; }

    public int? Age { get; set; }

    public string? IMDBLink { get; set; }

    public byte[]? Photo { get; set; }

}

