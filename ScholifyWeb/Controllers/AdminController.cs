using SchoolLife.Data;
using Microsoft.AspNetCore.Mvc;
using ScholifyWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ScholifyDataContext _context;

    public AdminController(ScholifyDataContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public ActionResult Index()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    #region Users Methods

    [HttpGet("Create")]
    public ActionResult Create()
    {
        ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student", "Parent" });
        return View();
    }


    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public ActionResult Create(User user)
    {
        if (ModelState.IsValid)
        {
            user.Password = HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            if (user.Role == "Student")
            {
                var student = new Student
                {
                    UserId = user.UserId
                };
                _context.Students.Add(student);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student", "Parent" }, user.Role);
        ViewBag.Students = _context.Students.Select(s => new { s.StudentId, s.User.FirstName, s.User.LastName }).ToList();
        return View(user);
    }

    [HttpGet("Edit/{id}")]
    public ActionResult Edit(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound("Користувача не знайдено.");
        }
        ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student", "Parent" }, user.Role);
        return View(user);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, User model, string newPassword)
    {
        var existingUser = _context.Users.Find(id);
        if (existingUser == null)
        {
            return NotFound("Користувача не знайдено.");
        }

        existingUser.UserName = model.UserName;
        existingUser.Email = model.Email;
        existingUser.FirstName = model.FirstName;
        existingUser.LastName = model.LastName;
        existingUser.Role = model.Role;
        if (string.IsNullOrEmpty(newPassword))
        {
            ModelState.Remove("newPassword"); 
        }
        else
        {
            existingUser.Password = HashPassword(newPassword);  
        }

        if (ModelState.IsValid)
        {
            _context.Entry(existingUser).State = EntityState.Modified;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student", "Parent" }, existingUser.Role);
        return View(model);
    }

    [HttpGet("Delete/{id}")]
    public ActionResult Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound("Користувача не знайдено.");
        }
        return View(user);
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound("Користувача не знайдено.");
        }
        _context.Users.Remove(user);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    private string HashPassword(string password)
    {
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }
    #endregion

    #region Class Methods
    [HttpGet("view-classes")]
    public ActionResult ViewClasses()
    {
        var classes = _context.Classes
     .Include(c => c.Teacher)
     .Include(c => c.Students)
     .Select(c => new ClassViewModel
     {
         ClassId = c.ClassId,
         ClassName = c.ClassName,
         TeacherName = c.Teacher != null ? c.Teacher.UserName : "Немає",
         StudentCount = c.Students.Count
     })
     .ToList();

        return View(classes);
    }


    [HttpGet("create-class")]
    public ActionResult CreateClass()
    {
        ViewBag.Teachers = new SelectList(_context.Users.Where(u => u.Role == "Teacher"), "UserId", "UserName");
        return View(new CreateClassViewModel());
    }


    [HttpPost("create-class")]
    [ValidateAntiForgeryToken]
    public ActionResult CreateClass(Class model)
    {
        if (ModelState.IsValid)
        {
            _context.Classes.Add(model);
            _context.SaveChanges();
            return RedirectToAction("ViewClasses");
        }
        ViewBag.Teachers = new SelectList(_context.Users.Where(u => u.Role == "Teacher").ToList(), "UserId", "UserName");
        return View(model);
    }

    [HttpGet("create-class-and-students")]
    public ActionResult CreateClassAndStudents()
    {
        return View();
    }
    [HttpPost("create-class-and-students")]
    [ValidateAntiForgeryToken]
    public ActionResult CreateClassAndStudents(CreateClassViewModel model)
    {
        if (ModelState.IsValid)
        {
            var newClass = new Class { ClassName = model.ClassName, TeacherId = model.TeacherId };
            _context.Classes.Add(newClass);
            _context.SaveChanges();

            foreach (var studentViewModel in model.Students)
            {
                var user = new User
                {
                    UserName = studentViewModel.UserName,
                    FirstName = studentViewModel.FirstName,
                    LastName = studentViewModel.LastName,
                    Email = studentViewModel.Email,
                    Password = HashPassword("defaultpassword"),
                    Role = "Student"
                };
                _context.Users.Add(user);
                _context.SaveChanges(); 

                var student = new Student
                {
                    UserId = user.UserId,
                    ClassId = newClass.ClassId 
                };
                _context.Students.Add(student);
            }

            try
            {
                _context.SaveChanges(); 
                return RedirectToAction("ViewClasses");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the data. Please check the details and try again.");
            }
        }

        ViewBag.Teachers = new SelectList(_context.Users.Where(u => u.Role == "Teacher"), "UserId", "UserName");
        return View(model);
    }

    #endregion
}
