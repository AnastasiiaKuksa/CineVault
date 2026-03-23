using CineVault.API.Data.Entities;

namespace CineVault.API.Controllers.Requests;

public sealed class UserRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }

    public User ToEntity()
    {
        return new User
        {
            Username = Username,
            Email = Email,
            Password = Password
        };
    }

    public void ApplyTo(User user)
    {
        user.Username = Username;
        user.Email = Email;
        if (!string.IsNullOrWhiteSpace(Password))
        {
            user.Password = Password;
        }
    }
}