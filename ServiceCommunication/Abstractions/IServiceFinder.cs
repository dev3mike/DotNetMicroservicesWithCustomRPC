using ServiceCommunication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication.Abstractions
{
    public interface IServiceFinder
    {
        Service GetService(string serviceName);
        List<Service> GetAllServices();
    }
}
