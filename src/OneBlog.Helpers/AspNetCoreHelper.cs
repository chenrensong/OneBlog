using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneBlog.Helpers
{
    public static class AspNetCoreHelper
    {

        public static HttpContext HttpContext
        {
            get
            {
                var accessor = IocContainer.Get<IHttpContextAccessor>();
                return accessor.HttpContext;
            }
        }

        public static ActionContext ActionContext
        {
            get
            {
                var accessor = IocContainer.Get<IActionContextAccessor>(); 
                return accessor.ActionContext;
            }
        }

        public static IUrlHelper UrlHelper
        {
            get
            {

                var urlHelperFactory = IocContainer.Get<IUrlHelperFactory>();
                return urlHelperFactory.GetUrlHelper(ActionContext);
            }
        }


        public static string GetRequestIP(bool tryUseXForwardHeader = true)
        {
            string ip = null;
            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For
            if (tryUseXForwardHeader)
                ip = GetHeaderValueAs<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();
            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (ip.IsNullEmpty() && HttpContext?.Connection?.RemoteIpAddress != null)
                ip = HttpContext.Connection.RemoteIpAddress.ToString();
            if (ip.IsNullEmpty())
                ip = GetHeaderValueAs<string>("REMOTE_ADDR");
            // _httpContextAccessor.HttpContext?.Request?.Host this is the local host.
            if (ip.IsNullEmpty())
                throw new Exception("Unable to determine caller's IP.");
            return ip;
        }

        public static T GetHeaderValueAs<T>(string headerName)
        {
            StringValues values;
            if (HttpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.
                if (!rawValues.IsNullEmpty())
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }

        public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable<string>()
                .Select(s => s.Trim())
                .ToList();
        }

    }
}
