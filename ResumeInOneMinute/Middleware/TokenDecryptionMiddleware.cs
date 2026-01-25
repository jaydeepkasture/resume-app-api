using Microsoft.AspNetCore.Http;
using ResumeInOneMinute.Infrastructure.CommonServices;
using System.Threading.Tasks;

namespace ResumeInOneMinute.Middleware;

/// <summary>
/// Middleware to decrypt the Access Token from the Authorization header before it reaches the Authentication Middleware.
/// This allows the client to send an encrypted token (for security) while the server processes a standard JWT.
/// </summary>
public class TokenDecryptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EncryptionHelper _encryptionHelper;

    public TokenDecryptionMiddleware(RequestDelegate next, EncryptionHelper encryptionHelper)
    {
        _next = next;
        _encryptionHelper = encryptionHelper;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var encryptedToken = authHeader.Replace("Bearer ", "").Trim();

            try
            {
                // Attempt to decrypt the token
                // We use Persistent encryption here because that's what we used in the Repository
                var decryptedToken = _encryptionHelper.DecryptPersistent(encryptedToken);

                if (!string.IsNullOrEmpty(decryptedToken))
                {
                    // Replace the header with the decrypted token
                    context.Request.Headers["Authorization"] = $"Bearer {decryptedToken}";
                }
            }
            catch
            {
                // If decryption fails, we leave the header as is. 
                // The Authentication middleware will likely fail to validate it, which is the correct behavior.
            }
        }

        await _next(context);
    }
}
