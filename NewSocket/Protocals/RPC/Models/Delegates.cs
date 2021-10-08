using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Models
{
    public delegate Task AsyncRPCHandler();
    public delegate Task AsyncRPCHandler<A1>(A1 argument1);
    public delegate Task AsyncRPCHandler<A1, A2>(A1 argument1, A2 argument2);
    public delegate Task AsyncRPCHandler<A1, A2, A3>(A1 argument1, A2 argument2, A3 argument3);
    public delegate Task AsyncRPCHandler<A1, A2, A3, A4>(A1 argument1, A2 argument2, A3 argument3, A4 argument4);
}
