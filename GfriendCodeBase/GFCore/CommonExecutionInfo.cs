using System;
using System.Collections.Generic;
using System.Linq;

namespace HP.GFriend.Core
{
    public static class CommonExecutionInfo
    {
        public static int CurrentRepeatCount { get; set; }
        public static bool IsHeadlessMode { get; set; } = false;
        public static bool SendMailwithReportSummary { get; set; } = false;
        public static string ScriptFolder { get; set; }

        [ThreadStatic]
        private static ExecutionContext _currentContext;

        [ThreadStatic]
        private static PlatformType _currentPlatform;

        private static Dictionary<string, string> _variables;
        private static Dictionary<string, object> _sharedObjects;
        private static Dictionary<string, object> _remoteExecutors;
        private static Dictionary<string, List<object>> _forLoopItems;

        public static void Initialize()
        {
            _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _sharedObjects = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _remoteExecutors = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _forLoopItems = new Dictionary<string, List<object>>(StringComparer.OrdinalIgnoreCase);
        }

        internal static void SetExecutionContext(ExecutionContext context, PlatformType platform)
        {
            _currentContext = context;
            _currentPlatform = platform;
        }

        internal static void ClearExecutionContext()
        {
            _currentContext = null;
            _currentPlatform = PlatformType.Unknown;
        }

        public static string GetVariable(string name)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    return deviceContext.GetVariable(name);
                }
            }

            EnsureInitialized();
            return _variables.ContainsKey(name) ? _variables[name] : string.Empty;
        }

        public static void SetVariable(string name, string value)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    deviceContext.SetVariable(name, value);
                    return;
                }
            }

            EnsureInitialized();
            _variables[name] = value;
        }

        public static bool HasVariable(string name)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    return deviceContext.HasVariable(name);
                }
            }

            EnsureInitialized();
            return _variables.ContainsKey(name);
        }

        public static bool VariableExists(string name)
        {
            return HasVariable(name);
        }

        public static void RemoveVariable(string name)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    deviceContext.RemoveVariable(name);
                    return;
                }
            }

            EnsureInitialized();
            _variables.Remove(name);
        }

        public static void SetSharedObject(string key, object obj)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    deviceContext.SetSharedObject(key, obj);
                    return;
                }
            }

            EnsureInitialized();
            _sharedObjects[key] = obj;
        }

        public static object GetSharedObject(string key)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    return deviceContext.GetSharedObject(key);
                }
            }

            EnsureInitialized();
            return _sharedObjects.ContainsKey(key) ? _sharedObjects[key] : null;
        }

        public static bool SharedObjectExists(string key)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    return deviceContext.GetSharedObject(key) != null;
                }
            }

            EnsureInitialized();
            return _sharedObjects.ContainsKey(key);
        }

        public static void RemoveSharedObject(string key)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    deviceContext.RemoveSharedObject(key);
                    return;
                }
            }

            EnsureInitialized();
            if (_sharedObjects.ContainsKey(key))
            {
                _sharedObjects.Remove(key);
            }
        }

        public static void SetRemoteExecutor(string remoteId, object obj)
        {
            EnsureInitialized();
            _remoteExecutors[remoteId] = obj;
        }

        public static object GetRemoteExecutor(string remoteId)
        {
            EnsureInitialized();
            return _remoteExecutors.ContainsKey(remoteId) ? _remoteExecutors[remoteId] : null;
        }

        public static string GetVariableByValue(string value)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    return deviceContext.GetVariableByValue(value);
                }
            }

            EnsureInitialized();
            return _variables.FirstOrDefault(x => x.Value == value).Key;
        }

        public static List<object> GetForLoopItems(string loopName)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    return deviceContext.GetForLoopItems(loopName);
                }
            }

            EnsureInitialized();
            return _forLoopItems.ContainsKey(loopName) ? _forLoopItems[loopName] : null;
        }

        public static void SetForLoopItems(string loopName, List<object> items)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    deviceContext.SetForLoopItems(loopName, items);
                    return;
                }
            }

            EnsureInitialized();
            _forLoopItems[loopName] = items;
        }

        public static bool HasForLoopItems(string loopName)
        {
            List<object> items = GetForLoopItems(loopName);
            return items != null && items.Count > 0;
        }

        public static void RemoveForLoopItems(string loopName)
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    deviceContext.RemoveForLoopItems(loopName);
                    return;
                }
            }

            EnsureInitialized();
            _forLoopItems.Remove(loopName);
        }

        public static void SetforloopItem(string itemId, string value)
        {
            SetForLoopItems(itemId, new List<object> { value });
        }

        public static string GetforloopItem(string itemId)
        {
            List<object> items = GetForLoopItems(itemId);
            if (items != null && items.Count > 0)
            {
                return items[0] == null ? string.Empty : items[0].ToString();
            }

            return string.Empty;
        }

        public static bool HasforloopItem(string itemId)
        {
            return HasForLoopItems(itemId);
        }

        public static int GetVariablesCount()
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    return deviceContext.GetVariablesCount();
                }
            }

            EnsureInitialized();
            return _variables.Count;
        }

        public static void ClearAll()
        {
            if (_currentContext != null)
            {
                DeviceContext deviceContext = _currentContext.GetDeviceContext(_currentPlatform);
                if (deviceContext != null)
                {
                    deviceContext.Clear();
                    return;
                }
            }

            EnsureInitialized();
            _variables.Clear();
            _sharedObjects.Clear();
            _remoteExecutors.Clear();
            _forLoopItems.Clear();
        }

        private static void EnsureInitialized()
        {
            if (_variables == null || _sharedObjects == null || _remoteExecutors == null || _forLoopItems == null)
            {
                Initialize();
            }
        }
    }
}
