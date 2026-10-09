using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HP.GFriend.Keywords;

namespace HP.GFriend.Core
{
    /// <summary>
    /// Represents an isolated execution context for a single device.
    /// </summary>
    /// <remarks>
    /// <!-- DeviceContext encapsulates all per-device execution state -->
    /// <!-- including variables, shared objects, and for-loop items -->
    /// <!-- All operations are thread-safe using ConcurrentDictionary -->
    /// <!-- Keys for variables are case-insensitive -->
    /// </remarks>
    public class DeviceContext
    {
        #region Fields

        // <!-- Thread-safe dictionaries for isolated device state -->
        private readonly ConcurrentDictionary<string, string> _variables;
        private readonly ConcurrentDictionary<string, object> _sharedObjects;
        private readonly ConcurrentDictionary<string, List<object>> _forLoopItems;
        private readonly object _stateLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the physical device under test.
        /// </summary>
        /// <!-- The DeviceUnderTest represents the physical target device -->
        public DeviceUnderTest DeviceUnderTest { get; set; }

        /// <summary>
        /// Gets or sets the current execution state.
        /// </summary>
        /// <!-- ExecutionState tracks whether the context is idle, running, paused, etc. -->
        public ExecutionState CurrentExecutionState { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceContext"/> class.
        /// </summary>
        /// <!-- Default constructor initializes all dictionaries with case-insensitive keys for variables -->
        public DeviceContext()
        {
            _variables = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _sharedObjects = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _forLoopItems = new ConcurrentDictionary<string, List<object>>(StringComparer.OrdinalIgnoreCase);
            CurrentExecutionState = ExecutionState.Idle;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceContext"/> class with a specified device.
        /// </summary>
        /// <param name="device">The device under test.</param>
        /// <!-- Constructor that accepts a DeviceUnderTest for immediate binding -->
        public DeviceContext(DeviceUnderTest device) : this()
        {
            DeviceUnderTest = device;
        }

        #endregion

        #region Variable Management

        /// <summary>
        /// Sets a variable value by key (case-insensitive).
        /// </summary>
        /// <!-- Variables are stored with case-insensitive keys for flexible lookup -->
        public void SetVariable(string key, string value)
        {
            _variables[key] = value;
        }

        /// <summary>
        /// Gets a variable value by key (case-insensitive).
        /// </summary>
        /// <!-- Returns empty string if the variable does not exist -->
        public string GetVariable(string key)
        {
            if (_variables.TryGetValue(key, out string value))
            {
                return value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks whether a variable exists.
        /// </summary>
        /// <!-- Case-insensitive key lookup -->
        public bool HasVariable(string key)
        {
            return _variables.ContainsKey(key);
        }

        /// <summary>
        /// Gets a variable key by value (case-sensitive value match).
        /// </summary>
        public string GetVariableByValue(string value)
        {
            return _variables.FirstOrDefault(x => x.Value == value).Key;
        }

        /// <summary>
        /// Gets the total count of stored variables.
        /// </summary>
        public int GetVariablesCount()
        {
            return _variables.Count;
        }

        /// <summary>
        /// Removes a variable by key.
        /// </summary>
        /// <param name="key">The variable key to remove.</param>
        public void RemoveVariable(string key)
        {
            _variables.TryRemove(key, out _);
        }

        #endregion

        #region Shared Object Management

        /// <summary>
        /// Sets a shared object by key.
        /// </summary>
        /// <!-- Shared objects allow cross-keyword data sharing within a device context -->
        public void SetSharedObject(string key, object obj)
        {
            _sharedObjects[key] = obj;
        }

        /// <summary>
        /// Gets a shared object by key.
        /// </summary>
        /// <!-- Returns null if the shared object does not exist -->
        public object GetSharedObject(string key)
        {
            if (_sharedObjects.TryGetValue(key, out object obj))
            {
                return obj;
            }
            return null;
        }

        /// <summary>
        /// Removes a shared object by key.
        /// </summary>
        /// <!-- Thread-safe removal using ConcurrentDictionary -->
        public void RemoveSharedObject(string key)
        {
            _sharedObjects.TryRemove(key, out _);
        }

        #endregion

        #region ForLoop Item Management

        /// <summary>
        /// Sets a for-loop item list by identifier.
        /// </summary>
        public void SetForLoopItems(string loopName, List<object> items)
        {
            if (items == null)
            {
                _forLoopItems.TryRemove(loopName, out _);
                return;
            }

            _forLoopItems[loopName] = items;
        }

        /// <summary>
        /// Gets a for-loop item list by identifier.
        /// </summary>
        public List<object> GetForLoopItems(string loopName)
        {
            if (_forLoopItems.TryGetValue(loopName, out List<object> items))
            {
                return items;
            }
            return null;
        }

        /// <summary>
        /// Checks whether a for-loop item exists.
        /// </summary>
        public bool HasForLoopItems(string loopName)
        {
            return _forLoopItems.ContainsKey(loopName);
        }

        /// <summary>
        /// Removes a for-loop item by identifier.
        /// </summary>
        public void RemoveForLoopItems(string loopName)
        {
            _forLoopItems.TryRemove(loopName, out _);
        }

        // Backward-compatible singular wrappers
        public void SetForLoopItem(string itemId, string[] values)
        {
            if (values == null)
            {
                RemoveForLoopItems(itemId);
                return;
            }

            SetForLoopItems(itemId, values.Cast<object>().ToList());
        }

        public string[] GetForLoopItem(string itemId)
        {
            List<object> items = GetForLoopItems(itemId);
            return items == null ? null : items.Select(i => i == null ? string.Empty : i.ToString()).ToArray();
        }

        public bool HasForLoopItem(string itemId)
        {
            return HasForLoopItems(itemId);
        }

        public void RemoveForLoopItem(string itemId)
        {
            RemoveForLoopItems(itemId);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clears all stored state from this device context.
        /// </summary>
        /// <!-- Clear resets variables, shared objects, for-loop items and execution state -->
        public void Clear()
        {
            lock (_stateLock)
            {
                _variables.Clear();
                _sharedObjects.Clear();
                _forLoopItems.Clear();
                CurrentExecutionState = ExecutionState.Idle;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents the execution state of a device context.
    /// </summary>
    /// <!-- ExecutionState enum defines all possible lifecycle states -->
    public enum ExecutionState
    {
        /// <!-- Device is not executing any test -->
        Idle,
        /// <!-- Device is actively running a test -->
        Running,
        /// <!-- Execution is temporarily paused -->
        Paused,
        /// <!-- Execution completed successfully -->
        Completed,
        /// <!-- Execution encountered an error -->
        Failed
    }
}