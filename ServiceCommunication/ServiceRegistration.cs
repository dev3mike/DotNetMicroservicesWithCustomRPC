using Microsoft.Extensions.DependencyInjection;
using ServiceCommunication.Extensions;
using ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication
{
    public static class ServiceRegistration
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            // Service Communications
            services.AddServiceCommunication<IUserService>(serviceName: "UserService", servicePort: "1234");

            return services;
        }
    }
}
