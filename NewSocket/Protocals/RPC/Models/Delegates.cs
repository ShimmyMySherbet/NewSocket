using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Models.Delegates
{
    public delegate Task AsyncRPCHandlerArgs();

    public delegate Task AsyncRPCHandlerArgs<A1>(A1 argument1);

    public delegate Task AsyncRPCHandlerArgs<A1, A2>(A1 argument1, A2 argument2);

    public delegate Task AsyncRPCHandlerArgs<A1, A2, A3>(A1 argument1, A2 argument2, A3 argument3);

    public delegate Task AsyncRPCHandlerArgs<A1, A2, A3, A4>(A1 argument1, A2 argument2, A3 argument3, A4 argument4);

    public delegate Task AsyncRPCHandlerArgs<A1, A2, A3, A4, A5>(A1 argument1, A2 argument2, A3 argument3, A4 argument4, A5 argument5);

    public delegate Task<R> AsyncFuncRPCHandlerArgs<R>();

    public delegate Task<R> AsyncFuncRPCHandlerArgs<R, A1>(A1 argument1);

    public delegate Task<R> AsyncFuncRPCHandlerArgs<R, A1, A2>(A1 argument1, A2 argument2);

    public delegate Task<R> AsyncFuncRPCHandlerArgs<R, A1, A2, A3>(A1 argument1, A2 argument2, A3 argument3);

    public delegate Task<R> AsyncFuncRPCHandlerArgs<R, A1, A2, A3, A4>(A1 argument1, A2 argument2, A3 argument3, A4 argument4);

    public delegate Task<R> AsyncFuncRPCHandlerArgs<R, A1, A2, A3, A4, A5>(A1 argument1, A2 argument2, A3 argument3, A4 argument4, A5 argument5);

    public delegate void RPCHandlerArgs();

    public delegate void RPCHandlerArgs<A1>(A1 argument1);

    public delegate void RPCHandlerArgs<A1, A2>(A1 argument1, A2 argument2);

    public delegate void RPCHandlerArgs<A1, A2, A3>(A1 argument1, A2 argument2, A3 argument3);

    public delegate void RPCHandlerArgs<A1, A2, A3, A4>(A1 argument1, A2 argument2, A3 argument3, A4 argument4);

    public delegate void RPCHandlerArgs<A1, A2, A3, A4, A5>(A1 argument1, A2 argument2, A3 argument3, A4 argument4, A5 argument5);

    public delegate R FuncRPCHandlerArgs<R>();

    public delegate R FuncRPCHandlerArgs<R, A1>(A1 argument1);

    public delegate R FuncRPCHandlerArgs<R, A1, A2>(A1 argument1, A2 argument2);

    public delegate R FuncRPCHandlerArgs<R, A1, A2, A3>(A1 argument1, A2 argument2, A3 argument3);

    public delegate R FuncRPCHandlerArgs<R, A1, A2, A3, A4>(A1 argument1, A2 argument2, A3 argument3, A4 argument4);

    public delegate R FuncRPCHandlerArgs<R, A1, A2, A3, A4, A5>(A1 argument1, A2 argument2, A3 argument3, A4 argument4, A5 argument5);
}