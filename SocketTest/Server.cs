using NewSocket.Core;
using NewSocket.Protocals.OTP;
using NewSocket.Protocals.RPC;
using System;
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
            Client.RegisterProtocal<RPCProtocal>(new RPCProtocal());

            var msg = Client.GetProtocal<RPCProtocal>().CreateRPCResponse(Client, 696969, "NOP", "GAYASS!");
            Client.Enqueue(msg);

            for (int i = 0; i < 10; i++)
            {
                Client.OTPSend($"Channel{i}", $"Hello x {i}!");
            }
        }

        private Task Server_MessageRecieved(string channel, Stream content)
        {
            Console.WriteLine($"[Server] MsgRecieved on channel {channel}, Length: {content.Length}");
            return Task.CompletedTask;
        }
    }
}