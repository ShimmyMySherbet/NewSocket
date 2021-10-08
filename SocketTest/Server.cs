using NewSocket.Core;
using NewSocket.Protocals.OTP;
using NewSocket.Protocals.RPC;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest
{
    public class Server
    {
        public BaseSocketClient Client;

        public Server(Stream stream)
        {
            Client = new BaseSocketClient(stream);
            Client.Start();
            Client.Name = "Server";
            Client.RegisterProtocal(new ObjectTransferProtocal()).MessageRecieved += Server_MessageRecieved;
            var rpc = Client.RegisterProtocal<RPCProtocal>(new RPCProtocal(Client));

            ThreadPool.QueueUserWorkItem(async (_) =>
            {
                var sw = new Stopwatch();
                for (int i = 0; i < 10; i++)
                {

                    Console.WriteLine("Sending...");
                    sw.Restart();

                    var resp = await rpc.QueryAsync<string>("GetResponse");
                    sw.Stop();
                    Console.WriteLine($"Response: {resp}");
                    Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms");
                }
            });
        }

        private Task Server_MessageRecieved(string channel, Stream content)
        {
            Console.WriteLine($"[Server] MsgRecieved on channel {channel}, Length: {content.Length}");
            return Task.CompletedTask;
        }
    }
}