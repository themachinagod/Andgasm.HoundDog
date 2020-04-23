using Andgasm.HoundDog.API.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public interface IUserPhoneManager
    {
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ConfirmPhoneNumber(string userid, string token);
        Task<(bool Succeeded, FieldValidationErrorDTO Error)> GeneratePhoneConfirmation(string userid);
    }
}
