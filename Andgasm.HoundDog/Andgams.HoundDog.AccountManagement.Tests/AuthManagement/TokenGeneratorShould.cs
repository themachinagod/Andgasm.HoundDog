using Andgasm.HoundDog.AccountManagement.Core.AuthManagement;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Xunit;

namespace Andgams.HoundDog.AccountManagement.Tests
{
    public class TokenGeneratorShould
    {
        [Fact]
        public void GenerateValidToken_WhenValidUserRecieved()
        {
            var tg = InitialiseTokenGenerator();
            var usrid = Guid.NewGuid();
            var user = new UserDTO()
            {
                Id = usrid,
                UserName = "TestUser",
                Roles = "user"
            };
            var tokenresult = tg.GenerateToken(user);
            Assert.Equal("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9", tokenresult.Split('.')[0]); 
        }

        [Fact]
        
        public void ThrowException_WhenNoUserRecieved()
        {
            var tg = InitialiseTokenGenerator();
            Exception ex = Assert.Throws<Exception>(() => tg.GenerateToken(null));
            Assert.Equal("User data must be supplied to generate token!", ex.Message);
        }

        [Fact]

        public void ThrowException_WhenNoUserNameRecieved()
        {
            var tg = InitialiseTokenGenerator();
            var usrid = Guid.NewGuid();
            var user = new UserDTO()
            {
                Id = usrid,
                UserName = null,
                Roles = "user"
            };
            Exception ex = Assert.Throws<Exception>(() => tg.GenerateToken(null));
            Assert.Equal("User data must be supplied to generate token!", ex.Message);
        }

        [Fact]

        public void ThrowException_WhenNoUserIdRecieved()
        {
            var tg = InitialiseTokenGenerator();
            var usrid = Guid.NewGuid();
            var user = new UserDTO()
            {
                Id = Guid.Empty,
                UserName = "TestUser",
                Roles = "user"
            };
            Exception ex = Assert.Throws<Exception>(() => tg.GenerateToken(null));
            Assert.Equal("User data must be supplied to generate token!", ex.Message);
        }

        [Fact]

        public void ThrowException_WhenNoUserRolesRecieved()
        {
            var tg = InitialiseTokenGenerator();
            var usrid = Guid.NewGuid();
            var user = new UserDTO()
            {
                Id = Guid.Empty,
                UserName = "TestUser",
                Roles = null
            };
            Exception ex = Assert.Throws<Exception>(() => tg.GenerateToken(null));
            Assert.Equal("User data must be supplied to generate token!", ex.Message);
        }

        private TokenGenerator InitialiseTokenGenerator()
        {
            var _config = new Mock<IConfiguration>();
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenConfigName).Value).Returns("Fu5pvu7yW3uMRVRwXRo40l30mVsWC4tj");
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenExpiryConfigName).Value).Returns("1");
            return new TokenGenerator(_config.Object);
        }
    }
}
