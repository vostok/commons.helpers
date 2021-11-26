using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Observable
{
    /// <summary>
    /// <list type="bullet">
    ///     <listheader>
    ///         <term>This class has a simple 4-state machine:</term>
    ///     </listheader>
    ///     <item>
    ///         <term>1. Initial state</term>
    ///         <description>value = null, error = null, completed = false</description>
    ///     </item>
    ///     <item>
    ///         <term>2. State with value</term>
    ///         <description>value = value, error = null, completed = false</description>
    ///     </item>
    ///     <item>
    ///         <term>3. Completed</term>
    ///         <description>value = any, error = null, completed = true</description>
    ///     </item>
    ///     <item>
    ///         <term>4. CompletedWithError</term>
    ///         <description>value = any, error = Err,  completed = true</description>
    ///     </item>
    /// </list>
    /// <list type="bullet">
    ///     <listheader>
    ///         <term>Allowed transitions are:</term>
    ///     </listheader>
    ///     <item><term>1 - initial</term></item>
    ///     <item><term>1 -> 2; (obtaining the first value); 1 -> 3;     1 -> 4;</term></item>
    ///     <item><term>2 -> 2; (with another value);   2 -> 3;     2 -> 4;</term></item>
    ///     <item><term>3, 4 - terminal</term></item>
    /// </list>
    /// 
    /// All state changes occur under one lock. Reading without locks. Read safety is achieved by the order of reads and writes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    internal class LockFreeReadingCachingObservable<T> : ICachingObservable<T>
        where T : class
    {
        private const int Started = 1;
        private const int Completed = 2;
        private const int ErrorOccurred = 4;

        /// <summary>
        /// This collection is readonly and always exists as a single object (tied to this class instance).
        /// Se we can use it as "lock" object.
        /// </summary>
        private readonly List<IObserver<T>> observers = new List<IObserver<T>>();

        private volatile T value;
        private volatile int state;
        private volatile Exception savedError;

        public LockFreeReadingCachingObservable()
        {
        }

        public LockFreeReadingCachingObservable(T initialValue)
        {
            value = initialValue;
            state = Started;
        }

        public T Get()
        {
            var cachedVersion = state;

            if (!IsStarted(cachedVersion))
                throw new InvalidOperationException("Observable has not value.");
            if (HasError(cachedVersion))
                throw savedError;

            var cachedValue = value;

            //А есть ли смысл в даблчеке? Как будто, даже если кто-то успел зайти в OnError и уже начать проставлять savedError, нам-то какое дело, если мы уже взяли State?
            //Мы, в теории, давно уже могли и выйти из этого метода с захваченным State. И никаких порядков мы не нарушили.
            cachedVersion = state;
            if (HasError(cachedVersion))
                throw savedError;

            return cachedValue;
        }

        public T GetOrDefault()
        {
            var cachedVersion = state;

            if (!IsStarted(cachedVersion) || HasError(cachedVersion))
                return default;

            var cachedValue = value;

            //А есть ли смысл в даблчеке? Как будто, даже если кто-то успел зайти в OnError и уже начать проставлять savedError, нам-то какое дело, если мы уже взяли State?
            //Мы, в теории, давно уже могли и выйти из этого метода с захваченным State. И никаких порядков мы не нарушили.
            cachedVersion = state;
            if (!IsStarted(cachedVersion) || HasError(cachedVersion))
                return default;

            return cachedValue;
        }

        public void Next([CanBeNull] T nextValue)
        {
            lock (observers)
            {
                var cachedState = state;
                if (IsCompleted(cachedState))
                    return;

                value = nextValue;
                if (!IsStarted(cachedState))
                    Interlocked.Exchange(ref state, state | Started);

                foreach (var observer in observers)
                    try
                    {
                        observer.OnNext(nextValue);
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

            lock (observers)
            {
                if (IsCompleted(state))
                    return;

                savedError = error;
                Interlocked.Exchange(ref state, state | Completed | ErrorOccurred);

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
            lock (observers)
            {
                if (IsCompleted(state))
                    return;

                Interlocked.Exchange(ref state, state | Completed);

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

        public void Complete([CanBeNull] T lastValue)
        {
            lock (observers)
            {
                Next(lastValue);
                Complete();
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (observers)
            {
                if (savedError != null)
                {
                    observer.OnError(savedError);
                    return new EmptyDisposable();
                }

                var cachedState = state;
                if (IsStarted(cachedState))
                    observer.OnNext(value);

                if (IsCompleted(cachedState))
                {
                    observer.OnCompleted();
                    return new EmptyDisposable();
                }

                observers.Add(observer);
            }

            return new Subscription(this, observer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasError(int cachedState) => (cachedState & ErrorOccurred) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsStarted(int cachedState) => (cachedState & Started) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsCompleted(int cachedState) => (cachedState & Completed) != 0;

        #region Subscription

        private class Subscription : IDisposable
        {
            private readonly LockFreeReadingCachingObservable<T> observable;
            private readonly IObserver<T> observer;

            public Subscription(LockFreeReadingCachingObservable<T> observable, IObserver<T> observer)
            {
                this.observable = observable;
                this.observer = observer;
            }

            public void Dispose()
            {
                lock (observable.observers)
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