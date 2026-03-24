using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[Route("api/[controller]/[action]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepository;
    private readonly ILogger<UsersController> logger;

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
    {
        this.userRepository = userRepository;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        this.logger.LogInformation("Fetching all users from the database.");
        var users = await this.userRepository.GetAll();
        var response = users.Select(UserResponse.FromEntity);
        this.logger.LogInformation("Retrieved {UserCount} users successfully.", response.Count());
        return base.Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        this.logger.LogInformation("User {UserId} requested.", id);
        var user = await this.userRepository.GetById(id);
        if (user is null)
        {
            this.logger.LogWarning("User {UserId} not found.", id);
            return base.NotFound();
        }
        this.logger.LogInformation("User {UserId} retrieved successfully.", id);
        return base.Ok(UserResponse.FromEntity(user));
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        this.logger.LogInformation("Creating a new user with username {Username}.", request.Username);
        var user = request.ToEntity();
        await this.userRepository.Create(user);
        this.logger.LogInformation("User {Username} created successfully.", request.Username);
        return base.Ok();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        this.logger.LogInformation("Updating user {UserId}.", id);
        var user = await this.userRepository.GetById(id);
        if (user is null)
        {
            this.logger.LogWarning("User {UserId} not found for update.", id);
            return base.NotFound();
        }
        request.ApplyTo(user);
        await this.userRepository.Update(user);
        this.logger.LogInformation("User {UserId} updated successfully.", id);
        return base.Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        this.logger.LogInformation("Deleting user {UserId}.", id);
        var user = await this.userRepository.GetById(id);
        if (user is null)
        {
            this.logger.LogWarning("User {UserId} not found for deletion.", id);
            return base.NotFound();
        }
        await this.userRepository.Delete(user);
        this.logger.LogInformation("User {UserId} deleted successfully.", id);
        return base.NoContent();
    }
}