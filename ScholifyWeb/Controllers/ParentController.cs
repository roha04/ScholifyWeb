using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholifyWeb.Models;
using SchoolLife.Data;
using System.Security.Claims;

namespace ScholifyWeb.Controllers
{
    [Authorize(Roles = "Parent")]
    public class ParentController : Controller
    {
        private readonly ScholifyDataContext _context;

        public ParentController(ScholifyDataContext context)
        {
            _context = context;
        }

        [HttpGet("grades")]
        public IActionResult Grades()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Отримати дані користувача
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

                if (user == null)
                {
                    return View("ErrorView", new ErrorViewModel { ErrorMessage = "User not found." });
                }

                // Отримати дані студента
                var student = _context.Students
                                      .Include(s => s.User)
                                      .FirstOrDefault(s => s.User.LastName == user.LastName);

                if (student == null)
                {
                    return View("ErrorView", new ErrorViewModel { ErrorMessage = "Student not found." });
                }

                // Перевірити, чи прізвище батька збігається з прізвищем студента
                if (user.LastName == student.User.LastName)
                {
                    var grades = _context.Grades
                                         .Include(g => g.Schedule)
                                         .ThenInclude(s => s.Class)
                                         .Where(g => g.StudentId == student.StudentId)
                                         .OrderBy(g => g.Schedule.Date)
                                         .ToList();

                    if (!grades.Any())
                    {
                        return View("ErrorView", new ErrorViewModel { ErrorMessage = "No grades available." });
                    }

                    return View(grades);
                }
                else
                {
                    return View("ErrorView", new ErrorViewModel { ErrorMessage = "You are not authorized to view this student's grades." });
                }
            }
            catch (Exception ex)
            {
                return View("ErrorView", new ErrorViewModel { ErrorMessage = "An error occurred while fetching grades." });
            }
        }

    }
}
