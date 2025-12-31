using Core.Interfaces.Gateways;

namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        public IPedidoGateway PedidoRepository { get; }
    }
}
