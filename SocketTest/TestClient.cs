using NewSocket.Protocals.RPC;
using System;
using System.Threading.Tasks;

namespace SocketTest
{
    public class TestClient
    {
        public delegate Task<DateTime> GetTimeArgs();

        public delegate Task<bool> LoginArgs(string username, string password);

        public GetTimeArgs GetTime { get; }
        public LoginArgs Login { get; }

        public TestClient(RPCProtocal RPC)
        {
            GetTime = RPC.GetRPC<GetTimeArgs>("GetTime");
            Login = RPC.GetRPC<LoginArgs>("Login");
        }

        public async Task Start()
        {
            var loggedIn = await Login("Username", "Password");
            if (loggedIn)
            {
                Console.WriteLine($"Logged into remote server!");
                var time = await GetTime();
                Console.WriteLine($"Server Time: {time.ToShortTimeString()}");
            }
            else
            {
                Console.WriteLine("Login Failed.");
            }
        }
    }

    public class TestServer
    {
        public bool ClientLoggedIn { get; private set; } = false;

        public TestServer(RPCProtocal RPC)
        {
            RPC.RegisterFrom(this);
        }

        [RPC("Login")]
        public async Task<bool> LoginUserAsync(string username, string password)
        {
            // verify
            await Task.Delay(100);

            ClientLoggedIn = true;
            return true;
        }

        [RPC]
        public DateTime GetTime()
        {
            return DateTime.Now;
        }
    }
}