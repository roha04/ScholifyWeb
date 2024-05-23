using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScholifyWeb.Models;
using SchoolLife.Data;
using System.Linq;
using System.Security.Claims;

namespace ScholifyWeb.Controllers
{
    [Route("student")]
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ScholifyDataContext _context;

        public StudentController(ScholifyDataContext context)
        {
            _context = context;
        }

        [HttpGet("my-grades")]
        public IActionResult MyGrades()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var studentId = _context.Students
                                        .Where(s => s.UserId == userId)
                                        .Select(s => s.StudentId)
                                        .FirstOrDefault();

                if (studentId == 0)
                {
                    return View("ErrorView", new ErrorViewModel { ErrorMessage = "Student not found." });
                }

                var grades = _context.Grades
                                     .Include(g => g.Schedule)
                                     .ThenInclude(s => s.Class)
                                     .Where(g => g.StudentId == studentId)
                                     .OrderBy(g => g.Schedule.Date)
                                     .ToList();

                if (!grades.Any())
                {
                    return View("ErrorView", new ErrorViewModel { ErrorMessage = "No grades available." });
                }

                return View(grades);
            }
            catch (Exception ex)
            {
                return View("ErrorView", new ErrorViewModel { ErrorMessage = "An error occurred while fetching grades." });
            }
        }


        [HttpGet("my-announcements")]
        public IActionResult MyAnnouncements()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var studentId = _context.Students
                                        .Where(s => s.UserId == userId)
                                        .Select(s => s.StudentId)
                                        .FirstOrDefault();

                if (studentId == 0)
                {
                    return View("ErrorView", new ErrorViewModel { ErrorMessage = "Student not found." });
                }

                var classId = _context.Students
                                      .Where(s => s.StudentId == studentId)
                                      .Select(s => s.ClassId)
                                      .FirstOrDefault();

                if (classId == null)
                {
                    return View("ErrorView", new ErrorViewModel { ErrorMessage = "Class not found for the student." });
                }

                var announcements = _context.Announcements
                                            .Include(a => a.Class)
                                            .Where(a => a.ClassId == classId)
                                            .OrderByDescending(a => a.Date)
                                            .ToList();

                if (!announcements.Any())
                {
                    return View("ErrorView", new ErrorViewModel { ErrorMessage = "No announcements available." });
                }

                return View(announcements);
            }
            catch (Exception ex)
            {
                return View("ErrorView", new ErrorViewModel { ErrorMessage = "An error occurred while fetching announcements." });
            }
        }

    }
}
