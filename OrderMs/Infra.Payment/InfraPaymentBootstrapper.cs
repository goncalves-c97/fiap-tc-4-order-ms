using Core.Interfaces;
using Infra.Payment.MercadoPago;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Payment
{
    public class InfraPaymentBootstrapper
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IPagamentoService, PagamentoService>();
        }
    }
}
