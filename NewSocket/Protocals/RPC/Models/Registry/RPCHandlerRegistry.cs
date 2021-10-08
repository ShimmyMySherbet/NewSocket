using NewSocket.Protocals.RPC.Interfaces;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NewSocket.Protocals.RPC.Models.Registry
{
    public class RPCHandlerRegistry : IRPCHandlerRegistry
    {
        private ConcurrentDictionary<string, IRPCHandler> m_Handlers = new ConcurrentDictionary<string, IRPCHandler>(StringComparer.InvariantCultureIgnoreCase);

        public void Register(string name, IRPCHandler handler)
        {
            m_Handlers[name] = handler;
        }

        public bool TryDeregister(string name)
        {
            return m_Handlers.TryRemove(name, out var _);
        }

        public IRPCHandler GetHandler(string name)
        {
            if (m_Handlers.TryGetValue(name, out var handler))
            {
                return handler;
            }
            return null;
        }

        public bool TryDeregister(IRPCHandler handler)
        {
            var matches = m_Handlers.Where(x => x.Value == handler);
            foreach (var m in matches)
            {
                m_Handlers.Remove(m.Key, out _);
            }
            return matches.Any();
        }
    }
}