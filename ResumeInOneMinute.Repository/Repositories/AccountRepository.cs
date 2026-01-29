using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ResumeInOneMinute.Repository.Repositories;

public class AccountRepository : BaseRepository, IAccountRepository
{
    private readonly ResumeInOneMinute.Infrastructure.CommonServices.EncryptionHelper _encryptionHelper;
    private readonly IEmailService _emailService;

    public AccountRepository(IConfiguration configuration, ResumeInOneMinute.Infrastructure.CommonServices.EncryptionHelper encryptionHelper, IEmailService emailService) : base(configuration)
    {
        _encryptionHelper = encryptionHelper;
        _emailService = emailService;
    }

    public async Task<Response<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            var email = registerDto.Email.Trim().ToLower();

            // Check if email already exists
            if (await EmailExistsAsync(email))
            {
                return new Response<AuthResponseDto>
                {
                    Status = false,
                    Message = "Email already exists",
                    Data = null!
                };
            }

            // Create password hash and salt

            using (var context = CreateDbContext())
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var refreshToken = GenerateRefreshToken();
                    CreateHash(refreshToken, out byte[] refreshTokenHash, out byte[] refreshTokenSalt);
                    CreateHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    var user = new User
                    {
                        Email = email,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        IsActive = true,
                        RefreshTokenHash = refreshTokenHash,
                        RefreshTokenSalt = refreshTokenSalt,
                        RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Users.Add(user);
                    await context.SaveChangesAsync();

                    // Create user profile
                    var userProfile = new UserProfile
                    {
                        UserId = user.UserId,
                        FirstName = registerDto.FirstName.Trim(),
                        LastName = registerDto.LastName.Trim(),
                        Phone = string.IsNullOrWhiteSpace(registerDto.PhoneNumber) 
                            ? null 
                            : registerDto.PhoneNumber.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        // GlobalUserProfileId = Guid.NewGuid()
                    };

                    context.UserProfiles.Add(userProfile);
                    await context.SaveChangesAsync();

                    // Generate Refresh Token
                    
                    await context.SaveChangesAsync();

                    // specific Commit
                    await transaction.CommitAsync();

                    // Generate JWT token
                    var token = GenerateJwtToken(user);
                    var encryptedToken = _encryptionHelper.EncryptPersistent(token); // Encrypting the token

                    // Prepare response
                    var userDto = new UserDto
                    {
                        UserId = user.UserId,
                        GlobalUserId = user.GlobalUserId,
                        Email = user.Email,
                        FirstName = userProfile.FirstName,
                        LastName = userProfile.LastName,
                        PhoneNumber = userProfile.Phone,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    };

                    var authResponse = new AuthResponseDto
                    {
                        Token = encryptedToken,
                        RefreshToken = refreshToken,
                        User = userDto
                    };

                    return new Response<AuthResponseDto>
                    {
                        Status = true,
                        Message = "Registration successful",
                        Data = authResponse
                    };
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.InnerException != null 
                ? $"Registration failed: {ex.Message} Inner Exception: {ex.InnerException.Message}" 
                : $"Registration failed: {ex.Message}";

            return new Response<AuthResponseDto>
            {
                Status = false,
                Message = errorMessage,
                Data = null!
            };
        }
    }

    public async Task<Response<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            using (var context = CreateDbContext())
            {
                var email = loginDto.Email.Trim().ToLower();

                // Get user by email with profile
                var user = await context.Users
                    .Include(u => u.UserProfile)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return new Response<AuthResponseDto>
                    {
                        Status = false,
                        Message = "Invalid email or password",
                        Data = null!
                    };
                }

                // Verify password
                if (!VerifyHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return new Response<AuthResponseDto>
                    {
                        Status = false,
                        Message = "Invalid email or password",
                        Data = null!
                    };
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return new Response<AuthResponseDto>
                    {
                        Status = false,
                        Message = "Account is inactive. Please contact support.",
                        Data = null!
                    };
                }

                // Update last login time (using UpdatedAt field)
                user.UpdatedAt = DateTime.UtcNow;
                
                // Generate Refresh Token
                var refreshToken = GenerateRefreshToken();
                CreateHash(refreshToken, out byte[] refreshTokenHash, out byte[] refreshTokenSalt);
                user.RefreshTokenHash = refreshTokenHash;
                user.RefreshTokenSalt = refreshTokenSalt;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                
                await context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var encryptedToken = _encryptionHelper.EncryptPersistent(token); // Encrypting the token

                // Prepare response
                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    GlobalUserId = user.GlobalUserId,
                    Email = user.Email,
                    FirstName = user.UserProfile?.FirstName,
                    LastName = user.UserProfile?.LastName,
                    PhoneNumber = user.UserProfile?.Phone,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                var authResponse = new AuthResponseDto
                {
                    Token = encryptedToken,
                    RefreshToken = refreshToken,
                    User = userDto
                };

                return new Response<AuthResponseDto>
                {
                    Status = true,
                    Message = "Login successful",
                    Data = authResponse
                };
            }
        }
        catch (Exception ex)
        {
            return new Response<AuthResponseDto>
            {
                Status = false,
                Message = $"Login failed: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        using (var context = CreateDbContext())
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
        }
    }

    public async Task<User?> GetUserByIdAsync(long userId)
    {
        using (var context = CreateDbContext())
        {
            return await context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using (var context = CreateDbContext())
        {
            return await context.Users
                .AnyAsync(u => u.Email == email.Trim().ToLower());
        }
    }

    public async Task<Response<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto tokenDto)
    {
        string? accessToken = tokenDto.AccessToken;
        string? refreshToken = tokenDto.RefreshToken;

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return new Response<AuthResponseDto> { Status = false, Message = "Invalid access token or refresh token" };
        }

        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
        {
            return new Response<AuthResponseDto> { Status = false, Message = "Invalid user identity" };
        }

        using (var context = CreateDbContext())
        {
            var user = await context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || !VerifyHash(refreshToken, user.RefreshTokenHash ?? Array.Empty<byte>(), user.RefreshTokenSalt ?? Array.Empty<byte>()) || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new Response<AuthResponseDto> { Status = false, Message = "Invalid access token or refresh token" };
            }

            var newAccessToken = GenerateJwtToken(user);
            var encryptedToken = _encryptionHelper.EncryptPersistent(newAccessToken); // Encrypting the token
            
            var newRefreshToken = GenerateRefreshToken();
            CreateHash(newRefreshToken, out byte[] newRefreshTokenHash, out byte[] newRefreshTokenSalt);

            user.RefreshTokenHash = newRefreshTokenHash;
            user.RefreshTokenSalt = newRefreshTokenSalt;
            // Configurable expiry, e.g., 7 days
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); 
            
            await context.SaveChangesAsync();

            var userDto = new UserDto
            {
                UserId = user.UserId,
                GlobalUserId = user.GlobalUserId,
                Email = user.Email,
                FirstName = user.UserProfile?.FirstName,
                LastName = user.UserProfile?.LastName,
                PhoneNumber = user.UserProfile?.Phone,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return new Response<AuthResponseDto>
            {
                Status = true,
                Message = "Token refreshed successfully",
                Data = new AuthResponseDto
                {
                    Token = encryptedToken,
                    RefreshToken = newRefreshToken,
                    User = userDto
                }
            };
        }
    }

    public async Task<Response<string>> ForgotPasswordAsync(string email)
    {
        try
        {
            using (var context = CreateDbContext())
            {
                var normalizedEmail = email.Trim().ToLower();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

                if (user == null)
                {
                    return new Response<string> { Status = false, Message = "User not found" };
                }

                // Generate OTP (6 digits)
                var otp = new Random().Next(100000, 999999).ToString();
                
                user.OtpCode = otp;
                user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(10); // OTP valid for 10 minutes

                await context.SaveChangesAsync();

                // Send OTP via Email
                await _emailService.SendEmailAsync(user.Email, "Reset Password OTP", $"Your OTP for resetting your password is: {otp}");

                return new Response<string>
                {
                    Status = true,
                    Message = "OTP sent to your email",
                    Data = "OTP Sent"
                };
            }
        }
        catch (Exception ex)
        {
             return new Response<string> { Status = false, Message = $"Error sending OTP: {ex.Message}" };
        }
    }

    public async Task<Response<string>> LogoutAsync(long userId)
    {
        try
        {
            using (var context = CreateDbContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return new Response<string> { Status = false, Message = "User not found" };
                }

                // Clear Refresh Token
                user.RefreshTokenHash = null;
                user.RefreshTokenSalt = null;
                user.RefreshTokenExpiryTime = null;

                await context.SaveChangesAsync();

                return new Response<string>
                {
                    Status = true,
                    Message = "Logout successful"
                };
            }
        }
        catch (Exception ex)
        {
            return new Response<string> { Status = false, Message = $"Logout failed: {ex.Message}" };
        }
    }

    public async Task<Response<UserDto>> UpdateProfileAsync(long userId, ProfileUpdateDto profileUpdateDto)
    {
        try
        {
            using (var context = CreateDbContext())
            {
                var user = await context.Users
                    .Include(u => u.UserProfile)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return new Response<UserDto> { Status = false, Message = "User not found", Data = null! };
                }

                // Update Profile
                if (user.UserProfile == null)
                {
                    // Should not happen if registered correctly, but handle just in case
                    user.UserProfile = new UserProfile
                    {
                        UserId = userId,
                        GlobalUserProfileId = Guid.NewGuid()
                    };
                    context.UserProfiles.Add(user.UserProfile);
                }

                user.UserProfile.FirstName = profileUpdateDto.FirstName.Trim();
                user.UserProfile.LastName = profileUpdateDto.LastName.Trim();
                user.UserProfile.Phone = profileUpdateDto.PhoneNumber?.Trim();
                user.UserProfile.CountryCode = profileUpdateDto.CountryCode?.Trim();

                await context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    GlobalUserId = user.GlobalUserId,
                    Email = user.Email,
                    FirstName = user.UserProfile.FirstName,
                    LastName = user.UserProfile.LastName,
                    PhoneNumber = user.UserProfile.Phone,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return new Response<UserDto>
                {
                    Status = true,
                    Message = "Profile updated successfully",
                    Data = userDto
                };
            }
        }
        catch (Exception ex)
        {
            return new Response<UserDto> { Status = false, Message = $"Profile update failed: {ex.Message}", Data = null! };
        }
    }

    #region Private Helper Methods

    private void CreateHash(string text, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
    }

    private bool VerifyHash(string text, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
        return computedHash.SequenceEqual(storedHash);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("GlobalUserId", user.GlobalUserId.ToString())
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
            SigningCredentials = credentials,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
    
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var jwtSettings = Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false // Allow expired tokens here
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    #endregion
}
