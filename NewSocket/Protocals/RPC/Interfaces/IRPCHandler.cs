using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Interfaces
{
    public interface IRPCHandler
    {
        string Name { get; }
        public Type[] Parameters { get; }

        public Type ReturnType { get; }

        public bool IsAsync { get; }

        public bool HasReturn { get; }

        Task<object?> Execute(object?[]? parameters);
    }
}
