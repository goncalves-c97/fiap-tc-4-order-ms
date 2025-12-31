namespace Infra.Payment.MercadoPago.Entities
{
    public class MercadoPagoWebhook
    {
        public string Resource { get; set; }
        public string Topic { get; set; }
    }
}
