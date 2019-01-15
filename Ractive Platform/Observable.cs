using System;
using System.Threading;

namespace ReactivePlatform
{
    public abstract partial class Observable<T> : IObservable<T>
    {
        protected ObservableState State = ObservableState.Waiting;
        protected Thread WorkingThread;

        public IDisposable Subscribe(Action<T> onNext)
        {
            return Subscribe(Observer<T>.Create(onNext));
        }

        public abstract IDisposable Subscribe(IObserver<T> observer);

        protected enum ObservableState
        {
            Waiting = 1,
            Producing = 2,
            Stopping = 3,
            Disposed = 4
        }

        internal void StopProducing()
        {
            State = ObservableState.Stopping;
            if (WorkingThread.IsAlive)
                WorkingThread.Join();
        }
    }
}