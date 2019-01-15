using System;
using System.Diagnostics;

namespace ReactivePlatform
{
    [DebuggerStepThrough]
    public class Observer<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        internal Observer(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public static IObserver<T> Create(Action<T> onNext)
           => new Observer<T>(onNext, null, null);

        public static IObserver<T> Create(Action<T> onNext, Action<Exception> onError, Action onCompleted)
           => new Observer<T>(onNext, onError, onCompleted);

        public void OnCompleted() => _onCompleted?.Invoke();

        public void OnError(Exception error) => _onError?.Invoke(error);

        public void OnNext(T value) => _onNext?.Invoke(value);
    }
}