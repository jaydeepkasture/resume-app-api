using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ResumeInOneMinute.Infrastructure.CommonServices;

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
                        RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(Configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7")),
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
                        RefreshToken = _encryptionHelper.EncryptTemporary(refreshToken),
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
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(Configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7"));
                
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
                    RefreshToken = _encryptionHelper.EncryptTemporary(refreshToken),
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

    public async Task<Response<AuthResponseDto>> RefreshTokenAsync(string accessToken, string encryptedRefreshToken)
    {
        try
        {
            string refreshToken;
            try 
            {
                refreshToken = _encryptionHelper.DecryptTemporary(encryptedRefreshToken);
            }
            catch (Exception)
            {
                return new Response<AuthResponseDto> { Status = false, Message = "Refresh token decryption failed. Please login again." };
            }
            
            ClaimsPrincipal? principal;
            try 
            {
                principal = GetPrincipalFromExpiredToken(accessToken);
            }
            catch (Exception ex)
            {
                return new Response<AuthResponseDto> { Status = false, Message = $"Access token validation failed: {ex.Message}" };
            }

            if (principal == null)
            {
                return new Response<AuthResponseDto> { Status = false, Message = "Could not verify access token identity." };
            }

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return new Response<AuthResponseDto> { Status = false, Message = "User ID claim missing in token." };
            }

            using (var context = CreateDbContext())
            {
                var user = await context.Users
                    .Include(u => u.UserProfile)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return new Response<AuthResponseDto> { Status = false, Message = "User associated with token not found." };
                }

                if (user.RefreshTokenHash == null || user.RefreshTokenSalt == null)
                {
                    return new Response<AuthResponseDto> { Status = false, Message = "No refresh token found in database for this user." };
                }

                if (!VerifyHash(refreshToken, user.RefreshTokenHash, user.RefreshTokenSalt))
                {
                    return new Response<AuthResponseDto> { Status = false, Message = "Refresh token mismatch. Please login again." };
                }

                if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return new Response<AuthResponseDto> { Status = false, Message = "Refresh token has expired. Please login again." };
                }

            var newAccessToken = GenerateJwtToken(user);
            var encryptedToken = _encryptionHelper.EncryptPersistent(newAccessToken); // Encrypting the token
            
            var newRefreshToken = GenerateRefreshToken();
            CreateHash(newRefreshToken, out byte[] newRefreshTokenHash, out byte[] newRefreshTokenSalt);

            user.RefreshTokenHash = newRefreshTokenHash;
            user.RefreshTokenSalt = newRefreshTokenSalt;
            // Configurable expiry, e.g., 7 days
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(Configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7")); 
            
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
                        RefreshToken = _encryptionHelper.EncryptTemporary(newRefreshToken),
                        User = userDto
                    }
                };
            }
        }
        catch (Exception ex)
        {
            return new Response<AuthResponseDto>
            {
                Status = false,
                Message = $"Token refresh failed: {ex.Message}",
                Data = null!
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

                // Generate Reset Token (Short alphanumeric to fit existing DB column of 10 chars)
                var token = GenerateSecureToken(32);
                
                user.ResetToken = token;
                user.ResetTokenExpiryTime = DateTime.UtcNow.AddHours(1); // Link valid for 1 hour

                await context.SaveChangesAsync();

                // Get Frontend URL from configuration
                var frontendUrl = Configuration["AppSettings:FrontendUrl"] ?? throw new InvalidOperationException("Frontend URL is not configured in environment variables.");
                var resetLink = $"{frontendUrl}/reset-password?token={token}&email={Uri.EscapeDataString(user.Email)}";

                // Send Reset Link via Email (HTML format)
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                        <h2 style='color: #007bff;'>Reset Your Password</h2>
                        <p>We received a request to reset your password. Click the button below to proceed:</p>
                        <div style='margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Reset Password</a>
                        </div>
                        <p>This link will expire in 1 hour.</p>
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='color: #666; font-size: 14px;'>{resetLink}</p>
                        <hr style='border: 0; border-top: 1px solid #eee; margin-top: 30px;'>
                        <p style='font-size: 12px; color: #999;'>If you did not request a password reset, please ignore this email.</p>
                    </div>";

                await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);

                return new Response<string>
                {
                    Status = true,
                    Message = "Reset link sent to your email",
                    Data = "Link Sent"
                };
            }
        }
        catch (Exception ex)
        {
             return new Response<string> { Status = false, Message = $"Error sending reset link: {ex.Message} {(ex.InnerException != null ? ex.InnerException.Message : "")}" };
        }
    }

    public async Task<Response<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            using (var context = CreateDbContext())
            {
                var normalizedEmail = resetPasswordDto.Email.Trim().ToLower();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

                if (user == null)
                {
                    return new Response<string> { Status = false, Message = "User not found" };
                }

                if (user.ResetToken != resetPasswordDto.Token)
                {
                    return new Response<string> { Status = false, Message = "Invalid reset token" };
                }

                if (user.ResetTokenExpiryTime < DateTime.UtcNow)
                {
                    return new Response<string> { Status = false, Message = "Reset token expired" };
                }

                // Create new password hash
                CreateHash(resetPasswordDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
                
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                
                // Clear reset token
                user.ResetToken = null;
                user.ResetTokenExpiryTime = null;

                await context.SaveChangesAsync();

                return new Response<string>
                {
                    Status = true,
                    Message = "Password reset successful",
                    Data = "Password Reset"
                };
            }
        }
        catch (Exception ex)
        {
            return new Response<string> { Status = false, Message = $"Error resetting password: {ex.Message}" };
        }
    }


    public async Task<Response<string>> LogoutAsync(ClaimsPrincipal userPrincipal)
    {
        try
        {
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return new Response<string> { Status = false, Message = "Invalid user identity" };
            }

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

    public async Task<Response<UserDto>> UpdateProfileAsync(ClaimsPrincipal userPrincipal, ProfileUpdateDto profileUpdateDto)
    {
        try
        {
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return new Response<UserDto> { Status = false, Message = "Invalid user identity", Data = null! };
            }

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

    public async Task<Response<AuthResponseDto>> GoogleLoginAsync(GoogleLoginDto googleLoginDto)
    {
        try
        {
            var clientId = Configuration["GoogleSettings:ClientId"];
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDto.Token, validationSettings);

            using (var context = CreateDbContext())
            {
                var email = payload.Email.Trim().ToLower();
                var user = await context.Users
                    .Include(u => u.UserProfile)
                    .FirstOrDefaultAsync(u => u.Email == email);

                string refreshToken = GenerateRefreshToken();
                CreateHash(refreshToken, out byte[] refreshTokenHash, out byte[] refreshTokenSalt);

                if (user == null)
                {
                    using (var transaction = await context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var randomPassword = Guid.NewGuid().ToString();
                            CreateHash(randomPassword, out byte[] passwordHash, out byte[] passwordSalt);

                            user = new User
                            {
                                Email = email,
                                PasswordHash = passwordHash,
                                PasswordSalt = passwordSalt,
                                IsActive = true,
                                RefreshTokenHash = refreshTokenHash,
                                RefreshTokenSalt = refreshTokenSalt,
                                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(Configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7")),
                                CreatedAt = DateTime.UtcNow
                            };

                            context.Users.Add(user);
                            await context.SaveChangesAsync();

                            var userProfile = new UserProfile
                            {
                                UserId = user.UserId,
                                FirstName = payload.GivenName ?? "Unknown",
                                LastName = payload.FamilyName ?? "User",
                                CreatedAt = DateTime.UtcNow
                            };

                            context.UserProfiles.Add(userProfile);
                            await context.SaveChangesAsync();

                            user.UserProfile = userProfile;

                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
                else
                {
                    if (!user.IsActive)
                    {
                        return new Response<AuthResponseDto>
                        {
                            Status = false,
                            Message = "Account is inactive. Please contact support.",
                            Data = null!
                        };
                    }

                    user.RefreshTokenHash = refreshTokenHash;
                    user.RefreshTokenSalt = refreshTokenSalt;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(Configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7"));
                    user.UpdatedAt = DateTime.UtcNow;

                    await context.SaveChangesAsync();
                }

                var token = GenerateJwtToken(user);
                var encryptedToken = _encryptionHelper.EncryptPersistent(token);

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
                    Message = "Login successful",
                    Data = new AuthResponseDto
                    {
                        Token = encryptedToken,
                        RefreshToken = _encryptionHelper.EncryptTemporary(refreshToken),
                        User = userDto
                    }
                };
            }
        }
        catch (InvalidJwtException)
        {
            return new Response<AuthResponseDto> { Status = false, Message = "Invalid Google Token" };
        }
        catch (Exception ex)
        {
            return new Response<AuthResponseDto>
            {
                Status = false,
                Message = $"Google login failed: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<Response<string>> GetGoogleClientIdAsync()
    {
        try
        {
            var clientId = Configuration["GoogleSettings:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                return new Response<string> { Status = false, Message = "Google Client ID not configured" };
            }

            // Use AesEncryptionHelper for cross-platform encryption (compatible with CryptoJS in Angular)
            var encryptedClientId = AesEncryptionHelper.Encrypt(clientId);
            return new Response<string>
            {
                Status = true,
                Message = "Google Client ID retrieved successfully",
                Data = encryptedClientId
            };
        }
        catch (Exception ex)
        {
            return new Response<string> { Status = false, Message = $"Error encrypting Client ID: {ex.Message}" };
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
        if (string.IsNullOrEmpty(token)) return null;

        var jwtSettings = Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false, // We allow expired tokens here
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
        // We use ValidateToken but wrap it in more careful logic
        SecurityToken securityToken;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken)
        {
            throw new SecurityTokenException("Token is not a valid JWT");
        }

        // Check algorithm - handle both "HS512" and "HmacSha512" variations
        var alg = jwtSecurityToken.Header.Alg;
        if (!alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase) && 
            !alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException($"Invalid security algorithm: {alg}");
        }

        return principal;
    }

    private string GenerateSecureToken(int length)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes).ToLower();
    }

    #endregion
}
