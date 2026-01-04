using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Gateways;
using Newtonsoft.Json.Linq;

namespace Core.UseCases
{
    public static class PedidoUseCases
    {
        public static async Task<IEnumerable<Pedido>> GetAllPedidos(IPedidoGateway pedidoGateway, StatusPedidoEnum? status = null)
        {
            if (pedidoGateway == null)
                throw new ArgumentNullException(nameof(pedidoGateway), "Pedido gateway cannot be null.");

            if (status.HasValue)
                return await pedidoGateway.GetAllPedidosByStatusPedido(status.Value);
            else
                return await pedidoGateway.GetAllPedidos();
        }
        public static async Task<Pedido> GetPedidoById(IPedidoGateway pedidoGateway, int idPedido)
        {
            if (pedidoGateway == null)
                throw new ArgumentNullException(nameof(pedidoGateway), "Pedido gateway cannot be null.");

            return await pedidoGateway.GetById(idPedido) ?? throw new KeyNotFoundException($"Pedido with ID {idPedido} not found.");
        }
        public static async Task<Pedido> CreatePedido(IPedidoGateway pedidoGateway, int idCliente, string? emailCliente)
        {
            if (pedidoGateway == null)
                throw new ArgumentNullException(nameof(pedidoGateway), "Pedido gateway cannot be null.");

            Pedido pedido = new(idCliente, emailCliente);

            return await pedidoGateway.InsertPedido(pedido);
        }

        private static async Task UpdatePedido(IPedidoGateway pedidoGateway, Pedido pedido)
        {
            if (pedidoGateway == null)
                throw new ArgumentNullException(nameof(pedidoGateway), "Pedido gateway cannot be null.");
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido), "Pedido cannot be null.");

            await pedidoGateway.UpdatePedido(pedido);
        }

        public static async Task UpdateStatusPedido(IPedidoGateway pedidoGateway, IEmailService emailService, int idCliente, int idPedido, StatusPedidoEnum statusPedido)
        {
            if (pedidoGateway == null)
                throw new ArgumentNullException(nameof(pedidoGateway), "Pedido gateway cannot be null.");

            Pedido pedido = await pedidoGateway.GetById(idPedido) ?? throw new KeyNotFoundException($"Pedido with ID {idPedido} not found.");

            // Atribui o novo status ao pedido
            pedido.IdStatusPedido = (int)statusPedido;

            // Data e hora atual
            DateTime now = DateTime.Now;

            // Atualiza os campos de data e hora conforme o status do pedido
            switch (statusPedido)
            {
                case StatusPedidoEnum.PagamentoPendente:
                    pedido.DataHoraConfirmacao = now;
                    break;
                case StatusPedidoEnum.EmPreparacao:
                    pedido.DataHoraInicioPreparo = now;
                    break;
                case StatusPedidoEnum.Pronto:
                    pedido.DataHoraTerminoPreparo = now;
                    await EmailUseCases.SendNotificacaoPedidoPronto(emailService, pedido);
                    break;
                case StatusPedidoEnum.Finalizado:
                    pedido.DataHoraRetiradaCliente = now;
                    break;
            }

            // Atualiza o pedido no gateway
            await UpdatePedido(pedidoGateway, pedido);
        }

        public static async Task UpdateIdPagamento(IPedidoGateway pedidoGateway, int idCliente, int idPedido, int idPagamento)
        {
            Pedido? pedido = await GetPedidoById(pedidoGateway, idPedido);

            if (pedido.IdCliente != idCliente)
                throw new UnauthorizedAccessException("O cliente não tem permissão para atualizar o ID de pagamento deste pedido.");

            pedido.IdPagamento = idPagamento;

            pedido.DataHoraConfirmacao = DateTime.Now;

            pedido.IdStatusPedido = (int)StatusPedidoEnum.PagamentoPendente;

            await UpdatePedido(pedidoGateway, pedido);
        }
    }
}
