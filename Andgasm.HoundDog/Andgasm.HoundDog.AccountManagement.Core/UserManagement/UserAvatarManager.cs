using Andgasm.HoundDog.AccountManagement.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;
using Andgasm.HoundDog.API.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Transactions;
using System.Security.Claims;
using Andgasm.HoundDog.AccountManagement.Database;
using AutoMapper;
using System.Text.RegularExpressions;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class UserAvatarManager : IUserAvatarManager
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<UserManager> _logger;
        private readonly UserManager<HoundDogUser> _userManager;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public UserAvatarManager(IConfiguration config,
                           ILogger<UserManager> logger,
                           IMapper mapper,
                           UserManager<HoundDogUser> userManager)
        {
            _config = config;
            _logger = logger;
            _userManager = userManager;
            _mapper = mapper;
        }
        #endregion

        public async Task<(byte[] ImageData, IEnumerable<FieldValidationErrorDTO> Errors)> GetAvatar(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (null, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });

            return (user.ProfileAvatar, new List<FieldValidationErrorDTO>());
        }

        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> UploadAvatar(string userid, byte[] payload)
        {
            if (string.IsNullOrWhiteSpace(userid) || payload == null)
                return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide a user and image to upload an avatar!") });

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });

            user.ProfileAvatar = payload;
            var updateresult = await _userManager.UpdateAsync(user);
            if (!updateresult.Succeeded) return (false, updateresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

            return (true, new List<FieldValidationErrorDTO>());            
        }
    }
}
