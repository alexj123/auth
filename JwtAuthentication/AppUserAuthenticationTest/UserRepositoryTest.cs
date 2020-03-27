using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AppUserAuthentication.Access.Actions;
using AppUserAuthentication.Access.Repositories;
using AppUserAuthentication.Models;
using AppUserAuthentication.Models.Identity;
using AppUserAuthentication.TokenGeneration;
using AppUserAuthenticationTest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace AppUserAuthenticationTest
{
    public class UserRepositoryTest
    {
        private UserRepository<TestAppUser> _userRepository;
        private Mock<UserManager<TestAppUser>> _mockUserManager;
        private Mock<IJwtHandler> _mockDefaultJwtHandler;

        [SetUp]
        public void Setup()
        {
            var userManager = GetUserManagerMock();

            _mockDefaultJwtHandler = new Mock<IJwtHandler>();
            
            _userRepository = new UserRepository<TestAppUser>(userManager, _mockDefaultJwtHandler.Object, new DefaultRefreshTokenGenerator());
        }

        /// <summary>
        /// Tests if the UserRepository return a correct result when the correct information is supplied.
        /// </summary>
        [Test]
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
            
            Assert.IsTrue(result.Succeeded);
            Assert.IsEmpty(result.Errors);
            Assert.AreEqual(expectedToken, result.Jwt);
        }
        
        /// <summary>
        /// Tests if the UserRepository return a result containing errors when a identity errors occur in create.
        /// </summary>
        [Test]
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
            
            var task = _userRepository.Create(testUserCorrect, "Aa1234");
            var result = task.Result;
            
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsNull(result.Jwt);
            Assert.IsNull(result.RefreshToken);
            Assert.AreEqual(expectedErrorMsg, result.Errors[0].Message);
            //assert that generate was never called
            _mockDefaultJwtHandler.Verify(service => service.Generate(null),Times.Never());
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