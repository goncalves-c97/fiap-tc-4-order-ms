using Core.Dtos;
using Core.Interfaces;
using Infra.Payment.Exceptions;
using Infra.Payment.MercadoPago.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infra.Payment.MercadoPago
{
    public class PagamentoService(PaymentSettingsDto paymentSettingsDto) : IPagamentoService
    {
        private async Task<QrCodeResponse> GetQrCodePagamentoMercadoPago(PagamentoRequest pagamentoRequest)
        {
            pagamentoRequest.Sponsor = new()
            {
                Id = paymentSettingsDto.SponsorId
            };

            pagamentoRequest.CashOut = new()
            {
                Amount = 0
            };

            string urlMercadoPago = paymentSettingsDto.BaseUrl;
            string controllerGeracaoQrCode = paymentSettingsDto.PaymentController;
            string userId = paymentSettingsDto.UserId;
            string externalPosId = paymentSettingsDto.ExternalPosId;
            string token = paymentSettingsDto.Token;
            string pagamentoRequestJson = JsonConvert.SerializeObject(pagamentoRequest);
            string fullUrl = $"{urlMercadoPago}/{controllerGeracaoQrCode}/{userId}/pos/{externalPosId}/qrs";

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Put, fullUrl);
            request.Headers.Add("Authorization", $"Bearer {token}");
            var content = new StringContent(pagamentoRequestJson, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(responseContent);
            
            // Deserialize the response JSON into your object

            QrCodeResponse qrCodeResponse = JsonConvert.DeserializeObject<QrCodeResponse>(responseContent)!;

            return qrCodeResponse;
        }

        public async Task<QrCodePagamentoDto> GetQrCodePagamento(PedidoPagamentoDto pedidoDto)
        {
            try
            {
                PagamentoRequest pagamentoRequest = new()
                {
                    ExternalReference = pedidoDto.PedidoId,
                    Title = pedidoDto.TituloPedido,
                    Description = pedidoDto.Description,
                    NotificationUrl = pedidoDto.UrlNotificacao,
                    ExpirationDate = pedidoDto.DataHoraExpiracaoPagamento.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                    TotalAmount = pedidoDto.ValorTotal,
                    Items = []
                };

                foreach (ItemPedidoPagamentoDto item in pedidoDto.ItensPedido)
                {
                    pagamentoRequest.Items.Add(new Item
                    {
                        SkuNumber = item.Id,
                        Category = item.Categoria,
                        Title = item.Descricao,
                        UnitPrice = item.PrecoUnitario,
                        Quantity = item.Quantidade,
                        TotalAmount = item.ValorTotal,
                        UnitMeasure = "unit"
                    });
                }

                QrCodeResponse qrCodeResponse = await GetQrCodePagamentoMercadoPago(pagamentoRequest);
                QrCodePagamentoDto qrCodePagamentoDto = new(pedidoDto.PedidoId, qrCodeResponse.InStoreOrderId, qrCodeResponse.QrData);
                return qrCodePagamentoDto;
            }
            catch (Exception ex)
            {
                throw new QrCodeGenerationFailedException("Houve algum problema na geração do QRCode para pagamento.", ex);
            }
        }

        public async Task<bool?> CheckPagamentoAprovado(string jsonNotificacao)
        {
            MercadoPagoWebhook? mercadoPagoWebhook = JsonConvert.DeserializeObject<MercadoPagoWebhook>(jsonNotificacao);

            if (mercadoPagoWebhook == null)
                throw new ArgumentNullException(nameof(jsonNotificacao), "Notificação inválida");

            string fullUrl = mercadoPagoWebhook.Resource;

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            var token = paymentSettingsDto.Token;
            request.Headers.Add("Authorization", $"Bearer {token}");
            var response = await client.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(responseContent);

            MercadoPagoMerchantOrder merchantOrder = JsonConvert.DeserializeObject<MercadoPagoMerchantOrder>(responseContent)!;

            if (merchantOrder.Payments.Count == 0)
                return null;
            else
                return merchantOrder.Payments.Last().Status == "approved";
        }
    }
}
