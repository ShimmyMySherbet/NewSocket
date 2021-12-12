using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Models;
using NewSocket.Protocals.RPC;

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
            Server.onDisconnect += onDisconnect;
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

        private void onDisconnect(DisconnectContext context)
        {
            Console.WriteLine($"[Client] Disconnected: Expected: {!context.Unexpected}");
            throw new NotImplementedException();
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

            try
            {
                for (int i = 0; i < 100; i++)
                {
                    await RPC.InvokeAsync("H1", DateTime.Now);
                    await RPC.InvokeAsync("H2", DateTime.Now, $"A{i}");
                    await RPC.InvokeAsync("H3", DateTime.Now, $"A{i}", $"X{i}");
                    await RPC.InvokeAsync("H4", DateTime.Now, $"A{i}", $"X{i}", $"2x{i * 2}");
                }
            }
            catch (Exception)
            {
                throw;
            }

            Console.ReadLine();

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