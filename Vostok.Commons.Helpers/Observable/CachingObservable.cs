using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Observable
{
    [PublicAPI]
    internal class CachingObservable<T> : IObservable<T>
    {
        private readonly List<IObserver<T>> observers = new List<IObserver<T>>();
        private readonly object sync = new object();

        private T savedValue;
        private Exception savedError;
        private bool started;

        public CachingObservable()
        {
        }

        public CachingObservable(T initialValue)
        {
            savedValue = initialValue;
            started = true;
        }

        public bool IsCompleted { get; private set; }

        public T Get()
        {
            lock (sync)
            {
                if (!started)
                    throw new InvalidOperationException("Observable has not value.");
                if (savedError != null)
                    throw savedError;

                return savedValue;
            }
        }

        public T GetOrDefault()
        {
            lock (sync)
            {
                if (!started || savedError != null)
                    return default;

                return savedValue;
            }
        }

        public void Next([CanBeNull] T value)
        {
            lock (sync)
            {
                if (IsCompleted)
                    return;

                savedValue = value;
                started = true;

                foreach (var observer in observers)
                    try
                    {
                        observer.OnNext(value);
                    }
                    catch
                    {
                        // ignored
                    }
            }
        }

        public void Error([NotNull] Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            lock (sync)
            {
                if (IsCompleted)
                    return;

                IsCompleted = true;
                savedError = error;

                foreach (var observer in observers)
                    try
                    {
                        observer.OnError(error);
                    }
                    catch
                    {
                        // ignored
                    }

                observers.Clear();
            }
        }

        public void Complete()
        {
            lock (sync)
            {
                if (IsCompleted)
                    return;

                IsCompleted = true;

                foreach (var observer in observers)
                    try
                    {
                        observer.OnCompleted();
                    }
                    catch
                    {
                        // ignored
                    }

                observers.Clear();
            }
        }

        public void Complete([CanBeNull] T value)
        {
            lock (sync)
            {
                Next(value);
                Complete();
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (sync)
            {
                if (savedError != null)
                {
                    observer.OnError(savedError);
                    return new EmptyDisposable();
                }

                if (started)
                    observer.OnNext(savedValue);

                if (IsCompleted)
                {
                    observer.OnCompleted();
                    return new EmptyDisposable();
                }

                observers.Add(observer);
            }

            return new Subscription(this, observer);
        }

        #region Subscription

        private class Subscription : IDisposable
        {
            private readonly CachingObservable<T> observable;
            private readonly IObserver<T> observer;

            public Subscription(CachingObservable<T> observable, IObserver<T> observer)
            {
                this.observable = observable;
                this.observer = observer;
            }

            public void Dispose()
            {
                lock (observable.sync)
                {
                    observable.observers.Remove(observer);
                }
            }
        }

        #endregion

        #region EmptyDisposable

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        #endregion
    }
}