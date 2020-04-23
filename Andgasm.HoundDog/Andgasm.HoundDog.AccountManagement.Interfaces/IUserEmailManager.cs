using Andgasm.HoundDog.API.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public interface IUserEmailManager
    {
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ConfirmEmailAddress(string userid, string token);
        Task<(bool Succeeded, FieldValidationErrorDTO Error)> GenerateEmailConfirmation(string userid);
    }
}
