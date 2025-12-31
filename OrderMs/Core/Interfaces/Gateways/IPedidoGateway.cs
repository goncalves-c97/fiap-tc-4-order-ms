using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Gateways
{
    public interface IPedidoGateway
    {
        public Task<IEnumerable<Pedido>> GetAllPedidos();
        public Task<IEnumerable<Pedido>> GetAllPedidosByStatusPedido(StatusPedidoEnum statusPedidoEnum);
        public Task<Pedido> InsertPedido(Pedido pedido);
        public Task UpdatePedido(Pedido pedido);
        public Task<Pedido?> GetById(int idPedido);
    }
}
