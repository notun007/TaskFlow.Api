using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Api.Controllers;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record RequestPasswordResetRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
public sealed record CurrentUserResponse(Guid Id, string Email, string DisplayName, string? Department, string? JobTitle, IReadOnlyList<string> Roles);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(UserManager<ApplicationUser> users, IConfiguration configuration) : ControllerBase
{
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var user = await users.FindByEmailAsync(User.Identity?.Name ?? string.Empty);
        if (user is null || !user.IsActive) return Unauthorized();

        var roles = await users.GetRolesAsync(user);
        return Ok(new CurrentUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName ?? user.Email ?? "TaskFlow user",
            user.Department,
            user.JobTitle,
            roles.ToArray()));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email, DisplayName = request.DisplayName };
        var result = await users.CreateAsync(user, request.Password);
        return result.Succeeded ? Ok(new { user.Id, user.Email, user.DisplayName }) : BadRequest(result.Errors.Select(error => error.Description));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive || !await users.CheckPasswordAsync(user, request.Password)) return Unauthorized(new { message = "Invalid credentials." });
        var roles = await users.GetRolesAsync(user);
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(ClaimTypes.Name, user.Email ?? user.UserName ?? user.Id.ToString()), new(ClaimTypes.Email, user.Email ?? string.Empty) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return Ok(new { accessToken = new JwtSecurityTokenHandler().WriteToken(token), expiresAt = token.ValidTo });
    }

    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var user = await users.FindByEmailAsync(User.Identity?.Name ?? string.Empty);
        if (user is null) return Unauthorized();
        var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return result.Succeeded ? NoContent() : BadRequest(result.Errors.Select(error => error.Description));
    }

    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);
        string? resetToken = null;
        if (user is not null && user.IsActive)
            resetToken = await users.GeneratePasswordResetTokenAsync(user);

        // An email provider should deliver this token in production. Returning it is limited to Development.
        return Ok(new
        {
            message = "If the account exists, password reset instructions have been generated.",
            resetToken = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? resetToken : null
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null) return BadRequest(new { message = "The reset request is invalid or expired." });
        var result = await users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded ? NoContent() : BadRequest(result.Errors.Select(error => error.Description));
    }

}
