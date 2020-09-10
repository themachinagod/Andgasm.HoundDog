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

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class UserManager : IUserManager
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<UserManager> _logger;
        private readonly UserManager<HoundDogUser> _userManager;
        private readonly IUserEmailManager _useremailmanager;
        private readonly IUserPasswordManager _userpassmanager;
        private readonly IMapper _mapper;
        private readonly IParsePhoneNumber _phoneparser;
        #endregion

        #region Constructor
        public UserManager(IConfiguration config,
                           ILogger<UserManager> logger,
                           IMapper mapper,
                           UserManager<HoundDogUser> userManager,
                           IUserEmailManager useremailmanager,
                           IUserPasswordManager userpassmanager,
                           IParsePhoneNumber phoneparser)
        {
            _config = config;
            _logger = logger;
            _userManager = userManager;
            _useremailmanager = useremailmanager;
            _userpassmanager = userpassmanager;
            _mapper = mapper;
            _phoneparser = phoneparser;
        }
        #endregion

        #region CRUD
        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> CreateStandardUser(UserDTO userdata)
        {
            // TODO: hardcoded standard user role name!
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var userresult = CreateUserAndMapToEntity(userdata);
                if (userresult.Errors.Count() > 0) return (false, userresult.Errors);

                var createresult = await _userManager.CreateAsync(userresult.User, userdata.PasswordClear);
                if (!createresult.Succeeded) return (false, createresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

                userresult.User.RegisteredTimestamp = DateTime.UtcNow;
                var updateresult = await _userManager.UpdateAsync(userresult.User);
                if (!updateresult.Succeeded) return (false, updateresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

                var rolesresult = await this._userManager.AddToRolesAsync(userresult.User, new[] { "User" }); 
                if (!rolesresult.Succeeded) return (false, rolesresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

                var confirmmailresult = await _useremailmanager.GenerateEmailConfirmation(userresult.User.Id);
                if (!confirmmailresult.Succeeded) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(confirmmailresult.Error.Key), confirmmailresult.Error.Description) });

                scope.Complete();
                return (true, new List<FieldValidationErrorDTO>());
            }
        }

        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> UpdateUser(UserDTO userdata)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var findresult = await FindExistingUserAndRemapToEntity(userdata);
                if (findresult.Errors.Count() > 0) return (false, findresult.Errors);

                var updateresult = await _userManager.UpdateAsync(findresult.User);
                if (!updateresult.Succeeded) return (false, updateresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

                if (userdata.HasChangeEmail)
                {
                    var confirmmailresult = await _useremailmanager.GenerateEmailConfirmation(findresult.User.Id);
                    if (!confirmmailresult.Succeeded) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(confirmmailresult.Error.Key), confirmmailresult.Error.Description) });

                }

                scope.Complete();
                return (true, new List<FieldValidationErrorDTO>());
            }
        }

        private void ChangePhoneNumber(HoundDogUser user, UserDTO userdata)
        {
            if (userdata.HasChangePhone)
            {
                user.PhoneNumber = _phoneparser.ParsePhoneNumber(userdata.PhoneNumber);
                user.PhoneNumberConfirmed = false;
            }
        }

        private void ChangeEmailAddress(HoundDogUser user, UserDTO userdata)
        {
            if (userdata.HasChangeEmail)
            {
                user.EmailConfirmed = false;
                user.EmailConfirmedTimestamp = null;
                user.Email = userdata.Email;
                user.NormalizedEmail = userdata.Email.ToUpperInvariant();
            }
        }
        #endregion

        #region Retrieval
        public async Task<(UserDTO User, IEnumerable<FieldValidationErrorDTO> Errors)> GetCurrentUser(ClaimsPrincipal currentuser)
        {
            var user = await _userManager.GetUserAsync(currentuser);
            if (user == null) return (null, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });

            var userdto = _mapper.Map<UserDTO>(user);
            userdto.AccountType = await DetermineAccountType(user);
            return (userdto, null);
        }
        #endregion

        #region Internal Mapping
        private async Task<(HoundDogUser User, IEnumerable<FieldValidationErrorDTO> Errors)> FindExistingUserAndRemapToEntity(UserDTO userdata)
        {
            if (userdata.Id == Guid.Empty) return (null, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.UserName), "Specified user identifier was not valid!") });

            var user = await _userManager.FindByIdAsync(userdata.Id.ToString());
            if (user == null) return (null, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.UserName), "Specified user does not exist!") });
            userdata.HasChangeEmail = user.Email.ToLowerInvariant() != userdata.Email.ToLowerInvariant();
            userdata.HasChangePhone = user.PhoneNumber.ToLowerInvariant() != userdata.PhoneNumber.ToLowerInvariant();

            user = _mapper.Map(userdata, user);
            ChangePhoneNumber(user, userdata);
            ChangeEmailAddress(user, userdata);

            return (user, new List<FieldValidationErrorDTO>());
        }

        private (HoundDogUser User, IEnumerable<FieldValidationErrorDTO> Errors) CreateUserAndMapToEntity(UserDTO userdata)
        {
            userdata.HasChangeEmail = true;
            userdata.HasChangePhone = true;

            var user = _mapper.Map<HoundDogUser>(userdata);
            ChangePhoneNumber(user, userdata);
            ChangeEmailAddress(user, userdata);

            return (user, new List<FieldValidationErrorDTO>());
        }

        private async Task<string> DetermineAccountType(HoundDogUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any(x => x.ToLowerInvariant() == "admin")) return "Admin";
            if (roles.Any(x => x.ToLowerInvariant() == "user")) return "User";
            return "Guest";
        }
        #endregion
    }
}
