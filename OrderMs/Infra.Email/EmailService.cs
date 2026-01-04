using Core.Dtos;
using Core.Interfaces;
using Infra.Email.Exceptions;
using System.Net;
using System.Net.Mail;

namespace Infra.Email
{
    public class EmailService(EmailSettingsDto emailSettings) : IEmailService
    {
        public async Task SendEmailAsync(EmailRequestDto emailRequestDto)
        {
            MailMessage messageObj;
            SmtpClient smtp;

            try
            {
                messageObj = new MailMessage()
                {
                    From = new MailAddress(emailSettings.Mail, emailSettings.DisplayName),
                    Subject = emailRequestDto.Subject,
                    IsBodyHtml = true,
                    Body = emailRequestDto.Body
                };

                messageObj.To.Add(emailRequestDto.ToEmail);

                smtp = new SmtpClient
                {
                    Host = emailSettings.Host,
                    Port = emailSettings.Port,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(emailSettings.Mail, emailSettings.Password)
                };
            }
            catch (Exception ex)
            {
                throw new EmailBuildException("Falha na montagem do e-mail", ex);
            }

            try
            {
                await smtp.SendMailAsync(messageObj);
            }
            catch (Exception ex)
            {
                throw new EmailFailureException("Falha no envio do e-mail", ex);
            }
        }
    }
}
