using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.ServerClient
{
    public class NewSocketServerHost
    {
        public TcpListener TcpListener { get; }
        private CancellationTokenSource m_Source;
        public CancellationToken CancellationToken => m_Source.Token;
        private Task? AcceptLoopTask;

        public NewSocketServerHost(IPAddress binding, int port)
        {
            m_Source = new CancellationTokenSource();
            TcpListener = new TcpListener(binding, port);
        }

        public void Start()
        {
            if (CancellationToken.IsCancellationRequested)
            {
                m_Source?.Dispose();
                m_Source = new CancellationTokenSource();
            }

            TcpListener.Start();
            AcceptLoopTask = Task.Run(AcceptClientLoop);
        }

        public void Stop()
        {
            TcpListener.Stop();
            m_Source.Cancel();
        }

        private async Task AcceptClientLoop()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await TcpListener.AcceptTcpClientAsync();
                    ThreadPool.QueueUserWorkItem(async (_) => await HandleNewClient(client));
                }
                catch (SocketException ex)
                {
                    HandleAcceptLoopException(ex);
                }
            }
        }

        protected virtual async Task HandleNewClient(TcpClient client)
        {



        }


        protected virtual void HandleAcceptLoopException(SocketException ex)
        {
        }
    }
}