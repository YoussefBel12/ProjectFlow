
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Api.Contracts.Auth;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Infrastructure.Identity;

namespace ProjectFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(error => error.Description)
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = _tokenGenerator.GenerateToken(
            user.Id.ToString(),
            user.UserName!,
            user.Email!,
            roles);

        return Ok(new
        {
            userId = user.Id,
            userName = user.UserName,
            email = user.Email,
            displayName = user.DisplayName,
            token
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null ||
            !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = _tokenGenerator.GenerateToken(
            user.Id.ToString(),
            user.UserName!,
            user.Email!,
            roles);

        return Ok(new
        {
            userId = user.Id,
            userName = user.UserName,
            email = user.Email,
            displayName = user.DisplayName,
            token
        });
    }
}
