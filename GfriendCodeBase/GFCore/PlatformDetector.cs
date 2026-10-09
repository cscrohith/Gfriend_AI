using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.GFriend.Core
{
    public static class PlatformDetector
    {
        private static readonly Dictionary<string, PlatformType> _cache =
            new Dictionary<string, PlatformType>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Detects the platform type from a library instance by inspecting its type name.
        /// </summary>
        /// <param name="library">The library to detect the platform for.</param>
        /// <returns>The detected <see cref="PlatformType"/>, or <see cref="PlatformType.Unknown"/> if null.</returns>
        public static PlatformType DetectFromLibrary(Library library)
        {
            if (library == null) return PlatformType.Unknown;

            var typeName = library.GetType().Name;
            return DetectFromTypeName(typeName);
        }

        /// <summary>
        /// Detects the platform type from a type name string, using a cached lookup.
        /// </summary>
        /// <param name="typeName">The type name to match against known platform types.</param>
        /// <returns>The detected <see cref="PlatformType"/>, or <see cref="PlatformType.Unknown"/> if no match is found.</returns>
        public static PlatformType DetectFromTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return PlatformType.Unknown;

            // Check cache
            if (_cache.TryGetValue(typeName, out var cached))
                return cached;

            // Detect platform
            var result = PlatformType.Unknown;
            foreach (PlatformType platform in Enum.GetValues(typeof(PlatformType)))
            {
                if (platform == PlatformType.Unknown) continue;

                if (typeName.IndexOf(platform.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = platform;
                    break;
                }
            }

            // Cache and return
            _cache[typeName] = result;
            return result;
        }

        /// <summary>
        /// Clears the internal platform detection cache.
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }
    }
}
