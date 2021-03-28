using OneBlog.Data;
using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneBlog.Helpers
{
    public class CommonHelper
    {
        /// <summary>
        /// The pattern.
        /// </summary>
        private const string Pattern = "<head.*<link( [^>]*title=\"{0}\"[^>]*)>.*</head>";

        /// <summary>
        /// The application's relative web root.
        /// </summary>
        private static string applicationRelativeWebRoot;

        /// <summary>
        /// The href regex.
        /// </summary>
        private static readonly Regex HrefRegex = new Regex(
            "href=\"(.*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// The regex between tags.
        /// </summary>
        private static readonly Regex RegexBetweenTags = new Regex(@">\s+", RegexOptions.Compiled);

        /// <summary>
        /// The regex line breaks.
        /// </summary>
        private static readonly Regex RegexLineBreaks = new Regex(@"\n\s+", RegexOptions.Compiled);

        /// <summary>
        /// The regex strip html.
        /// </summary>
        private static readonly Regex RegexStripHtml = new Regex("<[^>]*>", RegexOptions.Compiled);

        public static string RemoveHtmlWhitespace(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            html = RegexBetweenTags.Replace(html, "> ");
            html = RegexLineBreaks.Replace(html, string.Empty);

            return html.Trim();
        }

        public static string StripHtml(string html)
        {
            return String.IsNullOrWhiteSpace(html) ? string.Empty : RegexStripHtml.Replace(html, string.Empty).Trim();
        }

        public static string GetDescription(PostItem post)
        {
            var description = post.Description;
            if (string.IsNullOrEmpty(description))
            {
                description = post.Content;
            }
            description = RemoveHtmlWhitespace(description);
            description = StripHtml(description);
            return description.Length < 280 ? description : description.Substring(0, 280) + "...";
        }
    }
}
