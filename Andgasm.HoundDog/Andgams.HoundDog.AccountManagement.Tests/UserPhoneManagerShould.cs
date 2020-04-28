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
    public class UserPhoneManagerShould
    {
        #region Test Fields
        HoundDogUser _testUser = new HoundDogUser()
        {
            Id = "fd50153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUser",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666],
            PhoneNumber = "1234",
            PhoneNumberConfirmed = false
        };
        HoundDogUser _testUserFail = new HoundDogUser()
        {
            Id = "bb10153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUser",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666]
        };
        #endregion

        #region Generate Phone Confirmation Tests
        [Fact]
        public async Task SendConfirmationEmail_WhenValidUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var sms = new Mock<ISMSVerification>();
            sms.Setup(x => x.SendVerificationToPhoneNumber(_testUser.PhoneNumber)).ReturnsAsync(true);

            var usrmgr = MockUserManager();
            var am = new UserPhoneManager(config.Object, new NullLogger<UserPhoneManager>(), sms.Object, usrmgr.Object);
            var avatar = await am.GeneratePhoneConfirmation(_testUser.Id);
            Assert.True(avatar.Succeeded);
        }

        [Fact]
        public async Task ReportError_WhenNullUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var sms = new Mock<ISMSVerification>();
            sms.Setup(x => x.SendVerificationToPhoneNumber(_testUser.PhoneNumber)).ReturnsAsync(true);

            var usrmgr = MockUserManager();
            var am = new UserPhoneManager(config.Object, new NullLogger<UserPhoneManager>(), sms.Object, usrmgr.Object);
            var avatar = await am.GeneratePhoneConfirmation(null);
            Assert.False(avatar.Succeeded);
            Assert.Equal("You must provide a user id  to genertae sms verification code!", avatar.Error.Description);
        }

        [Fact]
        public async Task ReportError_WhenNotExistsUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var sms = new Mock<ISMSVerification>();
            sms.Setup(x => x.SendVerificationToPhoneNumber(_testUser.PhoneNumber)).ReturnsAsync(true);

            var usrmgr = MockUserManager();
            var am = new UserPhoneManager(config.Object, new NullLogger<UserPhoneManager>(), sms.Object, usrmgr.Object);
            var avatar = await am.GeneratePhoneConfirmation(Guid.NewGuid().ToString());
            Assert.False(avatar.Succeeded);
            Assert.Equal("Specified user does not exist!", avatar.Error.Description);
        }
        #endregion

        #region Confirm Email Tests

        [Fact]
        public async Task ConfirmPhone_WhenValidUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var sms = new Mock<ISMSVerification>();
            sms.Setup(x => x.VerifyPhoneNumber(_testUser.PhoneNumber, "9999")).ReturnsAsync(true);

            var usrmgr = MockUserManager();
            var am = new UserPhoneManager(config.Object, new NullLogger<UserPhoneManager>(), sms.Object, usrmgr.Object);
            var avatar = await am.ConfirmPhoneNumber(_testUser.Id, "9999");
            Assert.True(avatar.Succeeded);
        }

        [Fact]
        public async Task ReportConfirmError_WhenNullUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var sms = new Mock<ISMSVerification>();
            var usrmgr = MockUserManager();
            var am = new UserPhoneManager(config.Object, new NullLogger<UserPhoneManager>(), sms.Object, usrmgr.Object);
            var avatar = await am.ConfirmPhoneNumber(null, "9999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("You must provide a user id & verification token to confirm sms verification code!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportConfirmError_WhenNotExistsUserIdRecieved()
        {
            var config = new Mock<IConfiguration>();
            var sms = new Mock<ISMSVerification>();
            var usrmgr = MockUserManager();
            var am = new UserPhoneManager(config.Object, new NullLogger<UserPhoneManager>(), sms.Object, usrmgr.Object);
            var avatar = await am.ConfirmPhoneNumber("4321", "9999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Specified user does not exist!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportConfirmError_WhenInvalidCodeRecieved()
        {
            var config = new Mock<IConfiguration>();
            var sms = new Mock<ISMSVerification>();
            sms.Setup(x => x.VerifyPhoneNumber(_testUser.PhoneNumber, "1234")).ReturnsAsync(false);

            var usrmgr = MockUserManager();
            var am = new UserPhoneManager(config.Object, new NullLogger<UserPhoneManager>(), sms.Object, usrmgr.Object);
            var avatar = await am.ConfirmPhoneNumber(_testUser.Id, "1234");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Verification code could not be validated!", avatar.Errors.First().Description);
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

            mgr.Setup(x => x.UpdateAsync(_testUser)).ReturnsAsync(IdentityResult.Success); // allow search by username
            mgr.Setup(x => x.UpdateAsync(_testUserFail)).ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code = "FailUpdate", Description = "FailUpdate" })); // allow search by username

            return mgr;
        }
        #endregion
    }
}
