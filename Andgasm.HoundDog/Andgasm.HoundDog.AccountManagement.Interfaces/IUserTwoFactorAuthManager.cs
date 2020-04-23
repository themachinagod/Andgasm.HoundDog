using Andgasm.HoundDog.API.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public interface IUserTwoFactorAuthManager
    {
        Task<(AuthenticatorPayloadDTO GeneratedCode, FieldValidationErrorDTO Error)> GenerateAuthenticatorSharedKey(string userid);
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ConfirmAuthenticatorCode(string userid, string token);
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> Enable2FA(string userid, string token);
        Task<(bool Success, IEnumerable<FieldValidationErrorDTO> Errors)> Disable2FA(string userid);
    }
}
