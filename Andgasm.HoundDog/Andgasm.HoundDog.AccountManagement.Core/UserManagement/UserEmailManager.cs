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
using System.Web;
using Andgasm.HoundDog.AccountManagement.Database;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class UserEmailManager : IUserEmailManager
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<UserManager> _logger;
        private readonly IEmailSender _emailer;
        private readonly UserManager<HoundDogUser> _userManager;
        #endregion

        #region Constructor
        public UserEmailManager(IConfiguration config,
                                         ILogger<UserManager> logger,
                                         IEmailSender emailer,
                                         UserManager<HoundDogUser> userManager)
        {
            _config = config;
            _logger = logger;
            _userManager = userManager;
            _emailer = emailer;
        }
        #endregion

        #region Email Confirmation
        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ConfirmEmailAddress(string userid, string token)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return (false, result.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

            return (true, new List<FieldValidationErrorDTO>());
        }

        public async Task<(bool Succeeded, FieldValidationErrorDTO Error)> GenerateEmailConfirmation(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new FieldValidationErrorDTO(nameof(UserDTO.UserName), "Specified user does not exist!"));
            return await GenerateEmailConfirmation(user);
        }

        private async Task<(bool Succeeded, FieldValidationErrorDTO Error)> GenerateEmailConfirmation(HoundDogUser user)
        {
            try
            {
                var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedtoken = HttpUtility.UrlEncode(confirmToken);
                var callbackUrl = $"{_config.GetSection("HostApiLinks:BaseUrl")?.Value}api/user/{user.Id}/emailconfirmation?token={encodedtoken}";
                var body = @$"<form method='post' action='{callbackUrl}' class='inline'" +
                            $"  <label>Please click the below button to confirm your email address on your HoundDog account.</label><br />" +
                            $"  <button type='submit' class='link-button'>I hereby confirm this email address to be my own</button>" +
                            $"</form>";
                await _emailer.SendEmailAsync(_config.GetSection("AppSettings:SendingFromAddress")?.Value, user.Email, "HoundDog email verification request", body);
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
