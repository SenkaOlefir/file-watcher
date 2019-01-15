using System;
using System.Collections.Generic;
using System.Threading;

namespace ReactivePlatform
{
    public abstract partial class Observable<T>
    {
        private struct ObserverItem
        {
            public T Value { get; private set; }
            public Exception Exception { get; private set; }
            public bool IsLastElement { get; private set; }

            public bool HasException => Exception != null;

            public static ObserverItem FromValue(T value) => new ObserverItem { Value = value };
            public static ObserverItem FromException(Exception exception) => new ObserverItem { Exception = exception };
            public static ObserverItem FromLast() => new ObserverItem { IsLastElement = true };
        }

        protected class ObserverWrapper : IDisposable
        {
            private readonly Cancellation _cancellationToken = new Cancellation();
            private readonly Thread _workingThread;
            private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
            private readonly IObserver<T> _observer;
            private readonly Queue<ObserverItem> _executionQueue = new Queue<ObserverItem>();

            private readonly object _lock = new object();

            public ObserverWrapper(IObserver<T> observer)
            {
                _observer = observer;
                _workingThread = new Thread(StartExecution);
                _workingThread.Start();
            }

            public void Dispose()
            {
                _cancellationToken.RequestCancellation(); //request for cancellation
                ForceEnqueueLast();
                _workingThread.Join(); //wait when last executions will be done
                _manualResetEvent.Dispose(); //we can dispose the last element
            }

            public void EnqueueNext(T value) => Enqueue(ObserverItem.FromValue(value));
            public void EnqueueError(Exception exception) => Enqueue(ObserverItem.FromException(exception));
            public void EnqueueLast() => Enqueue(ObserverItem.FromLast());

            private void ForceEnqueueLast()
            {
                lock (_lock)
                {
                    _executionQueue.Clear();
                    EnqueueLast();
                }
            }

            private void Enqueue(ObserverItem item)
            {
                lock (_lock)
                {
                    _executionQueue.Enqueue(item);
                }
                _manualResetEvent.Set();
            }

            private void StartExecution()
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    _manualResetEvent.WaitOne();
                    while (_executionQueue.Count > 0)
                    {
                        ObserverItem item = default;
                        lock (_lock)
                        {
                            item = _executionQueue.Dequeue();
                        }
                        //we can get an exception here if item in queue is bad 
                        //NRE for example
                        if (item.HasException)
                        {
                            _observer.OnError(item.Exception);
                        }
                        else if (!item.IsLastElement)
                        {
                            _observer.OnNext(item.Value);
                        }
                        else
                        {
                            _observer.OnCompleted();
                            //we do nothing, when execution was completed
                            //in theory we can still receive items, but I'm not process it, because this class just a wrapper for background consuming
                        }
                    }
                    _manualResetEvent.Reset();
                }
            }
        }
    }
}