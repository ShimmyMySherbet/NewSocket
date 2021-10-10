namespace NewSocket.Protocals.RPC.Interfaces
{
    public interface IRPCHandlerRegistry
    {
        void Register(string name, IRPCHandler handler);

        bool TryDeregister(string name);

        bool TryDeregister(IRPCHandler handler);

        IRPCHandler? GetHandler(string name);
    }
}