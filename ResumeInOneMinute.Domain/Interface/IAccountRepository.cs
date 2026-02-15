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
    Task<Response<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto tokenDto);
    Task<Response<string>> ForgotPasswordAsync(string email);
    Task<Response<string>> LogoutAsync(long userId);
    Task<Response<UserDto>> UpdateProfileAsync(long userId, ProfileUpdateDto profileUpdateDto);
    Task<Response<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto googleLoginDto);
    Task<Response<string>> GetGoogleClientIdAsync();
}
