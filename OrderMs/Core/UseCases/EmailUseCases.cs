using Core.Dtos;
using Core.Entities;
using Core.Gateways;
using Core.Interfaces;
using Core.Interfaces.Gateways;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.UseCases
{
    public static class EmailUseCases
    {
        public static async Task SendEmail(IEmailService emailService, EmailRequestDto emailRequestDto)
        {
            if (emailService == null)
                throw new ArgumentNullException(nameof(emailService), "Email service cannot be null.");

            if (emailRequestDto == null)
                throw new ArgumentNullException(nameof(emailRequestDto), "Email request DTO cannot be null.");

            await emailService.SendEmailAsync(emailRequestDto);
        }

        public static async Task SendEmailPing(IEmailService emailService, string toEmail)
        {
            EmailRequestDto emailRequestDto = new()
            {
                ToEmail = toEmail,
                Subject = $"Ping FastFoodChallengeWebApi {DateTime.UtcNow.Ticks}",
                Body = DateTime.UtcNow.Ticks.ToString()
            };

            await SendEmail(emailService, emailRequestDto);
        }

        public static string? GetClaimValue(string jwt, string claimType)
        {
            if (string.IsNullOrWhiteSpace(jwt)) throw new ArgumentNullException(nameof(jwt));
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(jwt)) return null;
            var token = handler.ReadJwtToken(jwt);
            return token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }


        public static async Task SendNotificacaoPedidoPronto(IEmailService emailService, Pedido pedido)
        {

            // Em situações de cliente anônimo, não há notificação por email
            if (string.IsNullOrEmpty(pedido.Email))
                return;

            string emailMessage = $@"
                <!DOCTYPE html>
                <html lang=""pt-br"">
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f9f9f9;
                            padding: 20px;
                            color: #333;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            text-align: center;
                            color: #4CAF50;
                        }}
                        .content {{
                            margin-top: 20px;
                            font-size: 16px;
                            line-height: 1.6;
                        }}
                        .footer {{
                            margin-top: 30px;
                            font-size: 13px;
                            color: #888;
                            text-align: center;
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <h2 class=""header"">Pedido #{pedido.IdPedido} pronto para retirada!</h2>
                        <div class=""content"">
                            <p>Olá,</p>
                            <p>Seu pedido <strong>#{pedido.IdPedido}</strong> está prontinho!</p>
                            <p>É só ir até o balcão e informar o número do pedido a um de nossos atendentes.</p>
                            <p>Obrigado pela sua preferência. Esperamos vê-lo novamente em breve! 😊</p>
                        </div>
                        <div class=""footer"">
                            © {DateTime.Now.Year} - Nossa Loja. Todos os direitos reservados.
                        </div>
                    </div>
                </body>
                </html>";

            EmailRequestDto emailRequestDto = new()
            {
                ToEmail = pedido.Email,
                Subject = $"Seu pedido (#{pedido.IdPedido}) está pronto!",
                Body = emailMessage
            };

            try
            {
                await SendEmail(emailService, emailRequestDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Falha ao enviar e-mail");
            }
        }
    }
}
