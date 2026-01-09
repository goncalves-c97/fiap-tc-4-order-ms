using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Gateways;

namespace Core.Gateways
{
    public class PedidoGateway(IDbConnection dbConnection) : IPedidoGateway
    {
        private readonly IDbConnection _dbConnection = dbConnection;
        private readonly string _tableName = nameof(Pedido);

        public async Task<Pedido> InsertPedido(Pedido pedido)
        {
            int registeredId = await _dbConnection.InsertAndReturnIdAsync(_tableName, new Dictionary<string, object>
            {
                { "id_cliente", pedido.IdCliente },
                { "email", pedido.Email },
                { "data_hora_inicio", pedido.DataHoraInicio },
                { "data_hora_confirmacao", pedido.DataHoraConfirmacao },
                { "data_hora_inicio_preparo", pedido.DataHoraInicioPreparo },
                { "data_hora_termino_preparo", pedido.DataHoraTerminoPreparo },
                { "data_hora_retirada_cliente", pedido.DataHoraRetiradaCliente },
                { "id_pagamento", pedido.IdPagamento },
                { "id_status_pedido", pedido.IdStatusPedido },
            }, "id_pedido");

            return await GetById(registeredId) ?? throw new Exception("Insert failed");
        }

        public async Task<IEnumerable<Pedido>> GetAllPedidos()
        {
            return await _dbConnection.ListAllAsync<Pedido>(_tableName);
        }

        public async Task<IEnumerable<Pedido>> GetAllPedidosByStatusPedido(StatusPedidoEnum statusPedidoEnum)
        {
            return await _dbConnection.SearchByParametersAsync<Pedido>(_tableName, "id_status_pedido = @Status", new { Status = (int)statusPedidoEnum });
        }

        public async Task<Pedido?> GetById(int idPedido)
        {
            Pedido? pedido = await _dbConnection.SearchFirstOrDefaultByParametersAsync<Pedido>(
                _tableName,
                "id_pedido = @Id",
                new { Id = idPedido }
            );

            if (pedido == null)
                return null;

            return pedido;
        }

        public async Task UpdatePedido(Pedido pedido)
        {
            await _dbConnection.UpdateAsync(_tableName, new Dictionary<string, object>
            {
                { "id_cliente", pedido.IdCliente },
                { "email", pedido.Email },
                { "data_hora_inicio", pedido.DataHoraInicio },
                { "data_hora_confirmacao", pedido.DataHoraConfirmacao },
                { "data_hora_inicio_preparo", pedido.DataHoraInicioPreparo },
                { "data_hora_termino_preparo", pedido.DataHoraTerminoPreparo },
                { "data_hora_retirada_cliente", pedido.DataHoraRetiradaCliente },
                { "id_pagamento", pedido.IdPagamento },
                { "id_status_pedido", pedido.IdStatusPedido }
            }
            , "id_pedido = @Id"
            , new { Id = pedido.IdPedido });
        }
    }
}