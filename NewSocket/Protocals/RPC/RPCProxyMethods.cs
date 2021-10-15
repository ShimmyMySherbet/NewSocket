using NewSocket.Protocals.RPC.Models;
using System;
using System.Threading.Tasks;

#pragma warning disable CS8603 // TODO: Handle null returns

namespace NewSocket.Protocals.RPC
{
    internal class RPCProxyMethods
    {
#pragma warning disable CS8604 // Possible null reference argument.

        public class AsyncInvoke : RPCProxy
        {
            public AsyncInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task Execute()
            {
                await RPC.InvokeAsync(Method);
            }
        }

        public class AsyncInvoke<A1> : RPCProxy
        {
            public AsyncInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task Execute(A1 arg1)
            {
                await RPC.InvokeAsync(Method, arg1);
            }
        }

        public class AsyncInvoke<A1, A2> : RPCProxy
        {
            public AsyncInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task Execute(A1 arg1, A2 arg2)
            {
                await RPC.InvokeAsync(Method, arg1, arg2);
            }
        }

        public class AsyncInvoke<A1, A2, A3> : RPCProxy
        {
            public AsyncInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task Execute(A1 arg1, A2 arg2, A3 arg3)
            {
                await RPC.InvokeAsync(Method, arg1, arg2, arg3);
            }
        }

        public class AsyncInvoke<A1, A2, A3, A4> : RPCProxy
        {
            public AsyncInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task Execute(A1 arg1, A2 arg2, A3 arg3, A4 arg4)
            {
                await RPC.InvokeAsync(Method, arg1, arg2, arg3, arg4);
            }
        }

        public class AsyncInvoke<A1, A2, A3, A4, A5> : RPCProxy
        {
            public AsyncInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task Execute(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5)
            {
                await RPC.InvokeAsync(Method, arg1, arg2, arg3, arg4, arg5);
            }
        }

        public class AsyncQuery<R> : RPCProxy
        {
            public AsyncQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task<R> Execute()
            {
                return await RPC.QueryAsync<R>(Method);
            }
        }

        public class AsyncQuery<R, A1> : RPCProxy
        {
            public AsyncQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task<R> Execute(A1 arg1)
            {
                return await RPC.QueryAsync<R>(Method, arg1);
            }
        }

        public class AsyncQuery<R, A1, A2> : RPCProxy
        {
            public AsyncQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task<R> Execute(A1 arg1, A2 arg2)
            {
                return await RPC.QueryAsync<R>(Method, arg1, arg2);
            }
        }

        public class AsyncQuery<R, A1, A2, A3> : RPCProxy
        {
            public AsyncQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task<R> Execute(A1 arg1, A2 arg2, A3 arg3)
            {
                return await RPC.QueryAsync<R>(Method, arg1, arg2, arg3);
            }
        }

        public class AsyncQuery<R, A1, A2, A3, A4> : RPCProxy
        {
            public AsyncQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task<R> Execute(A1 arg1, A2 arg2, A3 arg3, A4 arg4)
            {
                return await RPC.QueryAsync<R>(Method, arg1, arg2, arg3, arg4);
            }
        }

        public class AsyncQuery<R, A1, A2, A3, A4, A5> : RPCProxy
        {
            public AsyncQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public async Task<R> Execute(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5)
            {
                return await RPC.QueryAsync<R>(Method, arg1, arg2, arg3, arg4, arg5);
            }
        }

        public class BlockingInvoke : RPCProxy
        {
            public BlockingInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public void Execute()
            {
                RPC.InvokeAsync(Method).GetAwaiter().GetResult();
            }
        }

        public class BlockingInvoke<A1> : RPCProxy
        {
            public BlockingInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public void Execute(A1 arg1)
            {
                RPC.InvokeAsync(Method, arg1).GetAwaiter().GetResult();
            }
        }

        public class BlockingInvoke<A1, A2> : RPCProxy
        {
            public BlockingInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public void Execute(A1 arg1, A2 arg2)
            {
                RPC.InvokeAsync(Method, arg1, arg2).GetAwaiter().GetResult();
            }
        }

        public class BlockingInvoke<A1, A2, A3> : RPCProxy
        {
            public BlockingInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public void Execute(A1 arg1, A2 arg2, A3 arg3)
            {
                RPC.InvokeAsync(Method, arg1, arg2, arg3).GetAwaiter().GetResult();
            }
        }

        public class BlockingInvoke<A1, A2, A3, A4> : RPCProxy
        {
            public BlockingInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public void Execute(A1 arg1, A2 arg2, A3 arg3, A4 arg4)
            {
                RPC.InvokeAsync(Method, arg1, arg2, arg3, arg4).GetAwaiter().GetResult();
            }
        }

        public class BlockingInvoke<A1, A2, A3, A4, A5> : RPCProxy
        {
            public BlockingInvoke(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public void Execute(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5)
            {
                RPC.InvokeAsync(Method, arg1, arg2, arg3, arg4, arg5).GetAwaiter().GetResult();
            }
        }

        public class BlockingQuery<R> : RPCProxy
        {
            public BlockingQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public R Execute()
            {
                return RPC.QueryAsync<R>(Method).GetAwaiter().GetResult();
            }
        }

        public class BlockingQuery<R, A1> : RPCProxy
        {
            public BlockingQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public R Execute(A1 arg1)
            {
                return RPC.QueryAsync<R>(Method, arg1).GetAwaiter().GetResult();
            }
        }

        public class BlockingQuery<R, A1, A2> : RPCProxy
        {
            public BlockingQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public R Execute(A1 arg1, A2 arg2)
            {
                return RPC.QueryAsync<R>(Method, arg1, arg2).GetAwaiter().GetResult();
            }
        }

        public class BlockingQuery<R, A1, A2, A3> : RPCProxy
        {
            public BlockingQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public R Execute(A1 arg1, A2 arg2, A3 arg3)
            {
                return RPC.QueryAsync<R>(Method, arg1, arg2, arg3).GetAwaiter().GetResult();
            }
        }

        public class BlockingQuery<R, A1, A2, A3, A4> : RPCProxy
        {
            public BlockingQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public R Execute(A1 arg1, A2 arg2, A3 arg3, A4 arg4)
            {
                return RPC.QueryAsync<R>(Method, arg1, arg2, arg3, arg4).GetAwaiter().GetResult();
            }
        }

        public class BlockingQuery<R, A1, A2, A3, A4, A5> : RPCProxy
        {
            public BlockingQuery(RPCProtocal rPC, string method, Type delegateHandler) : base(rPC, method, delegateHandler)
            {
            }

            public R Execute(A1 arg1, A2 arg2, A3 arg3, A4 arg4, A5 arg5)
            {
                return RPC.QueryAsync<R>(Method, arg1, arg2, arg3, arg4, arg5).GetAwaiter().GetResult();
            }
        }
    }
}