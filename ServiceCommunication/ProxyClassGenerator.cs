using ServiceCommunication.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ServiceCommunication
{
    public class ProxyClassGenerator
    {
        internal static T GenerateClass<T>(IHttpCommunication httpCommunication, string serviceName, string servicePort)
        {
            var t = typeof(T);
            var inheritedTypes = t.GetInterfaces();

            return CreateServiceClass<T>(httpCommunication, serviceName, servicePort, inheritedTypes);
        }

        private static T CreateServiceClass<T>(IHttpCommunication httpCommunication, string serviceName, string servicePort, Type[] inheritedTypes)
        {
            try
            {
                var t = typeof(T);
                var typeBuilder = CreateTypeBuilder(); // Creates a class type

                var methods = t.GetMethods().ToList();
                if (inheritedTypes != null)
                    foreach (Type type in inheritedTypes)
                    {
                        methods.AddRange(type.GetMethods());
                    }

                Type proxyType = typeof(ServiceProxy.ServiceProxy);

                // Add proxy field to the class
                var field = AddProxyField(typeBuilder, proxyType);

                // Add a constructor to the class
                AddConstructor(typeBuilder, field, proxyType);

                foreach (var m in methods)
                {
                    // Create and add a method implementation for each method in the interface to the class
                    CreateServiceMethod(m, typeBuilder, field, serviceName, servicePort, proxyType);
                }

                typeBuilder.AddInterfaceImplementation(t); // Add interface implementation to the class

                return (T)Activator.CreateInstance(typeBuilder.CreateTypeInfo().AsType(), new ServiceProxy.ServiceProxy(httpCommunication)); // Create class type and instantiate it
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        private static void AddConstructor(TypeBuilder typeBuilder, FieldInfo proxyField, Type proxyType)
        {
            // Create constructor for the class that that takes a input argument of type ServiceProxy
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public
                , CallingConventions.Standard
                , new[] { proxyType });

            // Get Intermediate Language Generator for the constructor
            var il = constructorBuilder.GetILGenerator(); 

            // Adds a body to the constructor that updates the value of the field with the input value
            il.Emit(OpCodes.Ldarg_0); // Loading <this> onto the stack, https://stackoverflow.com/questions/1473346/msil-question-basic?rq=1
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, proxyField);
            il.Emit(OpCodes.Ret);
        }

        // Adds a method and method body to the class
        private static void CreateServiceMethod(MethodInfo m, TypeBuilder tb, FieldInfo proxyField, string serviceName, string servicePort, Type proxyType)
        {
            var il = GetMethodILGenerator(m, tb);

            var isTask = m.ReturnType.Name.Contains("Task");

            Type tArgument = null;
            if (isTask && m.ReturnType.IsGenericType)
                tArgument = m.ReturnType.GenericTypeArguments[0];
            else
                tArgument = m.ReturnType;

            var methodName = isTask ? "ExecuteAsync" : "Execute";

            methodName = GetValueTypeExecutionMethodName(methodName, tArgument);

            var proxyMethod = GetCorrectMethod(proxyType.GetMethod(methodName), tArgument);

            // Adds a method call to the Execute method in ServiceProxy on the _proxy field with the service uri, method name and method return type name as inputs
            // and then returns from the method body
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, proxyField);
            il.Emit(OpCodes.Ldstr, serviceName);
            il.Emit(OpCodes.Ldstr, servicePort);
            il.Emit(OpCodes.Ldstr, m.Name);
            il.Emit(OpCodes.Ldstr, tArgument.AssemblyQualifiedName);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, proxyMethod);
            il.Emit(OpCodes.Ret);
        }

        // Want to invoke either Execute/ExecuteAsync or ExecuteValueType/ExecuteAsyncValueType depending on if the return type is a ValueType or not
        private static string GetValueTypeExecutionMethodName(string baseName, Type type)
        {
            var returnName = baseName;
            if (type.IsValueType)
            {
                returnName = baseName + "ValueType";
            }

            return returnName.Replace("`", "");
        }

        // Make sure the right type is sent to the generic ExecuteValueType/ExecuteAsyncValueType for unboxing
        private static MethodInfo GetCorrectMethod(MethodInfo proxyMethod, Type returnArgumentType)
        {
            if (returnArgumentType.IsValueType)
            {
                if (returnArgumentType.IsGenericType)
                {
                    if (returnArgumentType.GenericTypeArguments.Count() > 1)
                        return proxyMethod.MakeGenericMethod(returnArgumentType);

                    return proxyMethod.MakeGenericMethod(returnArgumentType.GenericTypeArguments);
                }
                else
                    return proxyMethod.MakeGenericMethod(returnArgumentType);
            }

            return proxyMethod;
        }

        private static ILGenerator GetMethodILGenerator(MethodInfo m, TypeBuilder tb)
        {
            var parameters = m.GetParameters();
            // Create the method definetion
            var mb = tb.DefineMethod(m.Name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName, CallingConventions.Standard, m.ReturnType, parameters.Select(p => p.ParameterType).ToArray());

            var il = mb.GetILGenerator();
            // Creates a local variable of type Dictionary<string,object>
            il.DeclareLocal(typeof(Dictionary<string, object>));
            il.Emit(OpCodes.Newobj, typeof(Dictionary<string, object>).GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc_0);

            var i = 0;
            // Adds each input parameter to the dictionary, Dictionary.Add(nameof(p), p)
            foreach (var p in parameters)
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldstr, p.Name);
                il.Emit(OpCodes.Ldarg, (short)(i + 1));

                if (p.ParameterType.GetTypeInfo().IsValueType)
                    il.Emit(OpCodes.Box, p.ParameterType);

                il.Emit(OpCodes.Callvirt, typeof(Dictionary<string, object>).GetMethod("Add"));

                i += 1;
            }

            return il;
        }

        private static FieldInfo AddProxyField(TypeBuilder typeBuilder, Type proxyType)
        {
            // Add the private ServiceProxy _proxy field to the class
            return typeBuilder.DefineField("_proxy", proxyType, FieldAttributes.Private);
        }

        // Defines a dynamic class type
        private static TypeBuilder CreateTypeBuilder()
        {
            var typeSignature = "DynamicType";
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var tb = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, null);

            return tb;
        }
    }
}
