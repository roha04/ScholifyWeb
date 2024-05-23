using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SchoolLife.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using ScholifyWeb.Models;

namespace SchoolLife.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ScholifyDataContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ScholifyDataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("login")]
        public IActionResult Login() =>  RedirectToAction("Index", "Home");

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password) 
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == username);

            if (existingUser == null || password != existingUser.Password/*!VerifyPassword(attemptedPassword: password, existingUser.Password)*/)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View("Index");
            }

            // Set up the claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, existingUser.UserName),
                new Claim(ClaimTypes.NameIdentifier, existingUser.UserId.ToString()),
                new Claim(ClaimTypes.Role, existingUser.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (existingUser.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (existingUser.Role == "Student")
            {
                return RedirectToAction("MyAnnouncements", "Student");
            }
            else if (existingUser.Role == "Teacher")
            {
                return RedirectToAction("MyClasses", "Teacher");
            }
            else if (existingUser.Role == "Parent")
            {
                return RedirectToAction("Grades", "Parent");
            }
            else 
            {
                return RedirectToAction("Index", "Home");
            }
            //return RedirectToAction("Index", "Home");
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] User user)
        {
            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                ModelState.AddModelError(string.Empty, "User already exists.");
                return View(user);
            }

            var newUser = new User
            {
                UserName = user.UserName,
                Password = HashPassword(user.Password),
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, newUser.UserName),
                new Claim(ClaimTypes.Role, newUser.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
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

        private bool VerifyPassword(string attemptedPassword, string storedHash)
        {
            var parts = storedHash.Split('.', 2);
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hashed = Convert.FromBase64String(parts[1]);

            var attemptedHash = KeyDerivation.Pbkdf2(
                password: attemptedPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return hashed.SequenceEqual(attemptedHash);
        }
    }
}