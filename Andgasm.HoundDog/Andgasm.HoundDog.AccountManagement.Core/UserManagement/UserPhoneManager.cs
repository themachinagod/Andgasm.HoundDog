using Andgasm.HoundDog.AccountManagement.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;
using Andgasm.HoundDog.API.Interfaces;
using Andgasm.HoundDog.Core.Email.Interfaces;
using Microsoft.Extensions.Configuration;
using Andgasm.HoundDog.AccountManagement.Database;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class UserPhoneManager : IUserPhoneManager
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<UserManager> _logger;
        private readonly ISMSVerification _smsverifier;
        private readonly UserManager<HoundDogUser> _userManager;
        #endregion

        #region Constructor
        public UserPhoneManager(IConfiguration config,
                                         ILogger<UserManager> logger,
                                         ISMSVerification smsverify,
                                         UserManager<HoundDogUser> userManager)
        {
            _config = config;
            _logger = logger;
            _userManager = userManager;
            _smsverifier = smsverify;
        }
        #endregion

        #region Phone Confirmation
        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ConfirmPhoneNumber(string userid, string token)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });
            if (user.PhoneNumberConfirmed) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(string.Empty, "The phone number for this user account has already been confirmed!") });

            var phonesuccess = await _smsverifier.VerifyPhoneNumber(user.PhoneNumber, token);
            if (phonesuccess)
            {
                user.PhoneNumberConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded) return (false, updateResult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));
                return (true, new List<FieldValidationErrorDTO>());
            }
            return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO("VerificationCode", "Verification code could not be validated!") });
        }

        public async Task<(bool Succeeded, FieldValidationErrorDTO Error)> GeneratePhoneConfirmation(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new FieldValidationErrorDTO(nameof(UserDTO.UserName), "Specified user does not exist!"));
            return await GeneratePhoneConfirmation(user);
        }

        private async Task<(bool Succeeded, FieldValidationErrorDTO Error)> GeneratePhoneConfirmation(HoundDogUser user)
        {
            try
            {
                if (user.PhoneNumberConfirmed) return (false, new FieldValidationErrorDTO(string.Empty, "The user account phone number has already been confirmed!"));

                var sendsuccess = await _smsverifier.SendVerificationToPhoneNumber(user.PhoneNumber);
                if (!sendsuccess) return (false, new FieldValidationErrorDTO(string.Empty, $"Could not send verification code to number {user.PhoneNumber}"));
                return (true, new FieldValidationErrorDTO());
            }
            catch (Exception ex)
            {
                return (false, new FieldValidationErrorDTO(string.Empty, ex.Message));
            }
        }
        #endregion
    }
}
