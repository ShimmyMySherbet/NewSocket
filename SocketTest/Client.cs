using NewSocket.Core;
using NewSocket.Models;
using NewSocket.Protocals.OTP;
using NewSocket.Protocals.RPC;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest
{
    public class Client
    {
        public BaseSocketClient Server;

        public Client(Stream stream)
        {
            Server = new BaseSocketClient(stream);
            Server.Name = "Client";
            Server.Start();
            Server.RegisterProtocal(new ObjectTransferProtocal()).MessageRecieved += Server_MessageRecieved;
            Server.RegisterProtocal<RPCProtocal>(new RPCProtocal());
        }

        private async Task Server_MessageRecieved(string channel, Stream content)
        {
            Program.Stopwatch.Stop();
            Console.WriteLine($"[Server] MsgRecieved on channel {channel}, Length: {content.Length}");
            content.Position = 0;

            Console.WriteLine($"Took {Program.Stopwatch.ElapsedMilliseconds} ms ({Program.Stopwatch.ElapsedTicks} ticks) (@ {Utils.GetReadableSpeed(Program.Stopwatch.Elapsed, content.Length)})");
        }
    }
}