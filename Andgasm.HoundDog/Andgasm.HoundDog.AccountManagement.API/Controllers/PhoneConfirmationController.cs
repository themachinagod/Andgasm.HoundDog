using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Andgasm.HoundDog.API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Andgasm.HoundDog.AccountManagement.API.Auth;

namespace Andgasm.HoundDog.AccountManagement.API
{
    [ApiController]
    [Route("api/user/{userid}/[controller]")]
    public class PhoneConfirmationController : APIController
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserPhoneManager _userPhoneManager;
        #endregion

        #region Constructor
        public PhoneConfirmationController(ILogger<AuthenticationController> logger,
                                           IConfiguration config,
                                           IUserPhoneManager userphoneManager)
        {
            _config = config;
            _logger = logger;
            _userPhoneManager = userphoneManager;
        }
        #endregion

        #region Phone Verification
        [HttpGet]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> SendPhoneVerificationCode(string userid)
        {
            var result = await _userPhoneManager.GeneratePhoneConfirmation(userid);
            if (!result.Succeeded) return CreateBadRequestError(result.Error.Key, result.Error.Description);
            return StatusCode(201);
        }

        [HttpPost]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> VerifyPhoneNumber(string userid, [FromQuery] string verifycode)
        {
            if (string.IsNullOrWhiteSpace(verifycode)) return CreateBadRequestError(string.Empty, "Insufficient data was provided to successfully confirm phone number for account!");
            var result = await _userPhoneManager.ConfirmPhoneNumber(userid, verifycode);
            if (!result.Succeeded) return CreateBadRequestError(result.Errors);
            return Ok();
        }
        #endregion
    }
}
