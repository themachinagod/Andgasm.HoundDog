using System.Threading.Tasks;

namespace Andgasm.HoundDog.Core.Email.Interfaces
{
    public interface ISMSVerification
    {
        Task<bool> SendVerificationToPhoneNumber(string phonenumber);
        Task<bool> VerifyPhoneNumber(string phonenumber, string token);
    }
}
