using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScholifyWeb.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required, StringLength(150)]
        public string UserName { get; set; }
        [Required, StringLength(255)]
        public string Password { get; set; }
        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; }
        [Required, StringLength(50)]
        public string FirstName { get; set; }
        [Required, StringLength(50)]
        public string LastName { get; set; }
        [Required, StringLength(50)]
        public string Role { get; set; }
    }

}
