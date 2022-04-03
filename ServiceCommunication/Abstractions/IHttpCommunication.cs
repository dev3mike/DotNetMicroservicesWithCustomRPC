using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCommunication.Abstractions
{
    public interface IHttpCommunication
    {
        Task<object> SendRequest(string uri, string method, Type type, Dictionary<string, string> headers = null, object payload = null);
    }
}
