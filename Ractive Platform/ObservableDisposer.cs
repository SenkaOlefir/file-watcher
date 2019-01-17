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

        //Is's not correct implemenration of disposable pattern
        //We should use protected disposable method and suppress finalization after disposing
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