using ServiceCommunication.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication
{
    public class HttpCommunication : IHttpCommunication
    {
        public Task<object> SendRequest(string uri, string method, Type type, Dictionary<string, string> headers = null, object payload = null)
        {
            throw new NotImplementedException();
        }
    }
}
