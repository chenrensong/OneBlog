using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace OneBlog.Helpers
{
    public class ApplicationHelper
    {
        #region Version()

        /// <summary>
        ///     The version.
        /// </summary>
        private static string version;

        /// <summary>
        /// Returns the OneBlog.NET version information.
        /// </summary>
        /// <value>
        /// The OneBlog.NET major, minor, revision, and build numbers.
        /// </value>
        /// <remarks>
        /// The current version is determined by extracting the build version of the OneBlog.Core assembly.
        /// </remarks>
        /// <returns>
        /// The version.
        /// </returns>
        public static string Version()
        {
            return version ?? (version = Assembly.GetEntryAssembly().GetName().Version.ToString());
        }

        #endregion
    }
}
