using System;
using System.Collections.Generic;
using System.Text;

namespace OneBlog.Helpers
{
    public class ContentHelper
    {
        public static string GetUniqueSlug(string slug)
        {
            string s = slug.Trim();// WebUtils.RemoveIllegalCharacters(slug.Trim());
            // will do for up to 100 unique post titles
            for (int i = 1; i < 101; i++)
            {
                //if (IsUniqueSlug(s))
                //    break;
                s = string.Format("{0}{1}", slug, i);
            }
            return s;
        }

    }
}
