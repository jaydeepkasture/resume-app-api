using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Controllers.Super;

namespace ResumeInOneMinute.Controllers.Contact;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contact")]
[ApiController]
[Produces("application/json")]
public class ContactController : SuperController
{
    private readonly IContactRepository _contactRepository;

    public ContactController(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    /// <summary>
    /// Submit a contact us message
    /// </summary>
    /// <param name="contactMessageDto">Contact message details (Name, Email, Message)</param>
    /// <returns>Status of the submission</returns>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitContactMessage([FromBody] ContactMessageDto contactMessageDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new 
            { 
                Status = false, 
                Message = "Validation failed", 
                Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
            });
        }

        var result = await _contactRepository.SaveContactMessageAsync(contactMessageDto);
        
        if (!result.Status)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
