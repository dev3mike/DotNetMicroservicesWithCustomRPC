using ServiceCommunication.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication.ServiceProxy
{
    public class ServiceProxy
    {
        private readonly IHttpCommunication _http;

        public ServiceProxy(IHttpCommunication http)
        {
            _http = http;
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
                var serviceUri = "localhost"; // Todo: Should be moved to ServiceFinder service and fetched based on serviceName

                // Create a http request to the proxy controller injected into each service
                var type = Type.GetType(typeName);
                inputs.Add("methodName", methodName);

                var result = await _http.SendRequest($"{serviceUri}:{servicePort}/proxy", "post", type, payload: inputs);
                return result;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("exception_")) // Known service exception has been thrown
                {
                    Exception exception = null;
                    try
                    {
                        // This is a very ugly way to get the error parts but will have to do for now
                        //var index = e.Message.IndexOf(": exception_");
                        //var statusCodeString = e.Message.Substring(0, index);
                        //var statusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCodeString);
                        //var message = e.Message.Replace($"{statusCodeString}: exception_", "");
                        //exception = _serializer.Deserialize<Exception>(message, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.All });
                    }
                    catch (ArgumentException)
                    {
                        throw;
                    }
                    catch (OverflowException)
                    {
                        throw;
                    }
                    if (exception == null)
                        throw;

                    throw exception;
                }
                throw;
            }
        }
    }
}
