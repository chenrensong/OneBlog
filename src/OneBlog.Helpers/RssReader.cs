using OneBlog.RssSyndication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OneBlog.Helpers
{
    public class RssReader
    {
        public async Task<List<RssItem>> GetRssFeed(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Headers["User-Agent"] = "Fiddler";
            var rep = await req.GetResponseAsync();
            var reader = XmlReader.Create(rep.GetResponseStream());
            var doc = XDocument.Load(reader, LoadOptions.None);

            return (from i in doc.Descendants("channel").Elements("item")
                    select new RssItem
                    {
                        Title = i.Element("title").Value,
                        Link = new Uri(i.Element("link").Value),
                        Description = i.Element("description").Value
                    }).ToList();
        }
    }
}
