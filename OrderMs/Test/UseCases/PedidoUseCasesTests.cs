using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Gateways;
using Core.UseCases;
using Moq;

namespace Test.UseCases;

public class PedidoUseCasesTests
{
    [Fact]
    public async Task GetAllPedidos_WhenGatewayIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        PedidoUseCases.GetAllPedidos(null!));
    }

    [Fact]
    public async Task GetAllPedidos_WhenStatusIsNull_CallsGetAllPedidos()
    {
        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetAllPedidos()).ReturnsAsync(Array.Empty<Pedido>());

        var result = await PedidoUseCases.GetAllPedidos(gateway.Object, status: null);

        Assert.NotNull(result);
        gateway.Verify(g => g.GetAllPedidos(), Times.Once);
        gateway.Verify(g => g.GetAllPedidosByStatusPedido(It.IsAny<StatusPedidoEnum>()), Times.Never);
    }

    [Fact]
    public async Task GetAllPedidos_WhenStatusHasValue_CallsGetAllPedidosByStatusPedido()
    {
        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetAllPedidosByStatusPedido(StatusPedidoEnum.Pronto))
        .ReturnsAsync(Array.Empty<Pedido>());

        var result = await PedidoUseCases.GetAllPedidos(gateway.Object, StatusPedidoEnum.Pronto);

        Assert.NotNull(result);
        gateway.Verify(g => g.GetAllPedidosByStatusPedido(StatusPedidoEnum.Pronto), Times.Once);
        gateway.Verify(g => g.GetAllPedidos(), Times.Never);
    }

    [Fact]
    public async Task GetPedidoById_WhenNotFound_ThrowsKeyNotFoundException()
    {
        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetById(10)).ReturnsAsync((Pedido?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
        PedidoUseCases.GetPedidoById(gateway.Object, 10));

        gateway.Verify(g => g.GetById(10), Times.Once);
    }

    [Fact]
    public async Task GetPedidoById_WhenFound_ReturnsPedido()
    {
        var pedido = new Pedido(idCliente: 1, emailCliente: "a@b.com") { IdPedido = 10 };

        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetById(10)).ReturnsAsync(pedido);

        var result = await PedidoUseCases.GetPedidoById(gateway.Object, 10);

        Assert.Same(pedido, result);
        gateway.Verify(g => g.GetById(10), Times.Once);
    }

    [Fact]
    public async Task CreatePedido_WhenGatewayIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        PedidoUseCases.CreatePedido(null!, 1, "a@b.com"));
    }

    [Fact]
    public async Task CreatePedido_CallsInsertPedido_WithPedidoContainingClientAndEmail()
    {
        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);

        gateway.Setup(g => g.InsertPedido(It.IsAny<Pedido>()))
        .ReturnsAsync((Pedido p) => p);

        var created = await PedidoUseCases.CreatePedido(gateway.Object, idCliente: 123, emailCliente: "x@y.com");

        Assert.Equal(123, created.IdCliente);
        Assert.Equal("x@y.com", created.Email);
        Assert.True((DateTime.Now - created.DataHoraInicio).TotalSeconds < 5);

        gateway.Verify(g => g.InsertPedido(It.Is<Pedido>(p => p.IdCliente == 123 && p.Email == "x@y.com")), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusPedido_WhenPedidoNotFound_ThrowsKeyNotFoundException()
    {
        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        gateway.Setup(g => g.GetById(10)).ReturnsAsync((Pedido?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
        PedidoUseCases.UpdateStatusPedido(gateway.Object, emailService.Object, idCliente: 1, idPedido: 10, statusPedido: StatusPedidoEnum.Pronto));

        gateway.Verify(g => g.GetById(10), Times.Once);
        gateway.Verify(g => g.UpdatePedido(It.IsAny<Pedido>()), Times.Never);
    }

    [Theory]
    [InlineData(StatusPedidoEnum.PagamentoPendente)]
    [InlineData(StatusPedidoEnum.EmPreparacao)]
    [InlineData(StatusPedidoEnum.Finalizado)]
    public async Task UpdateStatusPedido_UpdatesPedidoAndSetsExpectedDateField(StatusPedidoEnum status)
    {
        var pedido = new Pedido(idCliente: 1, emailCliente: "a@b.com") { IdPedido = 10 };

        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        gateway.Setup(g => g.GetById(10)).ReturnsAsync(pedido);
        gateway.Setup(g => g.UpdatePedido(It.IsAny<Pedido>())).Returns(Task.CompletedTask);

        await PedidoUseCases.UpdateStatusPedido(gateway.Object, emailService.Object, 1, 10, status);

        gateway.Verify(g => g.UpdatePedido(It.Is<Pedido>(p => p.IdStatusPedido == (int)status)), Times.Once);

        switch (status)
        {
            case StatusPedidoEnum.PagamentoPendente:
                Assert.NotNull(pedido.DataHoraConfirmacao);
                break;
            case StatusPedidoEnum.EmPreparacao:
                Assert.NotNull(pedido.DataHoraInicioPreparo);
                break;
            case StatusPedidoEnum.Finalizado:
                Assert.NotNull(pedido.DataHoraRetiradaCliente);
                break;
        }

        emailService.Verify(e => e.SendEmailAsync(It.IsAny<Core.Dtos.EmailRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusPedido_WhenPronto_SendsNotificationAndSetsTerminoPreparo()
    {
        var pedido = new Pedido(idCliente: 1, emailCliente: "a@b.com") { IdPedido = 10 };

        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        gateway.Setup(g => g.GetById(10)).ReturnsAsync(pedido);
        gateway.Setup(g => g.UpdatePedido(It.IsAny<Pedido>())).Returns(Task.CompletedTask);
        emailService.Setup(e => e.SendEmailAsync(It.IsAny<Core.Dtos.EmailRequestDto>())).Returns(Task.CompletedTask);

        await PedidoUseCases.UpdateStatusPedido(gateway.Object, emailService.Object, 1, 10, StatusPedidoEnum.Pronto);

        Assert.NotNull(pedido.DataHoraTerminoPreparo);

        emailService.Verify(e => e.SendEmailAsync(It.Is<Core.Dtos.EmailRequestDto>(dto =>
        dto.ToEmail == "a@b.com" &&
        dto.Subject.Contains("#10"))), Times.Once);

        gateway.Verify(g => g.UpdatePedido(It.Is<Pedido>(p => p.IdStatusPedido == (int)StatusPedidoEnum.Pronto)), Times.Once);
    }

    [Fact]
    public async Task UpdateIdPagamento_WhenPedidoBelongsToDifferentClient_ThrowsUnauthorizedAccessException()
    {
        var pedido = new Pedido(idCliente: 1, emailCliente: null) { IdPedido = 10 };

        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetById(10)).ReturnsAsync(pedido);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
        PedidoUseCases.UpdateIdPagamento(gateway.Object, idCliente: 999, idPedido: 10, idPagamento: 55));

        gateway.Verify(g => g.UpdatePedido(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task UpdateIdPagamento_WhenAuthorized_UpdatesFieldsAndCallsUpdatePedido()
    {
        var pedido = new Pedido(idCliente: 1, emailCliente: null) { IdPedido = 10 };

        var gateway = new Mock<IPedidoGateway>(MockBehavior.Strict);
        gateway.Setup(g => g.GetById(10)).ReturnsAsync(pedido);
        gateway.Setup(g => g.UpdatePedido(It.IsAny<Pedido>())).Returns(Task.CompletedTask);

        await PedidoUseCases.UpdateIdPagamento(gateway.Object, idCliente: 1, idPedido: 10, idPagamento: 55);

        Assert.Equal(55, pedido.IdPagamento);
        Assert.Equal((int)StatusPedidoEnum.PagamentoPendente, pedido.IdStatusPedido);
        Assert.NotNull(pedido.DataHoraConfirmacao);

        gateway.Verify(g => g.UpdatePedido(It.Is<Pedido>(p => p.IdPagamento == 55)), Times.Once);
    }
}
