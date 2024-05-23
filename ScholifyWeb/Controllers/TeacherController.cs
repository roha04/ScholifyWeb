using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScholifyWeb.Models;
using SchoolLife.Data;
using System.Globalization;
using System.Security.Claims;

namespace ScholifyWeb.Controllers
{
    [Route("teacher")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ScholifyDataContext _context;

        public TeacherController(ScholifyDataContext context)
        {
            _context = context;
        }

        [HttpGet("schedules/{classId:int}")]
        public IActionResult Schedules(int classId)
        {
            var schedules = _context.Schedules
                .Include(s => s.Class)
                .Where(s => s.ClassId == classId)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToList();

            return View(schedules);
        }

        [HttpGet("gradebook/{classId:int}")]
        public IActionResult Gradebook(int classId)
        {
            var students = _context.Students
                .Where(s => s.ClassId == classId)
                .Include(s => s.Grades)
                .ThenInclude(g => g.Schedule)
                .ToList();

            if (!students.Any()) // Check if students list is empty
            {
                //return View("ErrorView", new ErrorViewModel { ErrorMessage = "No students found in this class." });
            }

            var gradebook = students.Select(s => new GradebookViewModel
            {
                StudentId = s.StudentId,
                StudentName = s.User.UserName,
                Grades = s.Grades.ToDictionary(g => g.Schedule.Date, g => g.Score)
            }).ToList();

            return View(gradebook);
        }


        [HttpGet("all-gradebooks/{teacherId:int}")]
        public IActionResult AllGradebooks(int teacherId)
        {
            try
            {
                var gradebooks = _context.Gradebooks
                    .Include(g => g.Teacher)
                    .Include(g => g.Class)
                    .Where(g => g.TeacherId == teacherId)
                    .ToList();

                return View(gradebooks);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                // For debugging purposes, you can also return a view with the exception details
                return Content($"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("my-classes")]
        public IActionResult MyClasses()
        {
            if (!int.TryParse(this.User.FindFirstValue(ClaimTypes.NameIdentifier), out int teacherId))
            {
                return Unauthorized("Invalid teacher ID");
            }

            var myClasses = _context.Classes.Where(c => c.TeacherId == teacherId).ToList();
            return View(myClasses);
        }

        [HttpGet("create-schedule/{classId:int}")]
        public IActionResult CreateSchedule(int classId)
        {
            ViewBag.ClassId = classId;
            return View(new Schedule { ClassId = classId });
        }

        [HttpPost("create-schedule/{classId:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSchedule([FromRoute] int classId, [Bind("Date,Subject,Room,StartTime,EndTime")] Schedule schedule)
        {
            ModelState.Remove("Class");
            if (ModelState.IsValid)
            {
                schedule.ClassId = classId;
                schedule.Date = schedule.Date.ToUniversalTime();
                var existingGradebook = _context.Gradebooks.FirstOrDefault(g => g.Subject == schedule.Subject && g.ClassId == classId);
                if (existingGradebook == null)
                {
                    var newGradebook = new Gradebook
                    {
                        TeacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                        ClassId = classId,
                        Subject = schedule.Subject
                    };
                    _context.Gradebooks.Add(newGradebook);
                }
                _context.Schedules.Add(schedule);
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Schedules", new { classId = classId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the schedule: " + ex.Message);
                }
            }
            ViewBag.ClassId = classId;
            return View(schedule);
        }

        [HttpGet]
        public IActionResult ViewGrades(int classId, string subject)
        {
            ViewBag.ClassId = classId;
            ViewBag.Subject = subject;
            var schedules = _context.Schedules
                .Where(s => s.ClassId == classId)
                .OrderBy(s => s.Date)
                .ToList();

            var students = _context.Students
               .Include(s => s.User)  // Make sure to include this line
               .Where(s => s.ClassId == classId)
               .ToList();


            var grades = _context.Grades
                .Include(g => g.Schedule)
                .Include(g => g.Student)
                .Where(g => g.Gradebook.ClassId == classId && g.Gradebook.Subject == subject)
                .ToList();

            var model = students.Select(student => new StudentGradesViewModel
            {
                StudentId = student.StudentId,
                StudentName = student.User?.UserName ?? "Unknown",
                GradesByDate = schedules.ToDictionary(
                    schedule => schedule.Date.Date,
                    schedule => grades.FirstOrDefault(g => g.StudentId == student.StudentId && g.Schedule.Date.Date == schedule.Date.Date)?.Score.ToString() ?? "-")
            }).ToList();


            return View(model);
        }

        [HttpPost]
        public IActionResult SaveGrades(int classId, string subject, Dictionary<int, Dictionary<DateTime, string>> grades)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var studentGrades in grades)
                    {
                        int studentId = studentGrades.Key;

                        foreach (var gradeDetail in studentGrades.Value)
                        {
                            DateTime date = gradeDetail.Key;
                            string scoreStr = gradeDetail.Value;

                            if (!int.TryParse(scoreStr, out int score) && scoreStr != "-")
                                continue;

                            var schedule = _context.Schedules.FirstOrDefault(s => s.ClassId == classId && s.Date.Date == date);
                            if (schedule == null)
                                continue;

                            var gradebook = _context.Gradebooks.FirstOrDefault(gb => gb.ClassId == classId && gb.Subject == subject);
                            if (gradebook == null)
                                continue;

                            var grade = _context.Grades
                                .FirstOrDefault(g => g.StudentId == studentId && g.ScheduleId == schedule.ScheduleId);

                            if (grade == null)
                            {
                                grade = new Grade
                                {
                                    StudentId = studentId,
                                    ScheduleId = schedule.ScheduleId,
                                    GradebookId = gradebook.GradebookId,
                                    Score = scoreStr == "-" ? 0 : (int?)score
                                };
                                _context.Grades.Add(grade);
                            }
                            else
                            {
                                grade.Score = scoreStr == "-" ? 0 : (int?)score;
                            }
                        }
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    return BadRequest("Error while saving grades");
                }
            }

            return RedirectToAction("ViewGrades", new { classId = classId, subject = subject });
        }

        [HttpGet("add-announcement")]
        public IActionResult AddAnnouncement()
        {
            var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var classes = _context.Classes.Where(c => c.TeacherId == teacherId).ToList();

            ViewBag.ClassId = new SelectList(classes, "ClassId", "ClassName");
            if (!classes.Any())
            {
                TempData["ErrorMessage"] = "No classes found. Please contact the administrator.";
            }

            return View(new Announcement());
        }


        [HttpPost("add-announcement")]
        [ValidateAntiForgeryToken]
        public IActionResult AddAnnouncement(Announcement announcement)
        {
            // Retrieve the class once and use it to validate and assign.
            var classItem = _context.Classes.Find(announcement.ClassId);
            if (classItem == null)
            {
                ModelState.AddModelError("ClassId", "The specified class does not exist.");
            }
            else
            {
                announcement.Class = classItem;  // Assign the class to the announcement
            }

            ModelState.Remove("Class");
            if (!ModelState.IsValid)
            {
                PopulateClassesDropDown(); // Reload the class list if model is not valid
                return View(announcement);
            }

            announcement.Date = DateTime.UtcNow; // Normalize the date to UTC
            _context.Announcements.Add(announcement);
            _context.SaveChanges();
            return RedirectToAction("ViewAllAnnouncements");
        }


        private void PopulateClassesDropDown()
        {
            var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var classes = _context.Classes.Where(c => c.TeacherId == teacherId).ToList();
            ViewBag.ClassId = new SelectList(classes, "ClassId", "ClassName");
        }

        [HttpGet("view-all-announcements")]
        public IActionResult ViewAllAnnouncements()
        {
            var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var myClasses = _context.Classes.Where(c => c.TeacherId == teacherId).Select(c => c.ClassId).ToList();

            var announcements = _context.Announcements
                                        .Include(a => a.Class)  // Direct access to the navigation property
                                        .Where(a => myClasses.Contains(a.ClassId))
                                        .OrderByDescending(a => a.Date)
                                        .ToList();

            return View(announcements);
        }


    }

}
