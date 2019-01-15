using System;

namespace ReactivePlatform
{
    public class ObservableDisposer<T> : IDisposable
    {
        private readonly Observable<T> _observable;

        public ObservableDisposer(Observable<T> observable)
        {
            _observable = observable;
        }

        public void Dispose()
        {
            _observable.StopProducing();
        }
    }

    public class CompositeObservableDisposer<T> : IDisposable
    {
        private readonly BroadcastObservable<T> _observable;

        public CompositeObservableDisposer(BroadcastObservable<T> observable)
        {
            _observable = observable;
        }

        public void Dispose()
        {
            _observable.StopProducing();
            _observable.DisposeAll();
        }
    }
}