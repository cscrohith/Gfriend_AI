using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Core
{

    /// <summary>
    /// Base disposable class for common resource cleanup handling.
    /// </summary>
        public abstract class DisposableBase : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                {
                    DisposeManagedResources();
                }

                _disposed = true;
            }

            protected abstract void DisposeManagedResources();
        }
    
}

