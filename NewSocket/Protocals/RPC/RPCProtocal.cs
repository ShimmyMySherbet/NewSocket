using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Interfaces;
using NewSocket.Models;
using NewSocket.Protocals.RPC.Handlers;
using NewSocket.Protocals.RPC.Interfaces;
using NewSocket.Protocals.RPC.Models;
using NewSocket.Protocals.RPC.Models.Registry;

namespace NewSocket.Protocals.RPC
{
    public partial class RPCProtocal : IMessageProtocal
    {
        public virtual byte ID => 1;

        private IDAssigner RPCAssigner = new IDAssigner();

        public ISocketClient SocketClient { get; }

        public IRPCRequestRegistry RequestRegistry = new RPCRequestRegistry();

        public IRPCHandlerRegistry HandlerRegistry = new RPCHandlerRegistry();

        protected bool m_Invalid = false;

        public RPCProtocal(ISocketClient socketClient)
        {
            SocketClient = socketClient;
        }

        //public Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client)
        //{
        //    return Task.FromResult((IMessageDown)new RPCDown(messageID, this));
        //}

        //public IMessageUp CreateRPCCall(string method, params object[] parameters)
        //{
        //    var msg = new RPCUp(SocketClient, SocketClient.MessageIDAssigner.AssignID(), RPCAssigner.AssignID(), method, parameters);
        //    return msg;
        //}

        //public IMessageUp CreateRPCResponse(ulong parentRPCID, object response)
        //{
        //    var msg = new RPCUp(SocketClient, SocketClient.MessageIDAssigner.AssignID(), parentRPCID, response);
        //    return msg;
        //}

        //public IMessageUp CreateRPCResponse(ulong parentRPCID)
        //{
        //    var msg = new RPCUp(SocketClient, SocketClient.MessageIDAssigner.AssignID(), parentRPCID);
        //    return msg;
        //}

        public virtual Task<IMessageDown> CreateDown(ulong messageID, BaseSocketClient client)
        {
            return Task.FromResult((IMessageDown)new RPCDown(messageID, this));
        }

        public virtual IMessageUp CreateRPCCall(string method, out RPCHandle handle, params object?[]? parameters)
        {
            handle = new RPCHandle(SocketClient.MessageIDAssigner.AssignID(), RPCAssigner.AssignID(), method);
            RequestRegistry.RegisterRequest(handle);
            var msg = new RPCUp(SocketClient, handle, method, parameters);
            return msg;
        }

        public virtual IMessageUp CreateRPCResponse(ulong parentRPCID, object? response)
        {
            var context = new RPCHandle(SocketClient.MessageIDAssigner.AssignID(), parentRPCID, $"R:{parentRPCID}");
            var msg = new RPCUp(SocketClient, context, response);
            return msg;
        }

        public virtual IMessageUp CreateRPCResponse(ulong parentRPCID)
        {
            var context = new RPCHandle(SocketClient.MessageIDAssigner.AssignID(), parentRPCID, $"R:{parentRPCID}");
            var msg = new RPCUp(SocketClient, context);
            return msg;
        }

        public async Task<T?> QueryAsync<T>(string method, params object[] parameters)
        {
            var msg = CreateRPCCall(method, out var handle, parameters: parameters);
            SocketClient.Enqueue(msg);

            var resp = await handle.WaitAsync();

            if (resp.Objects.Count > 0)
            {
                return resp.ReadObject<T>(0);
            }

            throw new InvalidOperationException("Remote RPC didn't return anything");
        }

        public async Task InvokeAsync(string method, params object?[]? parameters)
        {
            var msg = CreateRPCCall(method, out var handle, parameters: parameters);
            SocketClient.Enqueue(msg);
            await handle.WaitAsync();
        }

