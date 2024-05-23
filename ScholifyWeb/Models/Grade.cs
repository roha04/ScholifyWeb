using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ScholifyWeb.Models
{
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        public int ScheduleId { get; set; }  
        [ForeignKey("ScheduleId")]
        public Schedule Schedule { get; set; }
        public int? GradebookId { get; set; }
        [ForeignKey("GradebookId")]
        public Gradebook Gradebook { get; set; }
        [Required]
        public int? Score { get; set; }
    }


}
