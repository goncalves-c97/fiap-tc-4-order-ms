namespace Core.Entities;

public partial class Pedido
{
    public int IdPedido { get; set; }

    public int IdCliente { get; set; }

    public DateTime DataHoraInicio { get; set; }

    public DateTime? DataHoraConfirmacao { get; set; }

    public DateTime? DataHoraInicioPreparo { get; set; }

    public DateTime? DataHoraTerminoPreparo { get; set; }

    public DateTime? DataHoraRetiradaCliente { get; set; }

    public int? IdPagamento { get; set; }

    public int? IdStatusPedido { get; set; }

    public virtual StatusPedido? IdStatusPedidoNavigation { get; set; } = null!;

    public Pedido() { }

    public Pedido(int idCliente)
    {
        IdCliente = idCliente;
        DataHoraInicio = DateTime.Now;
        DataHoraConfirmacao = null;
        DataHoraInicioPreparo = null;
        DataHoraTerminoPreparo = null;
        DataHoraRetiradaCliente = null;
        IdPagamento = null;
        IdStatusPedido = null;
    }
}
