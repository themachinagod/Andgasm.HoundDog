using Andgasm.HoundDog.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public interface IUserPasswordManager
    {
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ChangePassword(string userid, string oldpassword, string newpassword, string verifycode);
        Task<(bool Succeeded, FieldValidationErrorDTO Error)> GeneratePasswordReset(string username, string verifycode, string email);
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ResetPassword(Guid userid, string username, string password, string resettoken, string verifycode);
    }
}
