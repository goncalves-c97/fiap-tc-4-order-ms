using Newtonsoft.Json;

namespace Infra.Payment.MercadoPago.Entities
{
    public class CashOut
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}
