using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ResumeInOneMinute.Controllers.Super;

public abstract class SuperController : ControllerBase
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
             return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }
    }
}
