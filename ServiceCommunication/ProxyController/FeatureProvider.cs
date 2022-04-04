using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication.ProxyController
{
    // Feature provider that lets the ProxyController to be loaded like any other "native" controller for a WebApi when configured in the Startup file
    public class FeatureProvider<T> : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var controllerType = typeof(ProxyController<T>).GetTypeInfo();
            feature.Controllers.Add(controllerType);
        }
    }
}
