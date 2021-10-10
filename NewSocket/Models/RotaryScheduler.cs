using NewSocket.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Models
{
    public class RotaryScheduler<T> : IDisposable, IScheduler<T> where T : class
    {
        public int MaxConcurrent { get; set; } = 5;
        private SemaphoreSlim m_Semaphore = new SemaphoreSlim(0);
        private ConcurrentQueue<T> m_Queue = new ConcurrentQueue<T>();

        private List<T> m_ActiveMessages = new List<T>();
        private int m_ActiveMessageCount = 0;
        private int m_Index = 0;


        public void Enqueue(T value)
        {
            m_Queue.Enqueue(value);
            m_Semaphore.Release();
        }

        private async Task<T?> DequeueNext(CancellationToken token)
        {
            await m_Semaphore.WaitAsync(token);
            token.ThrowIfCancellationRequested();
            if (m_Queue.TryDequeue(out var up))
            {
                return up;
            }
            return null;
        }

        public async Task<T> GetNext(CancellationToken token)
        {
            m_Index = m_Index + 1 % MaxConcurrent;

            if (m_Index > m_ActiveMessageCount && (m_Queue.Count > 0 || m_ActiveMessageCount == 0))
            {
                T? next = null;
                while (next == null)
                {
                    next = await DequeueNext(token);
                }

                lock (m_ActiveMessages)
                    m_ActiveMessages.Add(next);
                m_ActiveMessageCount++;
                return next;
            } else
            {
                lock (m_ActiveMessages)
                    return m_ActiveMessages[m_Index % m_ActiveMessageCount];
            }
        }

        public bool Finalize(T value)
        {
            lock(m_ActiveMessages)
            {
                if (m_ActiveMessages.Contains(value))
                {
                    m_ActiveMessages.Remove(value);
                    m_ActiveMessageCount = m_ActiveMessages.Count;
                    if (value is IDisposable dispose)
                        dispose.Dispose();
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            m_Semaphore?.Dispose();
        }
    }
}
