namespace CineVault.API.Controllers.Requests;

public sealed class CreateActorRequest
{
    public required string FullName { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? Biography { get; init; }
}