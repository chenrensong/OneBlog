using OneBlog.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneBlog.Core.Services
{
    public class CultureService : ICultureService
    {
        public List<SelectOption> Get()
        {
            var items = new List<SelectOption>();
            items.Add(new SelectOption { OptionName = "Auto", OptionValue = "Auto" });
            items.Add(new SelectOption { OptionName = "English", OptionValue = "en" });
            items.Add(new SelectOption { OptionName = "中文(中国)", OptionValue = "zh-CN", IsSelected = false });
            items.Add(new SelectOption { OptionName = "Auto", OptionValue = "Auto", IsSelected = false });
            return items;
        }
    }
}
