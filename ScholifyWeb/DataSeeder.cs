using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ScholifyWeb.Models;
using SchoolLife.Data;
using System.Security.Cryptography;

public class DataSeeder
{
    public static async Task CreateDefaultAdminUser(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ScholifyDataContext>();

            if (!context.Users.Any(u => u.UserName == "admin"))
            {
                var adminUser = new User
                {
                    UserName = "admin",
                    Password = HashPassword("admin"),
                    FirstName = "admin",
                    LastName = "admin",
                    Email = "admin@example.com",
                    Role = "Admin"
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }

    private static string HashPassword(string password)
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
}
