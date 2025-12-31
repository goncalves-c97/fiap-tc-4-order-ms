using Core.Entities;
using Core.Enums;
using Core.Gateways;
using Core.Interfaces;
using Core.UseCases;

namespace Core.Controllers
{
    public static class PedidoController
    {
        public static async Task<IEnumerable<Pedido>> GetAllPedidos(IDbConnection dbConnection, StatusPedidoEnum? status = null)
        {
            PedidoGateway gateway = new(dbConnection);
            IEnumerable<Pedido> pedidos = await PedidoUseCases.GetAllPedidos(gateway, status);
            return pedidos;
        }

        public static async Task<Pedido> GetPedidoById(IDbConnection dbConnection, int idPedido)
        {
            PedidoGateway gateway = new(dbConnection);
            Pedido pedido = await PedidoUseCases.GetPedidoById(gateway, idPedido);
            return pedido;
        }

        public static async Task<Pedido> IniciaPedido(IDbConnection dbConnection, int idCliente)
        {
            PedidoGateway gateway = new(dbConnection);
            Pedido pedido = await PedidoUseCases.CreatePedido(gateway, idCliente);
            return pedido;
        }

        public static async Task SetIdPagamento(IDbConnection dbConnection, int idCliente, int idPedido, int idPagamento)
        {
            PedidoGateway gateway = new(dbConnection);
            await PedidoUseCases.UpdateIdPagamento(gateway, idCliente, idPedido, idPagamento);
        }

        public static async Task UpdateStatusPedido(IDbConnection dbConnection, int idCliente, int idProduto, StatusPedidoEnum statusPedido)
        {
            PedidoGateway gateway = new(dbConnection);
            await PedidoUseCases.UpdateStatusPedido(gateway, idCliente, idProduto, statusPedido);
        }
    }
}
