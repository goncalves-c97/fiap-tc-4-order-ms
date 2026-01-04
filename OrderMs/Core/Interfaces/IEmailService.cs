using Core.Dtos;

namespace Core.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailAsync(EmailRequestDto emailRequesDto);
    }
}
