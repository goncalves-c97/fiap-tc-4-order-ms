using Newtonsoft.Json;

namespace Infra.Payment.MercadoPago.Entities
{
    public class Sponsor
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }
}
