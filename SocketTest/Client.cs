using NewSocket.Core;
using NewSocket.Models;
using NewSocket.Protocals.RPC;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest
{
    public delegate Task<string> ResponseDelegate(string a, string b, string c, CancellationToken token);

    public delegate Task<string> ResponseDelegate2(string a1);

    public delegate Task<string> ResponseDelegate3(string a1, AuthValues d);

    public delegate Task ResponseDelegate4(string a1, AuthValues d, int dd);

    public delegate AuthValues ResponseDelegate5(string a1, string a2, string a3);

    public class Client
    {
        public NewSocketClient Server;
        public RPCProtocal RPC;
        public AsyncWaitHandle AuthWait = new AsyncWaitHandle();

        public Client(Stream stream)
        {
            Server = new NewSocketClient(stream, new SocketClientConfig() { RPCEnabled = true, Role = EClientRole.Client });
            Server.Name = "Client";

            if (Server.RPC == null)
            {
                throw new InvalidCastException();
            }
            RPC = Server.RPC;

            RPC.RegisterFrom(this);

            Server.Start();

            ThreadPool.QueueUserWorkItem(async (_) =>
            {
                await Run();
            });
        }

        public async Task Run()
        {
        }

        [RPC]
        public async Task RunSetup()
        {
            var r = await RPC.QueryAsync<string>("GetName");
            Console.WriteLine($"Server Name: {r}");
            AuthWait.Release();
        }
    }
}