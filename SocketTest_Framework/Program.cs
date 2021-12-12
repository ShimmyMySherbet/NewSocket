using NewSocket.Core;
using NewSocket.Protocals.RPC;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest_Framework
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            var lst = new TcpListener(IPAddress.Loopback, 2222);
            lst.Start();

            var con = new TcpClient();
            con.Connect("127.0.0.1", 2222);
            var cl = lst.AcceptTcpClient();

            Console.WriteLine("All connected!");

            var server = new NewSocketClient(cl.GetStream(), new NewSocket.Models.SocketClientConfig(NewSocket.Models.EClientRole.Server));
            server.AllowQueueBeforeStart = true;
            var client = new NewSocketClient(con.GetStream(), new NewSocket.Models.SocketClientConfig());
            client.AllowQueueBeforeStart = true;
            var s = new Server(server.RPC);
            var c = new Client(client.RPC);

            

            ThreadPool.QueueUserWorkItem(async (_) => await s.RunAsync());
            ThreadPool.QueueUserWorkItem(async (_) => await c.RunAsync());

            Thread.Sleep(1000);

            server.Start();
            client.Start();


            Console.WriteLine("running.");
            Console.ReadLine();
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            var ex = e.Exception;
            Console.WriteLine(ex.StackTrace);
        }
    }

    public class Server
    {
        public RPCProtocal RPC;

        public Server(RPCProtocal rpc)
        {
            RPC = rpc;
            RPC.RegisterFrom(this);
        }

        [RPC]
        public string GetName() => "SRV";

        [RPC]
        public void RunUpdate()
        {
            Console.WriteLine("UpdateRan");
        }


        [RPC]
        public async Task RunUpdate2()
        {

        }




        public async Task RunAsync()
        {
        }
    }

    public class Client
    {
        public RPCProtocal RPC;
        [RPC("GetName")]
        public delegate string GetNameRPC();
        public GetNameRPC GetName;
        [RPC("RunUpdate")]
        public delegate void UpdateRPC();
        public UpdateRPC Update;


        public delegate Task tsk();
        public delegate void vd();
        public Client(RPCProtocal rpc)
        {
            RPC = rpc;
            RPC.RegisterFrom(this);
            GetName = RPC.GetRPC<GetNameRPC>();
            Update = RPC.GetRPC<UpdateRPC>();
            RPC.GetRPC<tsk>("f");
            RPC.GetRPC<vd>("dd");
        }

        public async Task RunAsync()
        {
            var n = GetName();

            Console.WriteLine($"Name: {n}");
            Update();
        }
    }
}