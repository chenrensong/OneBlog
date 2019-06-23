using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>Represents a class that extends the <see cref="T:System.Web.HttpRequestBase" /> class by adding the ability to determine whether an HTTP request is an AJAX request.</summary>
    public static class AjaxRequestExtensions
    {
        /// <summary>Determines whether the specified HTTP request is an AJAX request.</summary>
        /// <returns>true if the specified HTTP request is an AJAX request; otherwise, false.</returns>
        /// <param name="request">The HTTP request.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="request" /> parameter is null (Nothing in Visual Basic).</exception>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            return (request.Headers != null && request.Headers["X-Requested-With"] == "XMLHttpRequest");
        }
    }
}
