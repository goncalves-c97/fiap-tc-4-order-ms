using Core.Entities;

namespace Test.Entities;

public class PedidoTests
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        var p = new Pedido(idCliente: 10, emailCliente: "a@b.com");

        Assert.Equal(10, p.IdCliente);
        Assert.Equal("a@b.com", p.Email);
        Assert.NotEqual(default, p.DataHoraInicio);
        Assert.Null(p.DataHoraConfirmacao);
        Assert.Null(p.DataHoraInicioPreparo);
        Assert.Null(p.DataHoraTerminoPreparo);
        Assert.Null(p.DataHoraRetiradaCliente);
        Assert.Null(p.IdPagamento);
        Assert.Null(p.IdStatusPedido);
    }
}
