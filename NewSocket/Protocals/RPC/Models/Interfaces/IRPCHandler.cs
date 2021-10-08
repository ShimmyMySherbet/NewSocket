using System;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Models.Interfaces
{
    public interface IRPCHandler
    {
        string Name { get; }

        public Type[] Parameters { get; }

        Task Execute(object[] parameters);
    }
}