using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Interfaces
{
    public interface IScheduler<T>
    {
        Task<T> GetNext(CancellationToken token);

        void Enqueue(T value);

        bool Finalize(T value);
    }
}