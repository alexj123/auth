using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AppUserAuthentication.Access.Actions;
using AppUserAuthentication.Access.Repositories;
using AppUserAuthentication.Models;
using AppUserAuthentication.Models.Identity;
using AppUserAuthentication.TokenGeneration;
using AppUserAuthenticationTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace AppUserAuthenticationTest
{
    public class UserRepositoryTest
    {
        private readonly UserRepository<TestAppUser> _userRepository;
        private Mock<UserManager<TestAppUser>> _mockUserManager;
        private readonly Mock<IJwtHandler> _mockDefaultJwtHandler;

        public UserRepositoryTest()
        {
            var userManager = GetUserManagerMock();

            _mockDefaultJwtHandler = new Mock<IJwtHandler>();
            
            _userRepository = new UserRepository<TestAppUser>(userManager, _mockDefaultJwtHandler.Object, new DefaultRefreshTokenGenerator());
        }

        /// <summary>
        /// Tests if the UserRepository returns a correct result when the correct information is supplied for create.
        /// </summary>
        [Fact]
        public void TestCreateCorrectInput()
        {
            //setup mocks
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<TestAppUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new IdentityResult()));
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<TestAppUser>()))
                .Verifiable();
            const string expectedToken = "test_token";
            _mockDefaultJwtHandler.Setup(x => x.Generate(It.IsAny<List<Claim>>()))
                .Returns(expectedToken);

            //setup result
            var testUserCorrect = new TestAppUser
            {
                Email = "test@test.com",
                UserName = "username",
                FirstName = "Test"
            };
            
            var task = _userRepository.Create(testUserCorrect, "Aa1234");
            var result = task.Result;
            
            Assert.True(result.Succeeded);
            Assert.Empty(result.Errors);
            Assert.NotNull(result.RefreshToken);
            Assert.Equal(expectedToken, result.Jwt);
            _mockUserManager.Verify(e => e.UpdateAsync(It.IsAny<TestAppUser>()), Times.Once);
        }
        
        /// <summary>
        /// Tests if the UserRepository returns a result containing errors when a identity errors occur in create.
        /// </summary>
        [Fact]
        public void TestCreateIdentityErrors()
        {
            const string expectedErrorMsg = "Test fail";
            //setup mocks
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<TestAppUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError {Description = expectedErrorMsg})));

            //setup user
            var testUserCorrect = new TestAppUser
            {
                Email = "test@test.com",
                UserName = "username",
                FirstName = "Test"
            };
            
            //run method
            var task = _userRepository.Create(testUserCorrect, "Aa1234");
            var result = task.Result;
            
            Assert.False(result.Succeeded);
            Assert.Equal(1, result.Errors.Count);
            Assert.Null(result.Jwt);
            Assert.Null(result.RefreshToken);
            Assert.Equal(expectedErrorMsg, result.Errors[0].Message);
            _mockDefaultJwtHandler.Verify(e => e.Generate(null), Times.Never());
        }

        /// <summary>
        /// Tests if the authenticates handles correct input correctly.
        /// </summary>
        [Fact]
        public void TestAuthenticateCorrectInput()
        {
            //setup user
            var testUserCorrect = new TestAppUser
            {
                Email = "test@test.com",
                UserName = "username",
                FirstName = "Test"
            };
            
            //setup expected
            const string expectedToken = "test_token";

            //setup mocks
            _mockUserManager.Setup(x => x.CheckPasswordAsync(It.IsAny<TestAppUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<TestAppUser>()))
                .Verifiable();
            _mockDefaultJwtHandler.Setup(x => x.Generate(It.IsAny<List<Claim>>()))
                .Returns(expectedToken);

            //run method
            var task = _userRepository.Authenticate(testUserCorrect, "Aa1234");
            var result = task.Result;
            
            Assert.True(result.Succeeded);
            Assert.Empty(result.Errors);
            Assert.NotNull(result.RefreshToken);
            Assert.Equal(expectedToken, result.Jwt);
            _mockUserManager.Verify(e => e.UpdateAsync(It.IsAny<TestAppUser>()), Times.Once);
        }
        
        /// <summary>
        /// Tests if the authenticates handles a password check fail correctly.
        /// </summary>
        [Fact]
        public void TestAuthenticateCheckPasswordFail()
        {
            //setup mocks
            _mockUserManager.Setup(x => x.CheckPasswordAsync(It.IsAny<TestAppUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            //setup user
            var testUserCorrect = new TestAppUser
            {
                Email = "test@test.com",
                UserName = "username",
                FirstName = "Test"
            };
            
            //setup expected
            const string expectedErrorMsg = "Invalid credentials supplied";
            
            //run method
            var task = _userRepository.Authenticate(testUserCorrect, "Aa1234");
            var result = task.Result;
            
            Assert.False(result.Succeeded);
            Assert.Equal(1, result.Errors.Count);
            Assert.Null(result.Jwt);
            Assert.Null(result.RefreshToken);
            Assert.Equal(expectedErrorMsg, result.Errors[0].Message);
            _mockUserManager.Verify(e => e.UpdateAsync(It.IsAny<TestAppUser>()), Times.Never);
        }
        
        /// <summary>
        /// Tests if the refresh token handles correct input correctly.
        /// </summary>
        [Fact]
        public void TestRefreshTokenCorrectInput()
        {
            //setup user and list of users.
            var testUser = new TestAppUser
            {
                Email = "test@test.com",
                UserName = "username",
                FirstName = "Test",
                RefreshTokens = new List<IRefreshToken> {new RefreshToken {Token = "refresh", 
                    Expiration = new DateTimeOffset(DateTime.Now.AddDays(100)).ToUnixTimeSeconds()} }
            };
            var listOfUsers = new List<TestAppUser> {testUser};
            
            //setup expected
            const string expectedToken = "test_token";
            
            //setup mocks
            var mockDbSet = listOfUsers.AsQueryable().BuildMockDbSet();
            _mockUserManager.Setup(x => x.Users).Returns(mockDbSet.Object);
            var principal = new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(new List<Claim> {new Claim(ClaimTypes.Email, "test@test.com")}));
            _mockDefaultJwtHandler.Setup(x => x.GetPrincipalFromExpiredToken(It.IsAny<string>()))
                .Returns(principal);
            _mockDefaultJwtHandler.Setup(x => x.Generate(It.IsAny<List<Claim>>()))
                .Returns(expectedToken);
            
            //run method
            var task = _userRepository.RefreshToken("jwt", "refresh");
            var result = task.Result;
            
            Assert.True(result.Succeeded);
            Assert.Empty(result.Errors);
            Assert.Equal(expectedToken, result.Jwt);
            Assert.NotNull(result.RefreshToken);
            _mockUserManager.Verify(e => e.UpdateAsync(It.IsAny<TestAppUser>()), Times.Once);
        }

        /// <summary>
        /// Tests if the refresh token handles an invalid token correctly.
        /// </summary>
        [Theory]
        [InlineData("refresh", -10)] //expired token
        [InlineData("refreshInvalid", 10)] //not known token
        [InlineData(null, 10)] //null token
        public void TestRefreshTokenInvalid(string refreshToken, int daysToAdd)
        {
            //setup user and list of users.
            var testUser = new TestAppUser
            {
                Email = "test@test.com",
                UserName = "username",
                FirstName = "Test",
                RefreshTokens = new List<IRefreshToken> {new RefreshToken {Token = "refresh", 
                    Expiration = new DateTimeOffset(DateTime.Now.AddDays(daysToAdd)).ToUnixTimeSeconds()} }
            };
            var listOfUsers = new List<TestAppUser> {testUser};
            
            //setup expected
            const string expectedErrorMsg = "Invalid token";
            
            //setup mocks
            var mockDbSet = listOfUsers.AsQueryable().BuildMockDbSet();
            _mockUserManager.Setup(x => x.Users).Returns(mockDbSet.Object);
            var principal = new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(new List<Claim> {new Claim(ClaimTypes.Email, "test@test.com")}));
            _mockDefaultJwtHandler.Setup(x => x.GetPrincipalFromExpiredToken(It.IsAny<string>()))
                .Returns(principal);

            //run method
            var task = _userRepository.RefreshToken("jwt", refreshToken);
            var result = task.Result;
            
            Assert.False(result.Succeeded);
            Assert.Equal(1, result.Errors.Count);
            Assert.Null(result.Jwt);
            Assert.Null(result.RefreshToken);
            Assert.Equal(expectedErrorMsg, result.Errors[0].Message);
            _mockUserManager.Verify(e => e.UpdateAsync(It.IsAny<TestAppUser>()), Times.Never);
            _mockDefaultJwtHandler.Verify(e => e.Generate(It.IsAny<List<Claim>>()), Times.Never);
        }
        
        /// <summary>
        /// Tests if the refresh token throws an exception of no claim with type email can be found.
        /// </summary>
        [Fact]
        public async Task TestRefreshTokenNoEmailClaim()
        {
            //setup user and list of users.
            var testUser = new TestAppUser
            {
                Email = "test@test.com",
                UserName = "username",
                FirstName = "Test",
                RefreshTokens = new List<IRefreshToken> {new RefreshToken {Token = "refresh", 
                    Expiration = new DateTimeOffset(DateTime.Now.AddDays(100)).ToUnixTimeSeconds()} }
            };
            var listOfUsers = new List<TestAppUser> {testUser};

            //setup mocks
            var mockDbSet = listOfUsers.AsQueryable().BuildMockDbSet();
            _mockUserManager.Setup(x => x.Users).Returns(mockDbSet.Object);
            var principal = new ClaimsPrincipal();
            _mockDefaultJwtHandler.Setup(x => x.GetPrincipalFromExpiredToken(It.IsAny<string>()))
                .Returns(principal);

            await Assert.ThrowsAsync<NullReferenceException>(() => _userRepository.RefreshToken("jwt", "refresh"));
            _mockUserManager.Verify(e => e.UpdateAsync(It.IsAny<TestAppUser>()), Times.Never);
            _mockDefaultJwtHandler.Verify(e => e.Generate(It.IsAny<List<Claim>>()), Times.Never);
        }

        /// <summary>
        /// Tests if the refresh token handles an invalid jwt correctly.
        /// </summary>
        [Fact]
        public void TestRefreshTokenInvalidJwt()
        {
            //setup user and list of users.
            var testUser = new TestAppUser
            {
                Email = "test@test.com",
                UserName = "username",
                FirstName = "Test",
                RefreshTokens = new List<IRefreshToken> {new RefreshToken {Token = "refresh", 
                    Expiration = new DateTimeOffset(DateTime.Now.AddDays(100)).ToUnixTimeSeconds()} }
            };
            var listOfUsers = new List<TestAppUser> {testUser};

            //setup expected
            const string expectedErrorMsg = "Invalid token";
            
            //setup mocks
            var mockDbSet = listOfUsers.AsQueryable().BuildMockDbSet();
            _mockUserManager.Setup(x => x.Users).Returns(mockDbSet.Object);
            //this returns null, as the jwt is invalid
            _mockDefaultJwtHandler.Setup(x => x.GetPrincipalFromExpiredToken(It.IsAny<string>()));

            //run method
            var task = _userRepository.RefreshToken("jwt", "refresh");
            var result = task.Result;
            
            Assert.False(result.Succeeded);
            Assert.Equal(1, result.Errors.Count);
            Assert.Null(result.Jwt);
            Assert.Null(result.RefreshToken);
            Assert.Equal(expectedErrorMsg, result.Errors[0].Message);
            _mockUserManager.Verify(e => e.UpdateAsync(It.IsAny<TestAppUser>()), Times.Never);
            _mockDefaultJwtHandler.Verify(e => e.Generate(It.IsAny<List<Claim>>()), Times.Never);
        }

        
        /// <summary>
        /// Gets the user manager mock.
        /// </summary>
        /// <returns>a mock of a user manager</returns>
        private UserManager<TestAppUser> GetUserManagerMock()
        {
            _mockUserManager = new Mock<UserManager<TestAppUser>>(
                new Mock<IUserStore<TestAppUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<TestAppUser>>().Object,
                new IUserValidator<TestAppUser>[0],
                new IPasswordValidator<TestAppUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<TestAppUser>>>().Object);
            
            return _mockUserManager.Object;
        }
    }
}