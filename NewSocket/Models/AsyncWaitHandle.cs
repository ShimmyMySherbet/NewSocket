using System;
using System.Threading;
using System.Threading.Tasks;

namespace NewSocket.Models
{
    // Wrapper for TaskCompletionSource
    public class AsyncWaitHandle<T>
    {
        public T Result { get; private set; }
        public bool Complete { get; private set; }
        public Exception Exception { get; private set; }

        /// <summary>
        /// Cancels on completion of task, or on task cancellation.
        /// </summary>
        public CancellationToken Token => m_TokenSource.Token;

        private TaskCompletionSource<T> m_Task = new TaskCompletionSource<T>();
        private CancellationTokenSource m_TokenSource = new CancellationTokenSource();

        public AsyncWaitHandle()
        {
            m_Task.Task.ContinueWith((Task) =>
            {
                m_TokenSource.Cancel();
                Result = Task.Result;
                Complete = true;
                if (Task.IsFaulted)
                {
                    Exception = Task.Exception.GetBaseException();
                }
            });
        }

        public async Task<T> WaitAsync()
        {
            return await m_Task.Task;
        }

        public void Wait(CancellationToken token)
        {
            SpinWait.SpinUntil(() => m_Task.Task.IsCompleted || token.IsCancellationRequested);
        }

        public void Release(T result)
        {
            m_Task.TrySetResult(result);
        }

        public void ReleaseException(Exception ex)
        {
            m_Task.SetException(ex);
        }

        public void Cancel()
        {
            m_Task.SetCanceled();
            m_TokenSource.Cancel();
        }
    }

    public class AsyncWaitHandle
    {
        public bool Complete { get; private set; }
        public Exception Exception { get; private set; }

        /// <summary>
        /// Cancels on completion of task, or on task cancellation.
        /// </summary>
        public CancellationToken Token => m_TokenSource.Token;

        private TaskCompletionSource<object> m_Task = new TaskCompletionSource<object>();
        private CancellationTokenSource m_TokenSource = new CancellationTokenSource();

        public AsyncWaitHandle()
        {
            m_Task.Task.ContinueWith((Task) =>
            {
                m_TokenSource.Cancel();
                Complete = true;
                if (Task.IsFaulted)
                {
                    Exception = Task.Exception.GetBaseException();
                }
            });
        }

        public async Task WaitAsync()
        {
            await m_Task.Task;
        }

        public void Wait(CancellationToken token)
        {
            SpinWait.SpinUntil(() => m_Task.Task.IsCompleted || token.IsCancellationRequested);
        }

        public void Release()
        {
            m_Task.TrySetResult(null);
        }

        public void ReleaseException(Exception ex)
        {
            m_Task.SetException(ex);
        }

        public void Cancel()
        {
            m_Task.SetCanceled();
            m_TokenSource.Cancel();
        }
    }
}