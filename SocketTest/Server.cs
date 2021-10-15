using NewSocket.Core;
using NewSocket.Protocals.RPC;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SocketTest
{
    public class Server
    {
        public NewSocketClient Client;
        public RPCProtocal RPC;

        public Server(Stream stream)
        {
            Client = new NewSocketClient(stream, new NewSocket.Models.SocketClientConfig()
            {
                RPCEnabled = true,
                Role = NewSocket.Models.EClientRole.Server,
                PartialSocket = false
            });
            Client.MessageIDAssigner.AssignID();
            Client.MessageIDAssigner.AssignID();
            Client.MessageIDAssigner.AssignID();
            Client.MessageIDAssigner.AssignID();
            Client.MessageIDAssigner.AssignID();

            Client.Name = "Server";

            if (Client.RPC == null)
            {
                throw new InvalidOperationException();
            }
            RPC = Client.RPC;
            Client.RPC.RegisterFrom(this);
            Client.Start();
        }

        [RPC]
        public async Task DoSomething()
        {
            await Task.Delay(10);
        }


        [RPC]
        public async Task<bool> Login(AuthValues values)
        {
            await RPC.InvokeAsync("RunSetup");
            return true;
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