using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Models;
using NewSocket.Protocals.RPC;
using NewSocket.Security.Protocols;

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
        public RPCSocketClient Server;
        public AsyncWaitHandle AuthWait = new AsyncWaitHandle();

        public LoginRPC Login;
        public GetNameRPC GetName;
        public GetTimeRPC GetTime;

        public Client(Stream stream)
        {
            //var sec = new AESPresharedKeyProtocol("pas");

            Server = new RPCSocketClient(stream, RSAProtocol.CreateCertExchange());

            Server.onDisconnect += onDisconnect;
            Server.RegisterFrom(this);

            Login = Server.GetRPC<LoginRPC>();

            Login = Server.GetRPC<LoginRPC>("Login");

            GetName = Server.GetRPC<GetNameRPC>();
            GetTime = Server.GetRPC<GetTimeRPC>();

            ThreadPool.QueueUserWorkItem(async (_) =>
            {
                try
                {
                    await Server.StartAsync();
                    await Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
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
                    await Server.InvokeAsync("H1", DateTime.Now);
                    await Server.InvokeAsync("H2", DateTime.Now, $"A{i}");
                    await Server.InvokeAsync("H3", DateTime.Now, $"A{i}", $"X{i}");
                    await Server.InvokeAsync("H4", DateTime.Now, $"A{i}", $"X{i}", $"2x{i * 2}");
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
            var r = await Server.QueryAsync<string>("GetName");
            Console.WriteLine($"Server Name: {r}");
            AuthWait.Release();
        }
    }
}