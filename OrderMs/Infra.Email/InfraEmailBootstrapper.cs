using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Email
{
    public class InfraEmailBootstrapper
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}
