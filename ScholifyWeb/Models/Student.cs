using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ScholifyWeb.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } // Student's user account
        public int? ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class Class { get; set; } // Class to which the student belongs
        public ICollection<Grade> Grades { get; set; } // Grades for the student
    }
}
