namespace Core.Entities;

public partial class StatusPedido
{
    public int IdStatusPedido { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
