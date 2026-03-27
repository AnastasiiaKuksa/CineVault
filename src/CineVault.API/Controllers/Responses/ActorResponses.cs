namespace CineVault.API.Controllers.Responses;

public sealed class ActorResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string? Biography { get; set; }
    public List<string> Movies { get; set; } = [];
}

public sealed class ActorCreatedResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
}