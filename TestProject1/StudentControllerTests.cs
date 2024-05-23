using Xunit;
using Microsoft.AspNetCore.Mvc;
using ScholifyWeb.Controllers;
using ScholifyWeb.Models;
using SchoolLife.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace ScholifyWeb.Tests
{
    public class StudentControllerTests
    {
        private readonly StudentController _controller;
        private readonly ScholifyDataContext _context;

        public StudentControllerTests()
        {
            // Set up the in-memory database
            var options = new DbContextOptionsBuilder<ScholifyDataContext>()
                .UseInMemoryDatabase(databaseName: "ScholifyTestDb")
                .Options;

            _context = new ScholifyDataContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            SeedDatabase(); // Populate the database

            _controller = new StudentController(_context);
            SetupControllerContext();
        }

        private void SetupControllerContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "TestAuthentication"));
            _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
        }

        private void SeedDatabase()
        {
            var user = new User { UserId = 1, UserName = "testuser", Password = "defaultpassword", Email = "user@test.com", Role = "Student", FirstName = "Test", LastName = "User" };
            var student = new Student { StudentId = 1, UserId = 1 };
            var announcement = new Announcement { AnnouncementId = 1, Content = "Test Announcement", Date = System.DateTime.Now, ClassId = 1 };

            _context.Users.Add(user);
            _context.Students.Add(student);
            _context.Announcements.Add(announcement);
            _context.SaveChanges();
        }

        [Fact]
        public void MyAnnouncements_ReturnsErrorView_WhenAnnouncementsAreMissing()
        {


            // Act
            var result = _controller.MyAnnouncements() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ErrorViewModel>(result.Model); 
            var errorModel = result.Model as ErrorViewModel;
            Assert.NotNull(errorModel);
            Assert.False(string.IsNullOrEmpty(errorModel.ErrorMessage));
        }
    }
}
