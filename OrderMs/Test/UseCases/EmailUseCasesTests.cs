using Core.Dtos;
using Core.Entities;
using Core.UseCases;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Test.UseCases;

public class EmailUseCasesTests
{
    [Fact]
    public async Task SendEmail_WhenServiceIsNull_ThrowsArgumentNullException()
    {
        var dto = new EmailRequestDto { ToEmail = "a@b.com", Subject = "s", Body = "b" };
        await Assert.ThrowsAsync<ArgumentNullException>(() => EmailUseCases.SendEmail(null!, dto));
    }

    [Fact]
    public async Task SendEmail_WhenDtoIsNull_ThrowsArgumentNullException()
    {
        var svc = new Mock<Core.Interfaces.IEmailService>(MockBehavior.Strict);
        await Assert.ThrowsAsync<ArgumentNullException>(() => EmailUseCases.SendEmail(svc.Object, null!));
    }

    [Fact]
    public async Task SendEmail_CallsServiceSendEmailAsync()
    {
        var svc = new Mock<Core.Interfaces.IEmailService>(MockBehavior.Strict);
        var dto = new EmailRequestDto { ToEmail = "a@b.com", Subject = "s", Body = "b" };
        svc.Setup(s => s.SendEmailAsync(dto)).Returns(Task.CompletedTask);

        await EmailUseCases.SendEmail(svc.Object, dto);

        svc.Verify(s => s.SendEmailAsync(dto), Times.Once);
    }

    [Fact]
    public async Task SendEmailPing_BuildsDtoAndCallsService()
    {
        var svc = new Mock<Core.Interfaces.IEmailService>(MockBehavior.Strict);
        svc.Setup(s => s.SendEmailAsync(It.IsAny<EmailRequestDto>())).Returns(Task.CompletedTask);

        await EmailUseCases.SendEmailPing(svc.Object, "ping@x.com");

        svc.Verify(s => s.SendEmailAsync(It.Is<EmailRequestDto>(d =>
        d.ToEmail == "ping@x.com" &&
        d.Subject.StartsWith("Ping FastFoodChallengeWebApi ") &&
        !string.IsNullOrWhiteSpace(d.Body))), Times.Once);
    }

    [Fact]
    public void GetClaimValue_WhenJwtIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => EmailUseCases.GetClaimValue(null!, ClaimTypes.Email));
    }

    [Fact]
    public void GetClaimValue_WhenTokenUnreadable_ReturnsNull()
    {
        var result = EmailUseCases.GetClaimValue("not-a-jwt", ClaimTypes.Email);
        Assert.Null(result);
    }

    [Fact]
    public void GetClaimValue_WhenClaimExists_ReturnsValue()
    {
        var token = new JwtSecurityToken(
        claims: new[] { new Claim(ClaimTypes.Email, "a@b.com") }
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        var result = EmailUseCases.GetClaimValue(jwt, ClaimTypes.Email);

        Assert.Equal("a@b.com", result);
    }

    [Fact]
    public async Task SendNotificacaoPedidoPronto_WhenEmailIsNull_DoesNotSend()
    {
        var svc = new Mock<Core.Interfaces.IEmailService>(MockBehavior.Strict);
        var pedido = new Pedido(idCliente: 1, emailCliente: null) { IdPedido = 1 };

        await EmailUseCases.SendNotificacaoPedidoPronto(svc.Object, pedido);

        svc.Verify(s => s.SendEmailAsync(It.IsAny<EmailRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task SendNotificacaoPedidoPronto_WhenServiceThrows_IsSwallowed()
    {
        var svc = new Mock<Core.Interfaces.IEmailService>(MockBehavior.Strict);
        var pedido = new Pedido(idCliente: 1, emailCliente: "a@b.com") { IdPedido = 99 };
        svc.Setup(s => s.SendEmailAsync(It.IsAny<EmailRequestDto>())).ThrowsAsync(new InvalidOperationException("boom"));

        await EmailUseCases.SendNotificacaoPedidoPronto(svc.Object, pedido);

        svc.Verify(s => s.SendEmailAsync(It.IsAny<EmailRequestDto>()), Times.Once);
    }
}
