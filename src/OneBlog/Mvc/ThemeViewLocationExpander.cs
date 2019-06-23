using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using OneBlog.Configuration;
using System;
using System.Collections.Generic;

namespace OneBlog.Mvc
{
    public class ThemeViewLocationExpander : IViewLocationExpander
    {
        private const string ValueKey = "theme";

        private string _theme;

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (viewLocations == null)
            {
                throw new ArgumentNullException(nameof(viewLocations));
            }

            context.Values.TryGetValue(ValueKey, out string theme);

            if (!string.IsNullOrEmpty(theme))
            {
                return ExpandViewLocationsCore(viewLocations, theme);
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(_theme))
            {
                var appSettings = context.ActionContext.HttpContext.RequestServices.GetService(typeof(IOptions<AppSettings>)) as IOptions<AppSettings>;
                _theme = appSettings.Value.Theme;

            }

            context.Values[ValueKey] = _theme;
        }

        private IEnumerable<string> ExpandViewLocationsCore(IEnumerable<string> viewLocations, string theme)
        {
            foreach (var location in viewLocations)
            {
                yield return location;
                yield return location.Insert(7, $"Themes/{theme}/");
            }
        }
    }
}
