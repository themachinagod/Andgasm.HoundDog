using System.Threading.Tasks;

namespace Andgasm.HoundDog.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string from, string to, string subject, string body);
    }
}
