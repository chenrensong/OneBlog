using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace OneBlog.Mvc
{
    /// <summary>
    /// An authorization filter that confirms requests are received over HTTPS.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireOnlyDoaminAttribute : Attribute, IAuthorizationFilter, IFilterMetadata, IOrderedFilter
    {
        private bool? _permanent;

        /// <summary>
        /// Specifies whether a permanent redirect, <c>301 Moved Permanently</c>,
        /// should be used instead of a temporary redirect, <c>302 Found</c>.
        /// </summary>
        public bool Permanent
        {
            get
            {
                return _permanent ?? false;
            }
            set
            {
                _permanent = value;
            }
        }

        public string Host { get; set; }

        public int Order { get; set; }

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized. Confirms requests are received over
        /// HTTPS. Takes no action for HTTPS requests. Otherwise if it was a GET request, sets
        /// <see cref="P:Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext.Result" /> to a result which will redirect the client to the HTTPS
        /// version of the request URI. Otherwise, sets <see cref="P:Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext.Result" /> to a result
        /// which will set the status code to <c>403</c> (Forbidden).
        /// </summary>
        /// <inheritdoc />
        public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            //域名相等
            HttpRequest request = filterContext.HttpContext.Request;
            if (!string.Equals(request.Host.Host, this.Host, StringComparison.OrdinalIgnoreCase) ||
                !filterContext.HttpContext.Request.IsHttps)
            {
                HandleRequest(filterContext);
            }

        }

        /// <summary>
        /// Called from <see cref="M:Microsoft.AspNetCore.Mvc.RequireHttpsAttribute.OnAuthorization(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)" /> if the request is not received over HTTPS. Expectation is
        /// <see cref="P:Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext.Result" /> will not be <c>null</c> after this method returns.
        /// </summary>
        /// <param name="filterContext">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext" /> to update.</param>
        /// <remarks>
        /// If it was a GET request, default implementation sets <see cref="P:Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext.Result" /> to a
        /// result which will redirect the client to the HTTPS version of the request URI. Otherwise, default
        /// implementation sets <see cref="P:Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext.Result" /> to a result which will set the status
        /// code to <c>403</c> (Forbidden).
        /// </remarks>
        protected virtual void HandleRequest(AuthorizationFilterContext filterContext)
        {
            if (!string.Equals(filterContext.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = (new StatusCodeResult(403));
            }
            else
            {
                HttpRequest request = filterContext.HttpContext.Request;
                HostString host = default(HostString);// request.Host;
                string hostString = this.Host;
                IOptions<MvcOptions> requiredService = ServiceProviderServiceExtensions.GetRequiredService<IOptions<MvcOptions>>(filterContext.HttpContext.RequestServices);
                if (requiredService.Value.SslPort.HasValue && requiredService.Value.SslPort > 0)
                {
                    host = new HostString(hostString, requiredService.Value.SslPort.Value);
                }
                else
                {
                    host = new HostString(hostString);
                }
                bool permanent = _permanent ?? requiredService.Value.RequireHttpsPermanent;
                string[] obj = new string[5]
                {
                    "https://",
                    host.ToUriComponent(),
                    request.PathBase.ToUriComponent(),
                    request.Path.ToUriComponent(),
                    request.QueryString.ToUriComponent()
                };
                string url = string.Concat(obj);
                filterContext.Result = (new RedirectResult(url, permanent));
            }
        }
    }

}
