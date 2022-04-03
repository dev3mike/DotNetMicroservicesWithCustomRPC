using Microsoft.Extensions.DependencyInjection;
using ServiceCommunication.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication.Extensions
{
    public static class ServiceCommunicationExtensions
    {
        /// <summary>
        /// Add a proxy to a service that implements the contract <typeparamref name="T"/> to the DI container.
        /// </summary>
        /// <typeparam name="T">The service contract.</typeparam>
        /// <param name="services">The service collection (DI container)</param>
        /// <returns>The configured service collection</returns>
        public static IServiceCollection AddServiceCommunication<T>(this IServiceCollection services, string serviceName, string servicePort)
        {
            if (!typeof(T).IsInterface)
                throw new InvalidOperationException("Generic type must be interface");

            return services.AddSingleton(typeof(T), sp => ProxyClassGenerator.GenerateClass<T>(sp.GetService<HttpCommunication>(), serviceName, servicePort));
        }
    }
}
