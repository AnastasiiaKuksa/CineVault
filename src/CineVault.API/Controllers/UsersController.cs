using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[Route("api/[controller]/[action]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepository;

    public UsersController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        var users = await this.userRepository.GetAll();

        var response = users.Select(UserResponse.FromEntity);

        return base.Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        var user = await this.userRepository.GetById(id);

        if (user is null)
        {
            return base.NotFound();
        }

        return base.Ok(UserResponse.FromEntity(user));
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        var user = request.ToEntity();

        await this.userRepository.Create(user);

        return base.Ok();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        var user = await this.userRepository.GetById(id);

        if (user is null)
        {
            return base.NotFound();
        }

        request.ApplyTo(user);

        await this.userRepository.Update(user);

        return base.Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var user = await this.userRepository.GetById(id);

        if (user is null)
        {
            return base.NotFound();
        }

        await this.userRepository.Delete(user);

        return base.NoContent();
    }
}