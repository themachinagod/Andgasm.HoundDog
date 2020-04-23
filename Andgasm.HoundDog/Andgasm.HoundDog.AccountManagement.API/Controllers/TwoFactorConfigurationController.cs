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
    public class TwoFactorConfigurationController : APIController
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserTwoFactorAuthManager _userAuthManager;
        #endregion

        #region Constructors
        public TwoFactorConfigurationController(ILogger<AuthenticationController> logger,
                                           IConfiguration config,
                                           IUserTwoFactorAuthManager userauthManager)
        {
            _config = config;
            _logger = logger;
            _userAuthManager = userauthManager;
        }
        #endregion

        #region 2FA Management
        [HttpGet]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> SendAuthenticatorSharedKey([FromRoute] string userid)
        {
            var result = await _userAuthManager.GenerateAuthenticatorSharedKey(userid);
            if (result.GeneratedCode == null) return CreateBadRequestError(result.Error.Key, result.Error.Description);
            return Ok(result.GeneratedCode);
        }

        [HttpPost()]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> EnableTwoFormAuthenticaton(string userid, [FromQuery] string verifycode)
        {
            if (string.IsNullOrWhiteSpace(verifycode)) return CreateBadRequestError(string.Empty, "Confirmation token must be provided to successfully confirm email for account!");
            var result = await _userAuthManager.Enable2FA(userid, verifycode);
            if (!result.Succeeded) return CreateBadRequestError(result.Errors);
            return Ok();
        }

        [HttpPut]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> ResetTwoFormAuthenticaton(string userid)
        {
            var result = await _userAuthManager.Disable2FA(userid);
            if (!result.Success) return CreateBadRequestError(result.Errors);
            return Ok();
        }
        #endregion
    }
}
