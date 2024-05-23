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
using System.Collections.Generic;

namespace ScholifyWeb.Tests
{
    public class ParentControllerTests
    {
        private readonly ParentController _controller;
        private readonly ScholifyDataContext _context;

        public ParentControllerTests()
        {
            var options = new DbContextOptionsBuilder<ScholifyDataContext>()
                .UseInMemoryDatabase(databaseName: "ScholifyTestDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new ScholifyDataContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            SeedDatabase();

            _controller = new ParentController(_context);
            SetupControllerContext();
        }

        private void SeedDatabase()
        {
            var user = new User
            {
                UserId = 1,
                UserName = "ParentUser",
                Email = "parent@example.com",
                Password = "hashedpassword",
                Role = "Parent",
                FirstName = "Parent",
                LastName = "Doe"
            };

            var studentUser = new User
            {
                UserId = 2,
                UserName = "StudentUser",
                Email = "student@example.com",
                Password = "hashedpassword",
                Role = "Student",
                FirstName = "Student",
                LastName = "Doe"
            };

            var student = new Student
            {
                StudentId = 1,
                UserId = 2,
                User = studentUser
            };

            _context.Users.Add(user);
            _context.Users.Add(studentUser);
            _context.Students.Add(student);
            _context.SaveChanges();
        }

        private void SetupControllerContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Parent"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public void Grades_UserNotFound_ReturnsErrorView()
        {
            // Arrange
            var user = _context.Users.FirstOrDefault(u => u.UserId == 1);
            _context.Users.Remove(user);
            _context.SaveChanges();

            // Act
            var result = _controller.Grades() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ErrorView", result.ViewName);
            var model = Assert.IsType<ErrorViewModel>(result.Model);
            Assert.Equal("User not found.", model.ErrorMessage);
        }

        [Fact]
        public void Grades_StudentNotFound_ReturnsErrorView()
        {
            // Arrange
            var student = _context.Students.FirstOrDefault(s => s.StudentId == 1);
            _context.Students.Remove(student);
            _context.SaveChanges();

            // Act
            var result = _controller.Grades() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ErrorView", result.ViewName);
            var model = Assert.IsType<ErrorViewModel>(result.Model);
            Assert.Equal("Student not found.", model.ErrorMessage);
        }

        [Fact]
        public void Grades_NoGradesFound_ReturnsErrorView()
        {
            // Act
            var result = _controller.Grades() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ErrorView", result.ViewName);
            var model = Assert.IsType<ErrorViewModel>(result.Model);
            Assert.Equal("No grades available.", model.ErrorMessage);
        }

        [Fact]
        public void Grades_Success_ReturnsGradesView()
        {
            // Arrange
            var student = _context.Students.First();
            var classEntity = new Class { ClassId = 1, ClassName = "Math" };
            var schedule = new Schedule
            {
                ScheduleId = 1,
                ClassId = 1,
                Class = classEntity,
                Date = DateTimeOffset.Now,
                Subject = "Mathematics",
                Room = "101",
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(10)
            };
            var grade = new Grade
            {
                GradeId = 1,
                StudentId = student.StudentId,
                ScheduleId = schedule.ScheduleId,
                Schedule = schedule,
                Score = 95
            };

            _context.Classes.Add(classEntity);
            _context.Schedules.Add(schedule);
            _context.Grades.Add(grade);
            _context.SaveChanges();

            // Act
            var result = _controller.Grades() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual("ErrorView", result.ViewName);
            var model = Assert.IsType<List<Grade>>(result.Model);
            Assert.Single(model);
        }
    }
}
