using System;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Disposable
{
    [PublicAPI]
    internal class ActionDisposable : IDisposable
    {
        private readonly Action dispose;

        public ActionDisposable(Action dispose) =>
            this.dispose = dispose;

        public void Dispose() =>
            dispose();
    }
}