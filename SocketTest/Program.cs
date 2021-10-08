using NewSocket.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SocketTest
{
    class Program
    {
        public static Stopwatch Stopwatch = new Stopwatch();
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

            var listener = new TcpListener(IPAddress.Loopback, 2122);
            listener.Start();


            Task.Run(() => StartConn(listener));
            Task.Run(() => Connect());


            Thread.Sleep(-1);
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Debug.WriteLine($"[First Chance] {e.Exception.Message}");
            Debug.WriteLine($"[First Chance] {e.Exception.Source}");
            Debug.WriteLine($"[First Chance] {e.Exception.StackTrace}");
            throw e.Exception;
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
