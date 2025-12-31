namespace Infra.Payment.MercadoPago.Entities
{
    public class MercadoPagoMerchantOrder
    {
        public long Id { get; set; }
        public string Status { get; set; }
        public string External_Reference { get; set; }
        public string Preference_Id { get; set; }
        public List<Payment> Payments { get; set; }
        public List<object> Shipments { get; set; }
        public List<object> Payouts { get; set; }
        public Collector Collector { get; set; }
        public string Marketplace { get; set; }
        public string Notification_Url { get; set; }
        public DateTime Date_Created { get; set; }
        public DateTime Last_Updated { get; set; }
        public long Sponsor_Id { get; set; }
        public double Shipping_Cost { get; set; }
        public double Total_Amount { get; set; }
        public string Site_Id { get; set; }
        public double Paid_Amount { get; set; }
        public double Refunded_Amount { get; set; }
        public Payer Payer { get; set; }
        public List<MerchantOrderItem> Items { get; set; }
        public bool Cancelled { get; set; }
        public string Additional_Info { get; set; }
        public object Application_Id { get; set; }
        public bool Is_Test { get; set; }
        public string Order_Status { get; set; }
        public string Client_Id { get; set; }
    }

    public class Payment
    {
        public long Id { get; set; }
        public double Transaction_Amount { get; set; }
        public double Total_Paid_Amount { get; set; }
        public double Shipping_Cost { get; set; }
        public string Currency_Id { get; set; }
        public string Status { get; set; }
        public string Status_Detail { get; set; }
        public string Operation_Type { get; set; }
        public DateTime Date_Approved { get; set; }
        public DateTime Date_Created { get; set; }
        public DateTime Last_Modified { get; set; }
        public double Amount_Refunded { get; set; }
    }

    public class Collector
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
    }

    public class Payer
    {
        public long Id { get; set; }
        public string Email { get; set; }
    }

    public class MerchantOrderItem
    {
        public string Id { get; set; }
        public string Category_Id { get; set; }
        public string Currency_Id { get; set; }
        public string Description { get; set; }
        public string Picture_Url { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public double Unit_Price { get; set; }
    }

}
