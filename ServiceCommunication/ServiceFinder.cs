using ServiceCommunication.Abstractions;
using ServiceCommunication.Models;
using ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication
{
    public class ServiceFinder : IServiceFinder
    {
        public Service GetService(string serviceName)
        {
            return GetAllServices().FirstOrDefault(x => x.Name == serviceName);
        }

        public List<Service> GetAllServices()
        {
            var services = new List<Service>
            {
                new Service { Name = "UserService", PortNumber = "1234" }
            };

            return services;
        }
    }
}
