using System;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Observable
{
    [PublicAPI]
    internal interface ICachingObservable<T> : IObservable<T>
    {
        void Next([CanBeNull] T value);
        void Error([NotNull] Exception error);
        void Complete();
        void Complete([CanBeNull] T value);
    }
}