namespace Core.Settings
{
    public sealed class AppSettings
    {
        public string AppUrl { get; init; } = default!;
        public string PaymentMicroserviceUrl { get; init; } = default!;
    }
}
