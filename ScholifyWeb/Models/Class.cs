using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScholifyWeb.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }
        [Required, StringLength(100)]
        public string ClassName { get; set; }
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public User Teacher { get; set; }
        public ICollection<Student> Students { get; set; }

        public ICollection<Announcement> Announcements { get; set; }
    }


}
