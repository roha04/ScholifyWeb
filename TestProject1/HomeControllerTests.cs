using Xunit;
using Microsoft.AspNetCore.Mvc;
using ScholifyWeb.Controllers;
using ScholifyWeb.Models;
using System.Diagnostics;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace ScholifyWeb.Tests
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;
        private readonly Mock<ILogger<HomeController>> _loggerMock;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test_trace_identifier";  // Mocking TraceIdentifier
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }


        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewResult_WithModelError()
        {
            // Arrange
            // Ensure HttpContext is set up if not done in the constructor
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.TraceIdentifier = "test_trace_identifier";

            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<ErrorViewModel>(result.Model);
            Assert.False(string.IsNullOrEmpty(model.RequestId), "RequestId should not be null or empty");
        }

    }
}
