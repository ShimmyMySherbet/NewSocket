using NewSocket.Core;
using NewSocket.Models;
using NewSocket.Protocals.RPC;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest
{
    [RPC("Login")]
    public delegate Task<bool> LoginRPC(string username, string password);

    [RPC("GetName")]
    public delegate Task<string> GetNameRPC();

    [RPC("GetTime")]
    public delegate Task<DateTime> GetTimeRPC();
    public class Client
    {
        public NewSocketClient Server;
        public RPCProtocal RPC;
        public AsyncWaitHandle AuthWait = new AsyncWaitHandle();

        public LoginRPC Login;
        public GetNameRPC GetName;
        public GetTimeRPC GetTime;
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

            Login = RPC.GetRPC<LoginRPC>();

            Login = RPC.GetRPC<LoginRPC>("Login");

            GetName = RPC.GetRPC<GetNameRPC>();
            GetTime = RPC.GetRPC<GetTimeRPC>();
            Server.Start();

            ThreadPool.QueueUserWorkItem(async (_) =>
            {
                await Run();
            });
        }
        public async Task Run()
        {
            Console.WriteLine("Logging in...");
            var pass = await Login("Username", "Password");
            if (pass)
            {
                Console.WriteLine("Logged in!");
            }

            Console.WriteLine($"Remote Name: {await GetName()}");
            Console.WriteLine($"Remote Name: {(await GetTime()).ToShortTimeString()}");

            Console.WriteLine("Disconnecting...");
            Server.Disconnect();
            Console.WriteLine("Disconnected.");
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