using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceCommunication.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceCommunication
{
    [Route("proxy")]
    public class ProxyController<T> : ControllerBase
    {
        private T Service;

        public ProxyController(T service)
        {
            Service = service;
        }

        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok("Service is healthy.");
        }

        [HttpPost]
        public async Task<IActionResult> Execute([FromBody] string rawInput)
        {
            var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.All };
            try
            {
                var input = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawInput);

                // Get the correct method implementation in the service
                var type = Service.GetType();
                var methods = type.GetMethods();
                var methodName = (string) input["methodName"];
                input.Remove("methodName");

                var method = methods.First(m => m.Name == methodName);

                object result = null;
                if (method.ReturnType.Name.Contains("Task"))
                    result = await type.ExecuteTypeMethodAsync(methodName, Service, input);
                else
                    result = type.ExecuteTypeMethod(methodName, Service, input);

                return Ok(result);
            }
            catch (System.Exception e)
            {
                var serialized = JsonConvert.SerializeObject(e.ToString(), serializerSettings);
                return StatusCode(500, $"service_exception_{serialized}");
            }
        }
    }
}
