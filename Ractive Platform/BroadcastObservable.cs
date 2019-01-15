using System;
using System.Collections.Generic;

namespace ReactivePlatform
{
    public interface IBroadcastObservable<T> : IObservable<T>
    {
        void Publish();
    }

    public abstract class BroadcastObservable<T> : Observable<T>, IBroadcastObservable<T>
    {
        protected IList<ObserverWrapper> Subscribers = new List<ObserverWrapper>();

        public abstract void Publish();

        //closure here that will cause of additional allocation in heap
        //we need use foreach loop to avoid this
        //ForEach extension added for convenience!!!
        protected void EnqueueNext(T item) => Subscribers.ForEach(x => x.EnqueueNext(item));

        protected void EnqueueError(Exception exception) => Subscribers.ForEach(x => x.EnqueueError(exception));

        protected void EnqueueLast() => Subscribers.ForEach(x => x.EnqueueLast());

        internal void DisposeAll() => Subscribers.ForEach(x => x.Dispose());
    }
}