using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ScholifyWeb.Models
{
    public class Gradebook
    {
        [Key]
        public int GradebookId { get; set; }
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public User Teacher { get; set; }
        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class Class { get; set; }
        [Required, StringLength(100)]
        public string Subject { get; set; }
        public ICollection<Grade> Grades { get; set; }
    }

}
