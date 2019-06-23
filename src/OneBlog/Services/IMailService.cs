using System.Threading.Tasks;

namespace OneBlog.Services
{
  public interface IMailService
  {
    Task SendMail(string template, string name, string email, string subject, string msg);
  }
}