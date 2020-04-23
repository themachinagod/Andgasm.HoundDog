using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Andgasm.HoundDog.API.Core;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Andgasm.HoundDog.AccountManagement.API.Auth;

namespace Andgasm.HoundDog.AccountManagement.API
{
    [ApiController]
    [Route("api/user/{userid}/[controller]")]
    public class AvatarController : APIController
    {
        #region Fields
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _config;
        private readonly IUserAvatarManager _avatarManager;
        #endregion

        #region Constructors
        public AvatarController(ILogger<AuthenticationController> logger,
                                IConfiguration config,
                                IUserAvatarManager avatarManager)
        {
            _logger = logger;
            _config = config;
            _avatarManager = avatarManager;
        }
        #endregion

        #region Avatar Management
        [HttpGet]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> GetAvatar(string userid)
        {
            var result = await _avatarManager.GetAvatar(userid);
            if (result.Errors.Count() > 0) return CreateBadRequestError(result.Errors);

            if (result.ImageData != null) return File(result.ImageData, "image/png");
            else return Ok();
        }

        [HttpPost]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> UploadAvatarImage(string userid, [FromForm(Name = "avatardata")] IFormFile avatarpayload)
        {
            var result = await _avatarManager.UploadAvatar(userid, await avatarpayload.GetBytes());
            if (!result.Succeeded) return CreateBadRequestError(result.Errors);
            return StatusCode(201);
        }
        #endregion
    }
}
