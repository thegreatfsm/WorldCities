using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Frameworks;
using WorldCities.Controllers;
using WorldCities.Data;
using WorldCities.Data.Models;
using Xunit;
using Moq;
using System;
using IdentityModel;

namespace WorldCities.Tests
{
    public class SeedController_Tests
    {
        [Fact]
        public async void CreateDefaultUsers()
        {
            // Arrange
            // Create the options instances required by ApplicationDbContext
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "WorldCities")
                .Options;
            var storeOptions = Options.Create(new OperationalStoreOptions());

            // Create an IWebHost enviornment mock instance
            var mockEnv = new Mock<IWebHostEnvironment>().Object;

            // define the variables for later use
            ApplicationUser user_Admin = null;
            ApplicationUser user_User = null;
            ApplicationUser user_NotExisting = null;

            // Act
            // Create an ApplicationDbContext instance using the in-memory DB
            using (var context = new ApplicationDbContext(options, storeOptions))
            {
                // Create a RoleManager instance
                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = IdentityHelper.GetRoleManager(roleStore);
                // Create a UserManager instance
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = IdentityHelper.GetUserManager(userStore);
                // Create a SeedController Instance
                var controller = new SeedController(context, mockEnv, roleManager, userManager);
                // Execure CreateDefaultUsers method
                await controller.CreateDefaultUsers();

                // Retrieve the users
                user_Admin = await userManager.FindByEmailAsync("admin@email.com");
                user_User = await userManager.FindByEmailAsync("user@email.com");
                user_NotExisting = await userManager.FindByEmailAsync("notexisting@email.com");
            }


            // Assert
            Assert.True(user_Admin != null && user_User != null && user_NotExisting == null);
        }
    }
}
