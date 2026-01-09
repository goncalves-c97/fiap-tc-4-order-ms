using Core.Constants;
using Core.Controllers;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Endpoints
{
    [ApiController]
    [Route("Pedido")]
    public class PedidoEndpoint(IDbConnection dbConnection, IEmailService emailService) : ControllerBase
    {
        private readonly IDbConnection _dbConnection = dbConnection;
        private readonly IEmailService _emailService = emailService;

        [Authorize]
        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] StatusPedidoEnum? status)
        {
            return Ok(await PedidoController.GetAllPedidos(_dbConnection, status));
        }

        [Authorize(Roles = UsuarioRoles.Administrador)]
        [HttpGet, Route("GetById")]
        public async Task<IActionResult> GetById([FromQuery] int idPedido)
        {
            return Ok(await PedidoController.GetPedidoById(_dbConnection, idPedido));
        }

        [Authorize(Roles = $"{UsuarioRoles.ClienteIdentificado}, {UsuarioRoles.ClienteAnonimo}")]
        [HttpPost, Route("IniciaPedido")]
        public async Task<IActionResult> IniciaPedido()
        {
            string? idClienteClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? emailClienteClaim = User.FindFirst(ClaimTypes.Email)?.Value;

            if (idClienteClaim == null)
                return Unauthorized("ID do cliente não encontrado.");

            if (!int.TryParse(idClienteClaim, out int idCliente))
                return Unauthorized("ID do cliente inválido!");

            Pedido pedido = await PedidoController.IniciaPedido(_dbConnection, idCliente, emailClienteClaim);

            return Ok(pedido.IdPedido);
        }

        [Authorize]
        [HttpPut, Route("UpdateStatusPedido")]
        public async Task<IActionResult> UpdateStatusPedido(int idPedido, StatusPedidoEnum? status)
        {
            string? idClienteClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (idClienteClaim == null)
                return Unauthorized("ID do cliente não encontrado.");

            if (!int.TryParse(idClienteClaim, out int idCliente))
                return Unauthorized("ID do cliente inválido!");

            if (status == null)
                return BadRequest("Status do pedido não informado.");

            await PedidoController.UpdateStatusPedido(_dbConnection, _emailService, idCliente, idPedido, (StatusPedidoEnum)status);

            return Ok();
        }

        [Authorize(Roles = $"{UsuarioRoles.ClienteIdentificado}, {UsuarioRoles.ClienteAnonimo}")]
        [HttpPut, Route("SetIdPagamento")]
        public async Task<IActionResult> SetIdPagamento(int idPedido, int idPagamento)
        {
            string? idClienteClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (idClienteClaim == null)
                return Unauthorized("ID do cliente não encontrado.");

            if (!int.TryParse(idClienteClaim, out int idCliente))
                return Unauthorized("ID do cliente inválido!");

            await PedidoController.SetIdPagamento(_dbConnection, idCliente, idPedido, idPagamento);

            return Ok();
        }
    }
}