        public virtual void Subscribe(string name, Delegate handler)
        {
            var global = new GlobalDelegateHandler(name, handler);
            HandlerRegistry.Register(name, global);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public virtual void RegisterFrom<T>(T instance) where T : class
        {
            var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            if (methods == null)
            {
                return;
            }
            foreach (var method in methods)
            {
                if (method == null) continue;
                if (method.GetCustomAttributes<RPCAttribute>().Any())
                {
                    var del = DelegateFactory.CreateDelegate(method, instance);
                    if (del != null)
                    {
                        foreach (var rpcAttrib in method.GetCustomAttributes<RPCAttribute>())
                        {
                            var name = rpcAttrib.MethodName ?? method.Name;
                            var handler = new GlobalDelegateHandler(name, del);
                            HandlerRegistry.Register(name, handler);
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[WARN] Failed to generate delegate handler for RPC {method.DeclaringType?.FullName}::{method.Name}");
                    }
                }
            }
        }

        public virtual void DispatchRPC(ulong RPCID, string? method, string[] arguments)
        {
            if (method == null) return;
            ThreadPool.QueueUserWorkItem(async (_) => await HandleRPC(RPCID, method, arguments));
        }

        /// <summary>
        /// Creates a delegate handle for the remote RPC, using the method name attributed to the delegate type.
        /// Uses <see cref="RPCAttribute"/> from the delegate type.
        /// </summary>
        /// <typeparam name="T">Delegate type tagged with <see cref="RPCAttribute"/></typeparam>
        /// <returns>RPC delegate handle</returns>
        public T GetRPC<T>() where T : Delegate
        {
            var attr = typeof(T).GetCustomAttribute<RPCAttribute>();
            if (attr == null || attr.MethodName == null)
            {
                throw new ArgumentException("Delegate is not tagged with remote RPC name. Use GetRPC<T>(string method) to specify remote RPC name.");
            }
            else
            {
                return GetRPC<T>(attr.MethodName);
            }
        }

        /// <summary>
        /// Creates a delegate handle for the remote RPC, using the specified remote RPC name.
        /// </summary>
        /// <typeparam name="T">Delegate type</typeparam>
        /// <param name="method">Name of the rmeote RPC method</param>
        /// <returns>RPC delegate handle</returns>
        public T GetRPC<T>(string method) where T : Delegate
        {
            return (T)GetRPC(method, typeof(T));
        }

        public Delegate GetRPC(string method, Type delegaateType)
        {
            if (DelegateTools.GetDelegateInfo(delegaateType, out var ReturnType, out var delegateParameters))
            {
                DelegateTools.GetReturnTypeInfo(ReturnType, out var delegateIsAsync, out var delegateReturnType, out bool delegateReturns, out _);

                foreach (var proxyClass in Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(RPCProxy).IsAssignableFrom(x)))
                {
                    var proxyMethod = proxyClass.GetMethod("Execute", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (proxyMethod == null) continue;
                    DelegateTools.GetReturnTypeInfo(proxyMethod.ReturnType, out var proxyIsAsync, out var proxyReturnType, out var proxyReturns, out _);

                    var proxyParameters = proxyMethod.GetParameters();
                    var proxyGenerics = proxyClass.GetGenericArguments();

                    if (proxyParameters.Length == delegateParameters.Length)
                    {
                        if (delegateIsAsync != proxyIsAsync)
                            continue;
                        if (delegateReturns != proxyReturns)
                            continue;

                        var buildGenerics = new Type[proxyGenerics.Length];

                        var compatible = false;

                        if (proxyReturnType == delegateReturnType)
                        {
                            compatible = true;
                        }
                        else if (proxyReturnType.IsGenericParameter)
                        {
                            buildGenerics[proxyReturnType.GenericParameterPosition] = delegateReturnType;
                            compatible = true;
                        }
                        else
                        {
                            // bad return
                            compatible = false;
                        }

                        if (compatible)
                        {
                            for (int i = 0; i < delegateParameters.Length; i++)
                            {
                                var proxyParam = proxyParameters[i];
                                var delegateParam = delegateParameters[i];

                                if (proxyParam.ParameterType.IsGenericParameter)
                                {
                                    var existing = buildGenerics[proxyParam.ParameterType.GenericParameterPosition];

                                    if (existing == null)
                                    {
                                        buildGenerics[proxyParam.ParameterType.GenericParameterPosition] = delegateParam.ParameterType;
                                    }
                                    else if (existing != delegateParam.ParameterType)
                                    {
                                        // generic is consumed and of a different typpe
                                        compatible = false;
                                        break;
                                    }
                                }
                                else if (proxyParam.ParameterType != delegateParam.ParameterType)
                                {
                                    // incompatible non-generic type
                                    compatible = false;
                                    break;
                                }
                            }
                        }

                        if (compatible)
                        {
                            for (int i = 0; i < buildGenerics.Length; i++)
                            {
                                if (buildGenerics[i] == null)
                                {
                                    // incomplete generic set.
                                    compatible = false;
                                }
                            }

                            if (!compatible)
                                continue;

                            Type proxyClassBuildType;
                            if (buildGenerics.Length > 0)
                            {
                                proxyClassBuildType = proxyClass.MakeGenericType(typeArguments: buildGenerics);
                            }
                            else if (!proxyClass.ContainsGenericParameters)
                            {
                                proxyClassBuildType = proxyClass;
                            }
                            else
                            {
                                // Delegate has generic parameters that are not accounted for.
                                compatible = false;
                                continue;
                            }

                            var activationArguments = new object[] { this, method, delegaateType };
                            RPCProxy? proxyInstance;

                            try
                            {
                                proxyInstance = (RPCProxy?)Activator.CreateInstance(proxyClassBuildType, args: activationArguments);
                                if (proxyInstance == null)
                                    throw new InvalidOperationException();
                            }
                            catch (Exception) // catch delegate activation exceptions
                            {
                                compatible = false;
                                continue;
                            }

                            try
                            {
                                var delegateBinding = Delegate.CreateDelegate(delegaateType, proxyInstance, "Execute", true, true);

                                if (delegateBinding == null)
                                    throw new InvalidOperationException();

                                return delegateBinding;
                            }
                            catch (Exception) // catch delegate binding and casting exceptions
                            {
                                compatible = false;
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                throw new InvalidCastException("T has to be a valid delegate");
            }
            throw new ArgumentException("Failed to bind proxy delegate for specified delegate type");
        }


        private async Task HandleRPC(ulong id, string method, string[] args)
        {
            var handler = HandlerRegistry.GetHandler(method);
            if (handler != null)
            {
                var paramList = new List<object?>();
                if (args.Length >= handler.Parameters.Length)
                {
                    var parameters = new RPCData(args.ToList());
                    for (int i = 0; i < handler.Parameters.Length; i++)
                    {
                        var pType = handler.Parameters[i];
                        paramList.Add(parameters.ReadObject(i, pType));
                    }

                    try
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        var task = handler.Execute(paramList.ToArray());

                        var returnValue = await task;

                        if (handler.HasReturn)
                        {
                            var outBound = CreateRPCResponse(id, returnValue);

                            SocketClient.Enqueue(outBound);
                        }
                        else
                        {
                            var outbound = CreateRPCResponse(id);
                            SocketClient.Enqueue(outbound);
                        }
                        return;
                    }
                    catch (System.Exception)
                    {
                        throw;
                        // TODO: Exception proxy logic
                    }
                }
            }
            else
            {
                Cout.Write("RPC", $"Failed to find handler for {method}. ID: {id}");
            }
            var msg = CreateRPCResponse(id);
            SocketClient.Enqueue(msg);
        }

        public Task OnSocketDisconnect(DisconnectContext context)
        {
            RequestRegistry.SendShutdown(context);
            return Task.CompletedTask;
        }
    }
}