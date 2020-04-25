using System.Linq;
using System.Security;
using System.Security.Claims;
using AppUserAuthentication.TokenGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Xunit;

namespace AppUserAuthenticationTest
{
    /// <summary>
    /// Test class for <see cref="DefaultJwtHandler"/>. 
    /// </summary>
    public class DefaultJwtHandlerTest
    {
        private readonly DefaultJwtHandler _defaultJwtHandler;
        
        public DefaultJwtHandlerTest()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["Jwt:key"]).Returns("key1234567891011");
            mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("issuer");
            _defaultJwtHandler = new DefaultJwtHandler(mockConfig.Object);
        }

        /// <summary>
        /// Tests the GetPrincipalFromExpiredToken method when the token does exists
        /// </summary>
        [Fact]
        public void TestGetPrincipalFromExpiredTokenCorrect()
        {
            //setup expected
            const string expectedFirstName = "test";
            const string expectedEmail = "test@test.com";
            const string expectedIssAndAud = "issuer";
            
            //run method
            var token = _defaultJwtHandler.Generate(DefaultJwtHandler.GetDefaultClaims(expectedFirstName, expectedEmail));
            var result = _defaultJwtHandler.GetPrincipalFromExpiredToken(token);

            Assert.Equal(expectedFirstName, result.FindFirstValue(ClaimTypes.Name));
            Assert.Equal(expectedEmail, result.FindFirstValue(ClaimTypes.Email));
            Assert.Equal(expectedIssAndAud, result.FindFirstValue(JwtRegisteredClaimNames.Iss));
            Assert.Equal(expectedIssAndAud, result.FindFirstValue(JwtRegisteredClaimNames.Aud));
        }

        /// <summary>
        /// Tests the GetPrincipalFromExpiredToken method when the token is faulty.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TestGetPrincipalFromExpiredTokenFaultyToken(string token)
        {
            Assert.Throws<SecurityException>(() => _defaultJwtHandler.GetPrincipalFromExpiredToken(token));
        }
    }
}