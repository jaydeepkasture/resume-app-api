using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ResumeInOneMinute.Controllers;

[ApiController]
[Produces("application/json")]
public class SuperController : ControllerBase
{
    protected long CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return 0;
            }
            if (long.TryParse(userIdClaim, out var id))
            {
                return id;
            }
            return 0;
        }
    }

    protected string CurrentUserEmail
    {
        get
        {
             return User.FindFirst(ClaimTypes.Email)?.Value?.ToString() ?? string.Empty;
        }
    }
}
