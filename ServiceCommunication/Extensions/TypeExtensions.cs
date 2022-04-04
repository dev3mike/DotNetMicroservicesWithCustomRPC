using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceCommunication.Extensions
{
    public static class TypeExtensions
    {
        public static object ExecuteTypeMethod<T>(this Type type, string methodName, T instance, Dictionary<string, object> inputs = null)
        {
            var methods = type.GetMethods();
            MethodInfo method = null;
            object[] arguments = new object[] { };
            if (inputs != null && inputs.Count() != 0)
            {
                method = methods.FirstOrDefault(m => m.Name == methodName && inputs.All(i => m.GetParameters().Any(a => a.Name == i.Key)));

                var parameters = method.GetParameters();
                // Get and convert all the inputs to match the input parameters to the method
                arguments = GenerateArguments(parameters, inputs);
            }
            else
                method = methods.FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 0);

            // Call the method
            var result = method.Invoke(instance, arguments);

            return result;
        }

        public static async Task<object> ExecuteTypeMethodAsync<T>(this Type type, string methodName, T instance, Dictionary<string, object> inputs = null)
        {
            // Convert the result to a task
            var task = (Task)ExecuteTypeMethod(type, methodName, instance, inputs);

            await task;
            // Get the result from the method
            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty?.GetValue(task);

            return result;
        }

        private static object[] GenerateArguments(ParameterInfo[] parameters, Dictionary<string, object> inputs)
        {
            var arguments = new List<object>();
            foreach (var p in parameters)
            {
                if (!inputs.ContainsKey(p.Name))
                {
                    if (p.HasDefaultValue)
                    {
                        arguments.Add(p.DefaultValue);
                    }
                }
                else
                {
                    var value = inputs[p.Name];
                    if (value == null)
                    {
                        if (p.HasDefaultValue)
                            arguments.Add(p.DefaultValue);
                        else
                            arguments.Add(value);
                    }
                    else
                    {
                        var paramType = p.ParameterType;
                        if (value.GetType() != paramType)
                        {
                            var stringValue = JsonConvert.SerializeObject(value);
                            value = JsonConvert.DeserializeObject(stringValue, paramType);
                        }
                        arguments.Add(value);
                    }
                }
            }

            return arguments.ToArray();
        }
    }
}
