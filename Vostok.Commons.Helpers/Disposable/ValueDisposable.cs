using System;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Disposable
{
    [PublicAPI]
    internal class ValueDisposable<T> : IDisposable
    {
        public readonly T Value;
        private readonly IDisposable disposable;

        public ValueDisposable(T value, IDisposable disposable)
        {
            Value = value;
            this.disposable = disposable;
        }

        public void Dispose() =>
            disposable?.Dispose();
    }
}