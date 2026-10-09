    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace HP.GFriend.Core
    {
        /// <summary>
        /// Represents the supported platform types for the application.
        /// </summary>
        public enum PlatformType
        {
            /// <summary>Unknown or unspecified platform.</summary>
            Unknown = 0,
            /// <summary>Microsoft Windows platform.</summary>
            Windows = 1,
            /// <summary>Apple iOS platform.</summary>
            iOS = 2,
            /// <summary>Apple macOS platform.</summary>
            Mac = 3,
            /// <summary>Android platform.</summary>
            Android = 4,
            /// <summary>Android secondary platform variant.</summary>
            Android2=5,
            /// <summary>Web-based platform.</summary>
            Web = 6,
            /// <summary>Jedi platform.</summary>
            Jedi = 7,
            /// <summary>Dune platform.</summary>
            Dune = 8,
            /// <summary>Ares platform.</summary>
            Ares = 9,
        /// <summary>Sirius platform.</summary>
            Sirius = 10,
    }

    /// <summary>
    /// Provides extension methods and utility functions for <see cref="PlatformType"/>.
    /// </summary>
    public static class PlatformTypeExtensions
        {
            /// <summary>
            /// Converts the platform type to its corresponding library file search pattern.
            /// </summary>
            /// <param name="platform">The platform type to convert.</param>
            /// <returns>A wildcard pattern "GFK.*" for Unknown, or "GFK.{platform}" for a specific platform.</returns>
            public static string ToLibraryPattern(this PlatformType platform)
            {
                return platform == PlatformType.Unknown
                    ? "GFK.*"
                    : $"GFK.{platform}";
            }

            /// <summary>
            /// Parses a string value into the corresponding <see cref="PlatformType"/> enum value.
            /// </summary>
            /// <param name="platform">The string representation of the platform type (case-insensitive).</param>
            /// <returns>The matching <see cref="PlatformType"/> value, or <see cref="PlatformType.Unknown"/> if parsing fails.</returns>
            public static PlatformType FromString(string platform)
            {
                return Enum.TryParse<PlatformType>(platform, true, out var result)
                    ? result
                    : PlatformType.Unknown;
            }
        }

    }
