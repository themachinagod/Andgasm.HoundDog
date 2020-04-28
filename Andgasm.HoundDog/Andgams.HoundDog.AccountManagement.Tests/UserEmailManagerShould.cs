using Andgasm.HoundDog.AccountManagement.Core;
using Andgasm.HoundDog.AccountManagement.Database;
using Andgasm.HoundDog.Core.Email.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Andgams.HoundDog.AccountManagement.Tests
{
    public class UserEmailManagerShould
    {
        #region Test Fields
        HoundDogUser _testUser = new HoundDogUser()
        {
            Id = "fd50153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUser",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666]
        };
        HoundDogUser _testUserFail = new HoundDogUser()
        {
            Id = "bb10153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUser",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666]
        };
        #endregion

        #region Generate Email Confirmation Tests
        [Fact]
        public async Task SendConfirmationEmail_WhenValidUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var email = new Mock<IEmailSender>();
            var usrmgr = MockUserManager();
            var am = new UserEmailManager(config.Object, new NullLogger<UserEmailManager>(), email.Object, usrmgr.Object);
            var avatar = await am.GenerateEmailConfirmation(_testUser.Id);
            Assert.True(avatar.Succeeded);
        }

        [Fact]
        public async Task ReportError_WhenNullUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var email = new Mock<IEmailSender>();
            var usrmgr = MockUserManager();
            var am = new UserEmailManager(config.Object, new NullLogger<UserEmailManager>(), email.Object, usrmgr.Object);
            var avatar = await am.GenerateEmailConfirmation(null);
            Assert.False(avatar.Succeeded);
            Assert.Equal("You must provide a user id to generate confirm email!", avatar.Error.Description);
        }

        [Fact]
        public async Task ReportError_WhenNotExistsUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var email = new Mock<IEmailSender>();
            var usrmgr = MockUserManager();
            var am = new UserEmailManager(config.Object, new NullLogger<UserEmailManager>(), email.Object, usrmgr.Object);
            var avatar = await am.GenerateEmailConfirmation(Guid.NewGuid().ToString());
            Assert.False(avatar.Succeeded);
            Assert.Equal("Specified user does not exist!", avatar.Error.Description);
        }
        #endregion

        #region Confirm Email Tests

        [Fact]
        public async Task ConfirmEmail_WhenValidUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var email = new Mock<IEmailSender>();
            var usrmgr = MockUserManager();
            var am = new UserEmailManager(config.Object, new NullLogger<UserEmailManager>(), email.Object, usrmgr.Object);
            var avatar = await am.ConfirmEmailAddress(_testUser.Id, "9999");
            Assert.True(avatar.Succeeded);
        }

        [Fact]
        public async Task ReportConfirmError_WhenNullUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var email = new Mock<IEmailSender>();
            var usrmgr = MockUserManager();
            var am = new UserEmailManager(config.Object, new NullLogger<UserEmailManager>(), email.Object, usrmgr.Object);
            var avatar = await am.ConfirmEmailAddress(null, "9999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("You must provide a user id to confirm email!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportConfirmError_WhenNotExistsUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var email = new Mock<IEmailSender>();
            var usrmgr = MockUserManager();
            var am = new UserEmailManager(config.Object, new NullLogger<UserEmailManager>(), email.Object, usrmgr.Object);
            var avatar = await am.ConfirmEmailAddress(Guid.NewGuid().ToString(), "9999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Specified user does not exist!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportConfirmError_WhenInvalidCodeRecieved()
        {
            var config = new Mock<IConfiguration>();
            var email = new Mock<IEmailSender>();
            var usrmgr = MockUserManager();
            var am = new UserEmailManager(config.Object, new NullLogger<UserEmailManager>(), email.Object, usrmgr.Object);
            var avatar = await am.ConfirmEmailAddress(_testUserFail.Id, "1234");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Fail", avatar.Errors.First().Description);
        }
        #endregion

        #region Mock Helpers
        public Mock<UserManager<HoundDogUser>> MockUserManager()
        {
            IList<IUserValidator<HoundDogUser>> UserValidators = new List<IUserValidator<HoundDogUser>>();
            IList<IPasswordValidator<HoundDogUser>> PasswordValidators = new List<IPasswordValidator<HoundDogUser>>();

            var store = new Mock<IUserStore<HoundDogUser>>();
            UserValidators.Add(new UserValidator<HoundDogUser>());
            PasswordValidators.Add(new PasswordValidator<HoundDogUser>());
            var mgr = new Mock<UserManager<HoundDogUser>>(store.Object, null, null, UserValidators, PasswordValidators, null, null, null, null);

            mgr.Setup(x => x.FindByIdAsync(_testUser.Id)).ReturnsAsync(_testUser); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUser)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            mgr.Setup(x => x.FindByIdAsync(_testUserFail.Id)).ReturnsAsync(_testUserFail); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUserFail)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            mgr.Setup(x => x.ConfirmEmailAsync(_testUser, "9999")).ReturnsAsync(IdentityResult.Success); // allow search by username
            mgr.Setup(x => x.ConfirmEmailAsync(_testUserFail, "1234")).ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code = "Fail", Description = "Fail" })); // allow search by username

            return mgr;
        }
        #endregion
    }
}
