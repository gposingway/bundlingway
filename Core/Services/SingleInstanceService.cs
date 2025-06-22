using Bundlingway.Core.Interfaces;
using Serilog;
using System.Threading;

namespace Bundlingway.Core.Services
{
    public class SingleInstanceService : ISingleInstanceService
    {
        private readonly string _mutexName;
        private Mutex? _instanceMutex;

        public SingleInstanceService()
        {
            _mutexName = "BundlingwayMutex";
        }

        public async Task<bool> IsAnotherInstanceRunningAsync()
        {
            try
            {
                using var mutex = new Mutex(false, _mutexName);
                bool acquired = mutex.WaitOne(100, false);
                return !acquired;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to check for existing instance");
                return false;
            }
        }

        public IDisposable AcquireInstanceLock()
        {
            try
            {
                _instanceMutex = new Mutex(true, _mutexName, out bool createdNew);
                
                if (!createdNew)
                {
                    _instanceMutex.Dispose();
                    _instanceMutex = null;
                    throw new InvalidOperationException("Another instance is already running");
                }

                return new MutexReleaser(_instanceMutex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to acquire instance lock");
                throw;
            }
        }

        private class MutexReleaser : IDisposable
        {
            private readonly Mutex _mutex;
            private bool _disposed = false;

            public MutexReleaser(Mutex mutex)
            {
                _mutex = mutex;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    try
                    {
                        _mutex.ReleaseMutex();
                        _mutex.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to release mutex");
                    }
                    finally
                    {
                        _disposed = true;
                    }
                }
            }
        }
    }
}
