using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Disposable
{
    [PublicAPI]
    public class CompositeDisposable : IDisposable
    {
        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            this.disposables = disposables;
        }

        public CompositeDisposable(params IDisposable[] disposables)
            : this(disposables as IEnumerable<IDisposable>)
        {
        }

        public void Dispose()
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
        }

        private readonly IEnumerable<IDisposable> disposables;
    }
}