using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface IContactRepository
{
    Task<Response<bool>> SaveContactMessageAsync(ContactMessageDto contactMessageDto);
}
