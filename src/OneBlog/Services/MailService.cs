using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OneBlog.Services
{
  public class MailService : IMailService
  {
    private IConfiguration _conf;
    private IWebHostEnvironment _env;
    private ILogger<MailService> _logger;

    public MailService(IWebHostEnvironment env, IConfiguration conf, ILogger<MailService> logger)
    {
      _env = env;
      _conf = conf;
      _logger = logger;
    }

    public async Task SendMail(string template, string name, string email, string subject, string msg)
    {
      try
      {
        var path = $"{_env.ContentRootPath}\\EmailTemplates\\{template}";
        var body = File.ReadAllText(path);

        var key = _conf["MailService:ApiKey"];

        var uri = $"https://api.sendgrid.com/api/mail.send.json";
        var post = new KeyValuePair<string, string>[]
              {
                new KeyValuePair<string, string>("api_user", _conf["MailService:ApiUser"]),
                new KeyValuePair<string, string>("api_key", _conf["MailService:ApiKey"]),
                new KeyValuePair<string, string>("to", _conf["MailService:Receiver"]),
                new KeyValuePair<string, string>("toname", name),
                new KeyValuePair<string, string>("subject", $"OneBlog Send Mail"),
                new KeyValuePair<string, string>("text", string.Format(body, email, name, subject, msg)),
                new KeyValuePair<string, string>("from", _conf["MailService:Receiver"])
              };

        var client = new HttpClient();
        var response = await client.PostAsync(uri, new FormUrlEncodedContent(post));
        if (!response.IsSuccessStatusCode)
        {
          var result = await response.Content.ReadAsStringAsync();
          _logger.LogError($"Failed to send message via SendGrid: {Environment.NewLine}Body: {post}{Environment.NewLine}Result: {result}");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError("Exception Thrown sending message via SendGrid", ex);
      }
    }
  }
}