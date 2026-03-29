namespace CineVault.API.Data.Entities;

public sealed class Actor
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Biography { get; set; }
    public bool IsDeleted { get; set; } = false;
    public ICollection<MovieActor> MovieActors { get; set; } = [];
}