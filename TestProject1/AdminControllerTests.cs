using Xunit;
using Microsoft.EntityFrameworkCore;
using ScholifyWeb.Controllers;
using ScholifyWeb.Models;
using SchoolLife.Data;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ScholifyWeb.Tests
{
    public class AdminControllerTests
    {
        private readonly AdminController _controller;
        private readonly ScholifyDataContext _context;

        public AdminControllerTests()
        {
            var options = new DbContextOptionsBuilder<ScholifyDataContext>()
                .UseInMemoryDatabase(databaseName: "ScholifyTestDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new ScholifyDataContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            SeedDatabase();

            _controller = new AdminController(_context);
            SetupControllerContext();
        }

        private void SeedDatabase()
        {
            _context.Users.Add(new User
            {
                UserId = 1,
                UserName = "AdminUser",
                Email = "admin@example.com",
                Password = "hashedpassword",
                Role = "Admin",
                FirstName = "Admin",
                LastName = "User"
            });
            _context.SaveChanges();
        }

        private void SetupControllerContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public void Index_ReturnsViewResult_WithAListOfUsers()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<List<User>>(result.Model);
            Assert.Single(model); // Check if there is exactly one user in the list
        }

        [Fact]
        public void Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var newUser = new User
            {
                UserName = "NewUser",
                Email = "newuser@example.com",
                Password = "password123",
                Role = "Student",
                FirstName = "First",
                LastName = "Last"
            };

            // Act
            var result = _controller.Create(newUser) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName); // Check redirection to Index after creation
        }


        [Fact]
        public void Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var user = _context.Users.First();
            user.UserName = "UpdatedUser";
            user.FirstName = "Updated";  // Ensure field is set
            user.LastName = "User";      // Ensure field is set

            // Act
            var result = _controller.Edit(user.UserId, user, "newPassword123") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

    }
}
