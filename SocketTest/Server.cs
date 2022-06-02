using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Models;
using NewSocket.Protocals.RPC;

namespace SocketTest
{
    public class Server
    {
        public RPCSocketClient Client;

        public delegate void VD();

        public Server(Stream stream)
        {
            Client = new RPCSocketClient(stream);
            Client.RegisterFrom(this);

            Client.onDisconnect += onClientDisconnect;

            Directory.CreateDirectory("Files");

            Task.Run(async () =>
            {
                try
                {
                    await Client.StartAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            });
        }

        [RPC]
        private string[] GetFiles()
        {
            var files = Directory.GetFiles("Files");
            Console.WriteLine($"[SERVER] available files: {files}");
            return files;
        }

        [RPC]
        private ulong GetFile(string path)
        {
            var stream = Client.CreateStream(ENetSyncedMode.ReadWrite);
            ThreadPool.QueueUserWorkItem(async (_) =>
            {
                using (stream)
                using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    await Task.Delay(2000);
                    await file.CopyToAsync(stream);
                    await file.FlushAsync();
                }
            });
            return stream.NetSyncedID;
        }

        private void onClientDisconnect(NewSocket.Models.DisconnectContext context)
        {
            Console.WriteLine($"[Server] Client Disconnected. Expected: {!context.Unexpected}, Faulted: {context.IsFaulted}");
        }

        // Used to test the scheduler to enure proper message rotation

        [RPC("H1")]
        public void H1(DateTime sent)
        {
            Console.WriteLine($"[Server][H1][{Math.Round(DateTime.Now.Subtract(sent).Ticks / 10000d, 2)}ms ago]");
        }

        [RPC("H2")]
        public void H2(DateTime sent, string a)
        {
            Console.WriteLine($"[Server][H1][{Math.Round(DateTime.Now.Subtract(sent).Ticks / 10000d, 2)}ms ago] {a}");
        }

        [RPC("H3")]
        public void H3(DateTime sent, string a, string b)
        {
            Console.WriteLine($"[Server][H1][{Math.Round(DateTime.Now.Subtract(sent).Ticks / 10000d, 2)}ms ago] {a}, {b}");
        }

        [RPC("H4")]
        public void H4(DateTime sent, string a, string b, string c)
        {
            Console.WriteLine($"[Server][H1][{Math.Round(DateTime.Now.Subtract(sent).Ticks / 10000d, 2)}ms ago] {a}, {b}, {c}");
        }

        [RPC]
        public async Task DoSomething()
        {
            await Task.Delay(10);
        }

        [RPC]
        public async Task<bool> Login(string username, string password)
        {
            // best security ever
            await Task.Delay(100);
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }

        [RPC]
        public string GetName()
        {
            Console.WriteLine($"[Server] Client requested name");
            return "ServerAss";
        }

        [RPC]
        public DateTime GetTime() => DateTime.Now;
    }
}