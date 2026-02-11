using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using System.Security.Claims;

namespace ResumeInOneMinute.Controllers.Billing;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/billing")]
[ApiController]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public BillingController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _subscriptionService.GetAvailablePlansAsync();
        return Ok(new { Status = true, Data = plans });
    }

    [HttpGet("benefits/me")]
    public async Task<IActionResult> GetMyBenefits()
    {
        var userId = GetUserId();
        var benefits = await _subscriptionService.GetUserBenefitsAsync(userId);
        return Ok(new { Status = true, Data = benefits });
    }

    [HttpGet("subscription/me")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = GetUserId();
        var sub = await _subscriptionService.GetUserSubscriptionAsync(userId);
        return Ok(new { Status = true, Data = sub });
    }

    [HttpPost("razorpay/order")]
    [HttpPost("subscribe")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var order = await _subscriptionService.CreatePaymentOrderAsync(userId, request);
            return Ok(new { Status = true, Data = order });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Status = false, Message = ex.Message });
        }
    }

    [HttpPost("subscription/confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequestDto request)
    {
        var userId = GetUserId();
        var result = await _subscriptionService.ConfirmPaymentAsync(userId, request);
        
        if (result != null)
        {
            return Ok(new { Status = true, Message = "Payment confirmed and subscription activated", Data = result });
        }
        
        return BadRequest(new { Status = false, Message = "Payment verification failed" });
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
