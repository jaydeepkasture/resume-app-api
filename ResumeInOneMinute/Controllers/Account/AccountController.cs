using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Controllers.Super;
using ResumeInOneMinute.Infrastructure.CommonServices;

namespace ResumeInOneMinute.Controllers.Account;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/account")]
[ApiController]
[Produces("application/json")]
public class AccountController : SuperController
{
    private readonly IAccountRepository _accountRepository;
    private readonly EncryptionHelper _encryptionHelper;

    public AccountController(IAccountRepository accountRepository, 
        EncryptionHelper encryptionHelper)
    {
        _accountRepository = accountRepository;
        _encryptionHelper = encryptionHelper;
    }

    [HttpGet("google/clientid")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGoogleClientId()
    {
        var result = await _accountRepository.GetGoogleClientIdAsync();

        if (!result.Status)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }


    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Status = false, Message = "Validation failed", Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        var result = await _accountRepository.RegisterAsync(registerDto);

        if (!result.Status)
        {
            return BadRequest(result);
        }

        // Set Refresh Token in Cookie (Encrypted)
        SetRefreshTokenCookie(result.Data.RefreshToken);

        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Status = false, Message = "Validation failed", Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        var result = await _accountRepository.LoginAsync(loginDto);

        if (!result.Status)
        {
            return Unauthorized(result);
        }

        // Set Refresh Token in Cookie (Encrypted)
        SetRefreshTokenCookie(result.Data.RefreshToken);

        return Ok(result);
    }

    [HttpPost("google/verify-token")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleTokenVerify([FromBody] GoogleLoginDto googleLoginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Status = false, Message = "Validation failed", Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        var result = await _accountRepository.GoogleLoginAsync(googleLoginDto);

        if (!result.Status)
        {
            return BadRequest(result);
        }

        // Set Refresh Token in Cookie (Encrypted)
        SetRefreshTokenCookie(result.Data.RefreshToken);

        return Ok(result);
    }

    [HttpGet("refresh-token")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken()
    {
        // 1. Get Access Token from Header (Bearer token)
        string? accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
        
        // 2. Refresh Token comes from Cookie
        var encryptedRefreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(encryptedRefreshToken))
        {
            return BadRequest(new { Status = false, Message = "Access token (Header) and Refresh token (Cookie) are required" });
        }

        var result = await _accountRepository.RefreshTokenAsync(accessToken, encryptedRefreshToken);

        if (!result.Status)
        {
            return BadRequest(result);
        }

        // Set New Refresh Token in Cookie (Encrypted)
        SetRefreshTokenCookie(result.Data.RefreshToken);

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Status = false, Message = "Validation failed", Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        var result = await _accountRepository.ForgotPasswordAsync(forgotPasswordDto.Email);

        if (!result.Status)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Status = false, Message = "Validation failed", Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        var result = await _accountRepository.ResetPasswordAsync(resetPasswordDto);

        if (!result.Status)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var result = await _accountRepository.LogoutAsync(User);
        
        // Clear Refresh Token Cookie
        Response.Cookies.Delete("refreshToken");

        if (!result.Status)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto profileUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Status = false, Message = "Validation failed", Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        var result = await _accountRepository.UpdateProfileAsync(User, profileUpdateDto);

        if (!result.Status)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Ensure this is true for production (and usually localhost too)
            SameSite = SameSiteMode.Strict,
            Path = "/", // Allow cookie to be sent to all paths (needed for versioned API)
            Expires = DateTime.UtcNow.AddDays(7)
        };

        // The token coming from the repository is already encrypted using EncryptTemporary
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
