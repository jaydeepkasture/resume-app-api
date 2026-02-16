using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface IAccountRepository
{
    Task<Response<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<Response<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(long userId);
    Task<bool> EmailExistsAsync(string email);
    Task<Response<AuthResponseDto>> RefreshTokenAsync(string accessToken, string encryptedRefreshToken);
    Task<Response<string>> ForgotPasswordAsync(string email);
    Task<Response<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<Response<string>> LogoutAsync(System.Security.Claims.ClaimsPrincipal user);
    Task<Response<UserDto>> UpdateProfileAsync(System.Security.Claims.ClaimsPrincipal userPrincipal, ProfileUpdateDto profileUpdateDto);
    Task<Response<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto googleLoginDto);
    Task<Response<string>> GetGoogleClientIdAsync();
}
