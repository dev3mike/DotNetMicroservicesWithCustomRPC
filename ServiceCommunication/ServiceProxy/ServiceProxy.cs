using Newtonsoft.Json;
using ServiceCommunication.Abstractions;
using ServiceCommunication.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication.ServiceProxy
{
    public class ServiceProxy
    {
        private readonly IHttpCommunication _http;

        public ServiceProxy(IHttpCommunication http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public T ExecuteValueType<T>(string serviceName, string servicePort, string methodName, string typeName, Dictionary<string, object> inputs)
        {
            var result = ExecuteAsync(serviceName, servicePort, methodName, typeName, inputs).GetAwaiter().GetResult();
            return (T)result;
        }

        public async Task<T> ExecuteAsyncValueType<T>(string serviceName, string servicePort, string methodName, string typeName, Dictionary<string, object> inputs)
        {
            var result = await ExecuteAsync(serviceName, servicePort, methodName, typeName, inputs);
            return (T)result;
        }

        public object Execute(string serviceName, string servicePort, string methodName, string typeName, Dictionary<string, object> inputs)
        {
            var result = ExecuteAsync(serviceName, servicePort, methodName, typeName, inputs).GetAwaiter().GetResult();
            return result;
        }

        public async Task<object> ExecuteAsync(string serviceName, string servicePort, string methodName, string typeName, Dictionary<string, object> inputs)
        {
            try
            {
                var serviceUri = "http://localhost"; // Todo: Should be moved to ServiceFinder service and fetched based on serviceName

                // Create a http request to the proxy controller injected into each service
                var type = Type.GetType(typeName);
                inputs.Add("methodName", methodName);

                var result = await _http.SendRequest($"{serviceUri}:{servicePort}/proxy", "post", type, payload: inputs);
                return result;
            }
            catch (ServiceException e)
            {
                // Log service exceptions here

                throw;
            }
        }
    }
}
