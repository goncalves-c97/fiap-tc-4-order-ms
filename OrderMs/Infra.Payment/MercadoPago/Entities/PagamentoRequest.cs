
using Newtonsoft.Json;

namespace Infra.Payment.MercadoPago.Entities
{
    public class PagamentoRequest
    {
        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty("expiration_date")]
        public string ExpirationDate { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("sponsor")]
        public Sponsor Sponsor { get; set; }

        [JsonProperty("cash_out")]
        public CashOut CashOut { get; set; }
    }
}
