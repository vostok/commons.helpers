﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Vostok.Commons.Helpers.Observable
{
    /// <summary>
    /// A lock-free observable (apart from subscriptions) that never completes and has no caching.
    /// </summary>
    internal class BroadcastObservable<T> : IObservable<T>
    {
        private readonly object observersLock = new object();

        private volatile List<IObserver<T>> observers = new List<IObserver<T>>();

        public bool HasObservers => observers.Count > 0;

        public void Push(T value)
        {
            foreach (var observer in observers)
                observer?.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (observersLock)
            {
                var newObservers = new List<IObserver<T>>(observers.Count + 1);

                newObservers.AddRange(observers);
                newObservers.Add(observer);

                Interlocked.Exchange(ref observers, newObservers);
            }

            return new Subscription(this, observer);
        }

        #region Subscription

        private class Subscription : IDisposable
        {
            private readonly BroadcastObservable<T> observable;
            private readonly IObserver<T> observer;

            public Subscription(BroadcastObservable<T> observable, IObserver<T> observer)
            {
                this.observable = observable;
                this.observer = observer;
            }

            public void Dispose()
            {
                lock (observable.observersLock)
                {
                    var newObservers = new List<IObserver<T>>(observable.observers);

                    newObservers.Remove(observer);

                    Interlocked.Exchange(ref observable.observers, newObservers);
                }
            }
        } 

        #endregion
    }
}
