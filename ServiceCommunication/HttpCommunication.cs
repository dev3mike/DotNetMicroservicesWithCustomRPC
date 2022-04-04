using Newtonsoft.Json;
using RestSharp;
using ServiceCommunication.Abstractions;
using ServiceCommunication.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication
{
    public class HttpCommunication : IHttpCommunication
    {
        public async Task<object> SendRequest(string serviceUri, string method, Type type, Dictionary<string, string> headers = null, object payload = null)
        {
            var client = new RestClient(serviceUri);
            var request = new RestRequest();
            var jsonPayload = JsonConvert.SerializeObject(payload);

            if(headers is not null)
                request.AddHeaders(headers);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(jsonPayload);

            var response = await client.ExecutePostAsync(request);

            // Throw microservice exception
            if (response.ErrorException != null && !string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                throw new ServiceException(response.Content.Replace("service_exception_", ""));

            if(response.ErrorException != null)
                throw response.ErrorException;

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                if (type.IsValueType)
                    return Activator.CreateInstance(type);
                else
                    return null;
            }
            if (type == typeof(string))
                return response;

            return JsonConvert.DeserializeObject(response.Content, type);
        }
    }
}
