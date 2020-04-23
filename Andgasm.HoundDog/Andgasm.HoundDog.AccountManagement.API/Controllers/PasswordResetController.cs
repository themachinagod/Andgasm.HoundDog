using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Andgasm.HoundDog.API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using Andgasm.HoundDog.AccountManagement.API.Auth;

namespace Andgasm.HoundDog.AccountManagement.API
{
    [ApiController]
    [Route("api/user/{username}/[controller]")]
    public class PasswordResetController : APIController
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserPasswordManager _userPasswordManager;
        #endregion

        #region Constructors
        public PasswordResetController(ILogger<AuthenticationController> logger,
                                           IConfiguration config,
                                           IUserPasswordManager passmanager)
        {
            _config = config;
            _logger = logger;
            _userPasswordManager = passmanager;
        }
        #endregion

        #region Reset Operations
        [HttpGet]
        public async Task<IActionResult> SendPasswordResetLink(string username, [FromQuery] string verificationCode, [FromQuery] string email)
        {
            var result = await _userPasswordManager.GeneratePasswordReset(username, verificationCode, email);
            if (!result.Succeeded) return CreateBadRequestError(result.Error.Key, result.Error.Description);
            return StatusCode(201);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string username, [FromBody] UserPasswordChangeDTO userdata)
        {
            if (userdata == null || string.IsNullOrWhiteSpace(username) || userdata.UserId == Guid.Empty) return CreateBadRequestError(string.Empty, "Insufficient data was provided to successfully reset password!");
            var result = await _userPasswordManager.ResetPassword(userdata.UserId, username, userdata.Password, userdata.ResetToken, userdata.VerificationCode);
            if (!result.Succeeded) return CreateBadRequestError(result.Errors);
            return StatusCode(201);
        }
        #endregion

        #region Change Operations
        [HttpPut("{userid}")]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> UpdatePassword(string userid, [FromBody] UserPasswordChangeDTO userdata)
        {
            if (string.IsNullOrWhiteSpace(userid) || userdata == null) return CreateBadRequestError(string.Empty, "No user data was supplied on the request!");
            var result = await _userPasswordManager.ChangePassword(userid, userdata.OldPassword, userdata.Password, userdata.VerificationCode);
            if (!result.Succeeded) return CreateBadRequestError(result.Errors);
            return StatusCode(201);
        }
        #endregion
    }
}
