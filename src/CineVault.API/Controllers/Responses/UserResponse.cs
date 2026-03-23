using CineVault.API.Data.Entities;

namespace CineVault.API.Controllers.Responses;

public sealed class UserResponse
{
    public required int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }

    public static UserResponse FromEntity(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }
}