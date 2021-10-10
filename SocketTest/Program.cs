using NewSocket.Protocals.RPC;
using NewSocket.Protocals.RPC.Models;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest
{
    internal class Program
    {
        public static Stopwatch Stopwatch = new Stopwatch();

        public class MTRS
        {

            string str;

            public MTRS(string s)
            {
                str = s;
            }

            public override string ToString()
            {
                return str;
            }

        }

        public static async Task<MTRS> Trigger(string in1, int var2, MTRS bse)
        {
            await Task.Delay(3000);
            return new MTRS($"SHITASS {in1}x{var2} + {bse}");
        }

        private static void Main(string[] args)
        {
            Console.WriteLine($"[b] both, [s] server, [c] client");
            var mode = Console.ReadKey();

            Console.WriteLine();

            if (mode.Key == ConsoleKey.S || mode.Key == ConsoleKey.B)
            {
                Console.WriteLine("RunServer");
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

                var listener = new TcpListener(IPAddress.Loopback, 2122);
                listener.Start();
                Task.Run(() => StartConn(listener));
            }
            if (mode.Key == ConsoleKey.C || mode.Key == ConsoleKey.B)
            {
                Console.WriteLine("Client");
                Task.Run(() => Connect());
            }

            Console.WriteLine("Active.");
            Thread.Sleep(-1);
        }

        private static void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Debug.WriteLine($"[First Chance] {e.Exception.Message}");
            Debug.WriteLine($"[First Chance] {e.Exception.Source}");
            Debug.WriteLine($"[First Chance] {e.Exception.StackTrace}");
        }

        private static async Task Connect()
        {
            var cl = new TcpClient();
            await cl.ConnectAsync(IPAddress.Loopback, 2122);

            var c = new Client(cl.GetStream());
        }

        private static async Task StartConn(TcpListener list)
        {
            var cl = await list.AcceptTcpClientAsync();
            new Server(cl.GetStream());
        }
    }
}