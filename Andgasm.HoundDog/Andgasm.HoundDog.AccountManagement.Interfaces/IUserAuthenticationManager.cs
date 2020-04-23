using Andgasm.HoundDog.API.Interfaces;
using System.Threading.Tasks;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public interface IUserAuthenticationManager
    {
        Task<(UserDTO ValidatedUser, FieldValidationErrorDTO Error)> AuthenticateUserCredentials(string username, string password, string verificationCode);
        Task<bool> UsernameRequires2FAVerification(string useridentifier);
    }
}
