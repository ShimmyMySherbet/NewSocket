using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NewSocket.Models;
using NewSocket.Protocals.NetSynced;
using NewSocket.Protocals.NetSynced.Models;
using NewSocket.Protocals.RPC;
using NewSocket.Protocals.RPC.Auto;
using NewSocket.Security.Interfaces;

namespace NewSocket.Core
{
    public class RPCSocketClient : BaseSocketClient
    {
        public RPCProtocal RPC { get; }
        public NetSyncedProtocol NetSynced { get; }
        public ISecurityProtocal? Security { get; }

        public bool Authenticated => Security == null || Security.Authenticated;

        internal Stream UnderlyingNetwork;

        public RPCSocketClient(Stream network, ISecurityProtocal? protocol = null)
        {
            UnderlyingNetwork = network;
            RPC = RegisterProtocal(new RPCProtocal(this));
            NetSynced = RegisterProtocal(new NetSyncedProtocol(this));
            Security = protocol;
            if (Security != null)
            {
                if (!Security.Preauthenticate)
                {
                    var up = Security.GetUpStream(network);
                    var down = Security.GetDownStream(network);

                    if (up != null)
                    {
                        SetStream(ESocketStream.Up, up);
                    }
                    if (down != null)
                    {
                        SetStream(ESocketStream.Down, down);
                    }
                }
            }
            else
            {
                SetStream(ESocketStream.Both, network);
            }
        }

        protected override async Task OnMessageSent()
        {
            if (Security != null)
            {
                await Security.MessageSent();
            }
        }

        public async Task StartAsync()
        {
            if (Security != null && Security.Preauthenticate)
            {
                await Security.Authenticate(UnderlyingNetwork);
                var up = Security.GetUpStream(UnderlyingNetwork);
                var down = Security.GetDownStream(UnderlyingNetwork);

                if (up != null)
                {
                    SetStream(ESocketStream.Up, up);
                }
                if (down != null)
                {
                    SetStream(ESocketStream.Down, down);
                }
            }
            Start();
            if (Security != null)
            {
                await Security.OnSocketStarted(this);
            }
        }

        public NetSyncedStream CreateStream(ENetSyncedMode mode)
        {
            bool readable = false;
            bool writable = false;
            switch(mode)
            {
                case ENetSyncedMode.Read:
                    readable = true;
                    break;
                case ENetSyncedMode.Write:
                    writable = true;
                    break;
                case ENetSyncedMode.ReadWrite:
                    readable = true;
                    writable = true;
                    break;
                default:
                    throw new ArgumentException($"Invalid mode for this networked stream: {mode}");
            }

            return NetSynced.CreateStream(readable, writable);
        }

        public async Task<NetSyncedStream> GetStreamAsync(ulong netSyncedID)
        {
            return await NetSynced.GetStream(netSyncedID);
        }

        public override void Start()
        {
            if (Security != null && !Security.Authenticated)
            {
                if (!Security.Authenticated && UpStream == null && DownStream == null)
                {
                    throw new UnauthorizedAccessException("Security protocol has not yet been initialized. Use StartAsync() or initialize the security protocol directly");
                }
            }

            base.Start();
        }

        public Task<T?> QueryAsync<T>(string method, params object[] parameters) => RPC.QueryAsync<T>(method, parameters: parameters);

        public Task InvokeAsync(string method, params object?[]? parameters) => RPC.InvokeAsync(method, parameters: parameters);

        public void RegisterFrom<T>(T instance) where T : class => RPC.RegisterFrom<T>(instance);

        public T GetRPC<T>() where T : Delegate => RPC.GetRPC<T>();

        public T GetRPC<T>(string method) where T : Delegate => RPC.GetRPC<T>(method);

        private void AutoInit()
        {
            var cType = GetType();
            if (cType.GetCustomAttribute<RPCAutoRegisterAttribute>() != null)
            {
                RegisterFrom(this);
            }

            var initAll = cType.GetCustomAttribute<RPCAutoInitAttribute>() != null;

            foreach (var field in cType.GetFields())
            {
                if (initAll || field.GetCustomAttribute(typeof(RPCAutoInitAttribute)) != null)
                {
                    if (field.FieldType.IsSubclassOf(typeof(Delegate)))
                    {
                        var RPCAttrib = field.FieldType.GetCustomAttribute(typeof(RPCAttribute));
                        if (RPCAttrib != null)
                        {
                            var atrb = RPCAttrib as RPCAttribute;
                            if (atrb != null && atrb.MethodName != null)
                            {
                                try
                                {
                                    field.SetValue(this, RPC.GetRPC(atrb.MethodName, field.FieldType));
                                }
                                catch (ArgumentException) // Catch bind errors
                                {
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}