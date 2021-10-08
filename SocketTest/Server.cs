using NewSocket.Core;
using NewSocket.Protocals.OTP;
using NewSocket.Protocals.RPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            //var call = rpc.CreateRPCCall("METH", "Argument1", "Argument2");
            //Client.Enqueue(call);

            //ThreadPool.QueueUserWorkItem(async (_) =>
            //{
            //    while (true)
            //    {
            //        var sw = new Stopwatch();
            //        sw.Start();
            //        var name = await rpc.QueryAsync<string>("GetName");
            //        sw.Stop();
            //        Console.WriteLine($"Name: {name}, took {sw.ElapsedMilliseconds}ms");
            //        Console.ReadLine();
            //        int i1 = 10;
            //        int i2 = 32;
            //        sw.Restart();
            //        var res = await rpc.QueryAsync<int>("Multiply", i1, i2);
            //        sw.Stop();
            //        Console.WriteLine($"Mult Res: {res}, took {sw.ElapsedMilliseconds}");
            //        Console.ReadLine();
            //    }
            //});

            ThreadPool.QueueUserWorkItem(async (_) =>
            {
                var c = new List<long>();
                var sw = new Stopwatch();
                var i = 0;
                while (true)
                {
                    i++;

                    sw.Restart();
                    var name = await rpc.QueryAsync<string>("GetName");
                    sw.Stop();

                    c.Add(sw.ElapsedTicks);


                    int i1 = 12 * i;
                    int i2 = 32 * (i / 2);


                    sw.Restart();
                    var res = await rpc.QueryAsync<int>("Multiply", i1, i2);
                    sw.Stop();

                    c.Add(sw.ElapsedTicks);

                    if (i % 1000 == 0)
                    {
                        var avg = c.Average();
                        Console.WriteLine($"Averate Duration: {avg / 10000}ms ({avg} ticks)");
                        c.Clear();
                    }
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