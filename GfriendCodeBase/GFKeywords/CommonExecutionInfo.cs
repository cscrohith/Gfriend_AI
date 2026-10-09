
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HP.GFriend.Keywords
{
    public static class CommonExecutionInfo
    {
        public static int CurrentRepeatCount { get; set; }
        public static bool IsHeadlessMode { get; set; } = false;
        public static bool SendMailwithReportSummary { get; set; } = false;
        public static string ScriptFolder { get; set; }
        private static ConcurrentDictionary<string, string> _variables;
        private static ConcurrentDictionary<string, object> _sharedObject;
        private static ConcurrentDictionary<string, object> _remoteExecutors;
        private static ConcurrentDictionary<string, string> _forloopItems;
        private static ConcurrentDictionary<string, string> _loopResults;
        private static readonly object _initLock = new object();

        public static void Initialize()
        {
            if (_variables != null && _sharedObject != null && _remoteExecutors != null && _forloopItems != null && _loopResults != null)
            {
                return;
            }

            lock (_initLock)
            {
                if (_variables == null)
                {
                    _variables = new ConcurrentDictionary<string, string>();
                }

                if (_sharedObject == null)
                {
                    _sharedObject = new ConcurrentDictionary<string, object>();
                }

                if (_remoteExecutors == null)
                {
                    _remoteExecutors = new ConcurrentDictionary<string, object>();
                }

                if (_forloopItems == null)
                {
                    _forloopItems = new ConcurrentDictionary<string, string>();
                }

                if (_loopResults == null)
                {
                    _loopResults = new ConcurrentDictionary<string, string>();
                }
            }
        }

        public static void SetVariable (string target, string value)
        {
            if (_variables == null)
            {
                Initialize();
            }
            _variables[target] = value;
        }

        public static string GetVariable(string target)
        {
            if (_variables == null)
            {
                Initialize();
            }
            if(_variables.ContainsKey(target))
            {
                return _variables[target];
            }
            return string.Empty;
        }

        public static bool HasVariable(string target)
        {
            if (_variables == null)
            {
                Initialize();
            }
            return _variables.ContainsKey(target);
        }

        public static void SetSharedObject(string key, object obj)
        {
            if (_sharedObject == null)
            {
                Initialize();
            }
            _sharedObject[key] = obj;
        }

        public static object GetSharedObject(string key)
        {
            if (_sharedObject == null)
            {
                Initialize();
            }

            object value;
            return _sharedObject.TryGetValue(key, out value) ? value : null;
        }

        public static void SetRemoteExecutor(string remoteId, object obj)
        {
            if (_remoteExecutors == null)
            {
                Initialize();
            }
            _remoteExecutors[remoteId] = obj;
        }

        public static object GetRemoteExecutor(string remoteId)
        {
            if (_remoteExecutors == null)
            {
                Initialize();
            }

            object value;
            return _remoteExecutors.TryGetValue(remoteId, out value) ? value : null;
        }
        public static void RemoveSharedObject(string key)
        {
            if (_sharedObject == null)
            {
                Initialize();
            }

            object removed;
            _sharedObject.TryRemove(key, out removed);
        }
        public static string GetVariableByValue(string value)
        {
            if (_variables == null)
            {
                Initialize();
            }
            var myKey = _variables.FirstOrDefault(x => x.Value == value).Key;
            return myKey;
        }
        public static void SetforloopItem(string itemId, string value)
        {
            if (_forloopItems == null)
            {
                Initialize();
            }
            _forloopItems[itemId] = value;
        }

        public static string GetforloopItem(string itemId)
        {
            if (_forloopItems == null)
            {
                Initialize();
            }
            //return _forloopItems[ItemId] ?? null;
            if (_forloopItems.ContainsKey(itemId))
            {
                return _forloopItems[itemId];
            }
            return string.Empty;
        }

        public static bool HasforloopItem(string itemId)
        {
            if (_forloopItems == null)
            {
                Initialize();
            }
            return _forloopItems.ContainsKey(itemId);
        }

        public static int GetVariablesCount()
        {
            if (_variables == null)
            {
                Initialize();
            }
            return _variables.Count;
        }
    }
}
