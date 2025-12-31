using Newtonsoft.Json;

namespace Infra.Payment.MercadoPago.Entities
{
    public class QrCodeResponse
    {
        [JsonProperty("in_store_order_id")]
        public string InStoreOrderId { get; set; }

        [JsonProperty("qr_data")]
        public string QrData { get; set; }
    }
}
