using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Core
{
    public class ExecutionContext : IDisposable
    {
        private readonly Dictionary<PlatformType, DeviceContext> _deviceContexts;
        private readonly object _lock = new object();
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContext"/> class.
        /// </summary>
        public ExecutionContext()
        {
            _deviceContexts = new Dictionary<PlatformType, DeviceContext>();
        }

        /// <summary>
        /// Registers a device under test for a specific platform type.
        /// </summary>
        /// <param name="platform">The platform type to register the device for.</param>
        /// <param name="device">The device under test to register.</param>
        /// <exception cref="ArgumentException">Thrown when platform is Unknown.</exception>
        /// <exception cref="ArgumentNullException">Thrown when device is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a device is already registered for the platform.</exception>
        public void RegisterDevice(PlatformType platform, DeviceUnderTest device)
        {
            if (platform == PlatformType.Unknown)
                throw new ArgumentException("Cannot register device with Unknown platform", nameof(platform));
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            lock (_lock)

            {
                if (_deviceContexts.ContainsKey(platform))
                    throw new InvalidOperationException($"Device already registered for platform {platform}");

                _deviceContexts[platform] = new DeviceContext(device);
            }
        }

        /// <summary>
        /// Gets the device context for the specified platform.
        /// </summary>
        /// <param name="platform">The platform type to look up.</param>
        /// <returns>The <see cref="DeviceContext"/> if found; otherwise, null.</returns>
        public DeviceContext GetDeviceContext(PlatformType platform)
        {
            lock (_lock)
            {
                return _deviceContexts.TryGetValue(platform, out var context) ? context : null;
            }
        }

        /// <summary>
        /// Gets all registered device contexts.
        /// </summary>
        /// <returns>A read-only list of all <see cref="DeviceContext"/> instances.</returns>
        public IReadOnlyList<DeviceContext> GetAllDeviceContexts()
        {
            lock (_lock)
            {
                return _deviceContexts.Values.ToList();
            }
        }

        /// <summary>
        /// Determines whether a device is registered for the specified platform.
        /// </summary>
        /// <param name="platform">The platform type to check.</param>
        /// <returns>True if a device is registered; otherwise, false.</returns>
        public bool HasDevice(PlatformType platform)
        {
            lock (_lock)
            {
                return _deviceContexts.ContainsKey(platform);
            }
        }

        /// <summary>
        /// Disposes the execution context, clearing all registered device contexts.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                foreach (var context in _deviceContexts.Values)
                {
                    context.Clear();
                }
                _deviceContexts.Clear();
                _disposed = true;
            }
        }
    }
}
