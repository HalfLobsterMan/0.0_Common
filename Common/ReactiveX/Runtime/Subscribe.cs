#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;

namespace CZToolKit.ReactiveX
{
    class Subscribe<T> : IObserver<T>
    {
        public static readonly Action<T> Ignore = (T t) => { };
        public static readonly Action<Exception> Throw = (ex) => { throw ex; };
        public static readonly Action None = () => { };

        readonly Action<T> onNext;
        readonly Action<Exception> onError;
        readonly Action onCompleted;

        public Subscribe(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onCompleted = onCompleted;
        }

        public Subscribe()
        {
            onNext = Ignore;
            onError = Throw;
            onCompleted = None;
        }

        public Subscribe(Action<T> onNext)
        {
            this.onNext = onNext;
            onError = Throw;
            onCompleted = None;
        }

        public Subscribe(Action<T> onNext, Action<Exception> onError)
        {
            this.onNext = onNext;
            this.onError = onError;
            onCompleted = None;
        }

        public Subscribe(Action<T> onNext, Action onCompleted)
        {
            this.onNext = onNext;
            onError = Throw;
            this.onCompleted = onCompleted;
        }

        public void OnNext(T value)
        {
            onNext?.Invoke(value);
        }

        public void OnError(Exception error)
        {
            onError(error);
        }

        public void OnCompleted()
        {
            onCompleted();
        }
    }
    
    public static partial class Extension
    {
        public static IDisposable Subscribe<T>(this IObservable<T> src)
        {
            return src.Subscribe(new Subscribe<T>());
        }

        public static IDisposable Subscribe<T>(this IObservable<T> src, Action<T> onNext)
        {
            return src.Subscribe(new Subscribe<T>(onNext));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> src, Action<T> onNext, Action<Exception> onError)
        {
            return src.Subscribe(new Subscribe<T>(onNext, onError));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> src, Action<T> onNext, Action onCompleted)
        {
            return src.Subscribe(new Subscribe<T>(onNext, onCompleted));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> src, Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            return src.Subscribe(new Subscribe<T>(onNext, onError, onCompleted));
        }
    }
}