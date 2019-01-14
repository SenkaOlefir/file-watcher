using System;
using System.Diagnostics;

namespace RactivePlatform
{
    [DebuggerStepThrough]
    public class Observer<T> : IObserver<T>
    {
        private readonly Action<T> onNext;
        private readonly Action<Exception> onError;
        private readonly Action onCompleted;

        internal Observer(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onCompleted = onCompleted;
        }

        public static IObserver<T> Create(Action<T> onNext)
           => new Observer<T>(onNext, null, null);

        public static IObserver<T> Create(Action<T> onNext, Action<Exception> onError, Action onCompleted)
           => new Observer<T>(onNext, onError, onCompleted);

        public void OnCompleted() => onCompleted?.Invoke();

        public void OnError(Exception error) => onError?.Invoke(error);

        public void OnNext(T value) => onNext?.Invoke(value);
    }
}