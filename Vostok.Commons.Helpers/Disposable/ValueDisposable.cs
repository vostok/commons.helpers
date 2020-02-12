using System;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Disposable
{
    [PublicAPI]
    internal class ValueDisposable<T> : IDisposable
    {
        public readonly T Value;
        private readonly IDisposable disposable;
        private bool disposed;

        public ValueDisposable(T value, IDisposable disposable)
        {
            Value = value;
            this.disposable = disposable;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                disposable?.Dispose();
            }
        }
    }
}