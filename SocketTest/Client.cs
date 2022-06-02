using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Core;
using NewSocket.Models;
using NewSocket.Protocals.RPC;

namespace SocketTest
{
    [RPC("Login")]
    public delegate Task<bool> LoginRPC(string username, string password);

    [RPC("GetName")]
    public delegate Task<string> GetNameRPC();

    [RPC("GetTime")]
    public delegate Task<DateTime> GetTimeRPC();

    [RPC("GetFiles")]
    public delegate Task<string[]> GetFilesRPC();

    [RPC("GetFile")]
    public delegate Task<ulong> OpenFileRPC(string path);

    public class Client
    {
        public RPCSocketClient Server;
        public AsyncWaitHandle AuthWait = new AsyncWaitHandle();

        public LoginRPC Login;
        public GetNameRPC GetName;
        public GetTimeRPC GetTime;

        public GetFilesRPC GetFiles;
        public OpenFileRPC OpenFile;

        public Client(Stream stream)
        {
            Server = new RPCSocketClient(stream);

            Server.onDisconnect += onDisconnect;
            Server.RegisterFrom(this);

            Directory.CreateDirectory("Local");
            Login = Server.GetRPC<LoginRPC>();

            Login = Server.GetRPC<LoginRPC>("Login");

            GetName = Server.GetRPC<GetNameRPC>();
            GetTime = Server.GetRPC<GetTimeRPC>();

            GetFiles = Server.GetRPC<GetFilesRPC>();
            OpenFile = Server.GetRPC<OpenFileRPC>();

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
            var files = await GetFiles();

            Console.WriteLine($"[Client] Files available: {files.Length}");

            foreach (var f in files)
            {
                Console.WriteLine($"[Client] {f}");

                var localName = Path.GetFileName(f);

                var localPath = Path.Combine("Local", localName);

                var netID = await OpenFile(f);

                Console.WriteLine("Opening stream...");
                var downloadStream = await Server.GetStreamAsync(netID);
                Console.WriteLine("Downloading file...");
                using (downloadStream)
                using(var lcFile = new FileStream(localPath, FileMode.Create, FileAccess.Write))
                {
                    Console.WriteLine("Transfer block...");
                    var buffer = new byte[1024];

                    while (true)
                    {
                        var c = await downloadStream.ReadAsync(buffer, 0, 1024);
                        
                        if (c == 0)
                        {
                            break;
                        }
                        lcFile.Write(buffer, 0, c);
                    }


                    await lcFile.FlushAsync();
                }
                Console.WriteLine("Done!");
            }
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