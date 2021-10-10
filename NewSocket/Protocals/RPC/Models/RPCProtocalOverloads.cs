using NewSocket.Protocals.RPC.Handlers;
using NewSocket.Protocals.RPC.Models.Delegates;
using System;

namespace NewSocket.Protocals.RPC
{
    public partial class RPCProtocal
    {
        public void Subscribe(string name, AsyncRPCHandlerArgs handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1>(string name, AsyncRPCHandlerArgs<A1> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1, A2>(string name, AsyncRPCHandlerArgs<A1, A2> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1, A2, A3>(string name, AsyncRPCHandlerArgs<A1, A2, A3> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1, A2, A3, A4>(string name, AsyncRPCHandlerArgs<A1, A2, A3, A4> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1, A2, A3, A4, A5>(string name, AsyncRPCHandlerArgs<A1, A2, A3, A4, A5> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1>(string name, AsyncFuncRPCHandlerArgs<R, A1> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1, A2>(string name, AsyncFuncRPCHandlerArgs<R, A1, A2> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1, A2, A3>(string name, AsyncFuncRPCHandlerArgs<R, A1, A2, A3> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1, A2, A3, A4>(string name, AsyncFuncRPCHandlerArgs<R, A1, A2, A3, A4> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1, A2, A3, A4, A5>(string name, AsyncFuncRPCHandlerArgs<R, A1, A2, A3, A4, A5> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe(string name, RPCHandlerArgs handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1>(string name, RPCHandlerArgs<A1> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1, A2>(string name, RPCHandlerArgs<A1, A2> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1, A2, A3>(string name, RPCHandlerArgs<A1, A2, A3> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1, A2, A3, A4>(string name, RPCHandlerArgs<A1, A2, A3, A4> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<A1, A2, A3, A4, A5>(string name, RPCHandlerArgs<A1, A2, A3, A4, A5> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R>(string name, FuncRPCHandlerArgs<R> handler)
        {
            SubscribeInternal(name, handler);
        }


        public void Subscribe<R, A1>(string name, FuncRPCHandlerArgs<R, A1> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1, A2>(string name, FuncRPCHandlerArgs<R, A1, A2> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void SubscribeRE<R, A1, A2>(string name, FuncRPCHandlerArgs<R, A1, A2> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1, A2, A3>(string name, FuncRPCHandlerArgs<R, A1, A2, A3> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1, A2, A3, A4>(string name, FuncRPCHandlerArgs<R, A1, A2, A3, A4> handler)
        {
            SubscribeInternal(name, handler);
        }

        public void Subscribe<R, A1, A2, A3, A4, A5>(string name, FuncRPCHandlerArgs<R, A1, A2, A3, A4, A5> handler)
        {
            SubscribeInternal(name, handler);
        }

        internal virtual void SubscribeInternal(string name, Delegate handler)
        {
            var global = new GlobalDelegateHandler(name, handler);
            HandlerRegistry.Register(name, global);
        }

        public bool Unsubscribe(string name) => HandlerRegistry.TryDeregister(name);
    }
}