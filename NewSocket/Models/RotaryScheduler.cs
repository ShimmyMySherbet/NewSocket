using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewSocket.Interfaces;

namespace NewSocket.Models
{
    public class RotaryScheduler<T> : IDisposable, IScheduler<T> where T : class
    {
        public int MaxConcurrent { get; set; } = 5;
        private SemaphoreSlim m_Semaphore = new SemaphoreSlim(0);
        private ConcurrentQueue<T> m_Queue = new ConcurrentQueue<T>();
        private List<T> m_Active = new List<T>();
        private bool m_ActiveStarving = true;
        private int m_Index = 0;

        public async Task<T> GetNext(CancellationToken token)
        {
            bool shouldRetriveNew;

            lock (m_Active)
                shouldRetriveNew = m_ActiveStarving || (!m_Queue.IsEmpty && m_Active.Count < MaxConcurrent);

            if (shouldRetriveNew)
            {
                T? next;
                while (true)
                {
                    await m_Semaphore.WaitAsync(token);
                    if (m_Queue.TryDequeue(out next) && next != null)
                    {
                        break;
                    }
                }

                m_ActiveStarving = false;
                lock (m_Active)
                {
                    m_Active.Add(next);
                }

                return next;
            }
            else
            {
                lock (m_Active)
                {
                    var index = (m_Index + 1) % m_Active.Count;
                    var next = m_Active[index];
                    return next;
                }
            }
        }

        public void Enqueue(T value)
        {
            m_Queue.Enqueue(value);
            m_Semaphore.Release();
        }

        public bool Finalize(T value)
        {
            lock (m_Active)
            {
                if (m_Active.Contains(value))
                {
                    m_Active.Remove(value);
                    m_ActiveStarving = m_Active.Count == 0;
                    return true;
                }
                m_ActiveStarving = m_Active.Count == 0;
            }
            return false;
        }

        public void Dispose()
        {
            m_Semaphore?.Dispose();
        }
    }
}