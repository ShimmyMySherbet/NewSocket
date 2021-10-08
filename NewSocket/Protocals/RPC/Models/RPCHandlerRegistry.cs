using NewSocket.Protocals.RPC.Models.Handlers;
using System;
using System.Collections.Concurrent;

namespace NewSocket.Protocals.RPC.Models
{
    public class RPCHandlerRegistry
    {
        private ConcurrentDictionary<string, RPCHandler> m_Handlers = new ConcurrentDictionary<string, RPCHandler>(StringComparer.InvariantCultureIgnoreCase);

        public void Register(string name, Delegate handler)
        {
            var rpcHandler = new RPCHandler(name, handler);
            m_Handlers[name] = rpcHandler;
        }

        public bool TryDeregister(string name)
        {
            return m_Handlers.TryRemove(name, out var _);
        }

        public RPCHandler GetHandler(string name)
        {
            if (m_Handlers.TryGetValue(name, out var handler))
            {
                return handler;
            }
            return null;
        }
    }
}