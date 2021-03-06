﻿using Andgasm.HoundDog.AccountManagement.Interfaces;
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
using System.Transactions;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class UserEmailManager : IUserEmailManager
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<UserEmailManager> _logger;
        private readonly IEmailSender _emailer;
        private readonly UserManager<HoundDogUser> _userManager;
        #endregion

        #region Constructor
        public UserEmailManager(IConfiguration config,
                                         ILogger<UserEmailManager> logger,
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
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (string.IsNullOrWhiteSpace(userid))
                    return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide a user id to confirm email!") });

                var user = await _userManager.FindByIdAsync(userid);
                if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });

                if (!user.EmailConfirmed)
                {
                    var result = await _userManager.ConfirmEmailAsync(user, token);
                    if (!result.Succeeded) return (false, result.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

                    user.EmailConfirmedTimestamp = DateTime.UtcNow;
                    var updateresult = await _userManager.UpdateAsync(user);
                    if (!updateresult.Succeeded) return (false, updateresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));
                }

                scope.Complete();
                return (true, new List<FieldValidationErrorDTO>());
            }
        }

        public async Task<(bool Succeeded, FieldValidationErrorDTO Error)> GenerateEmailConfirmation(string userid)
        {
            if (string.IsNullOrWhiteSpace(userid))
                return (false, new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide a user id to generate confirm email!"));

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new FieldValidationErrorDTO(nameof(UserDTO.UserName), "Specified user does not exist!"));
            return await GenerateEmailConfirmation(user);
        }

        private async Task<(bool Succeeded, FieldValidationErrorDTO Error)> GenerateEmailConfirmation(HoundDogUser user)
        {
            var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedtoken = HttpUtility.UrlEncode(confirmToken);
            var callbackUrl = $"{_config.GetSection(IUserEmailManager.HostAPIBaseUrlConfigName)?.Value}api/user/{user.Id}/emailconfirmation?token={encodedtoken}";
            var body = @$"<form method='post' action='{callbackUrl}' class='inline'" +
                        $"  <label>Please click the below button to confirm your email address on your HoundDog account.</label><br />" +
                        $"  <button type='submit' class='link-button'>I hereby confirm this email address to be my own</button>" +
                        $"</form>";
            await _emailer.SendEmailAsync(_config.GetSection(IUserEmailManager.SendingFromAddressConfigName)?.Value, user.Email, "HoundDog email verification request", body, true);
            return (true, new FieldValidationErrorDTO());
        }
        #endregion
    }
}
