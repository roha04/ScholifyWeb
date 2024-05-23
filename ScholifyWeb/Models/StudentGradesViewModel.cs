namespace ScholifyWeb.Models
{
    public class StudentGradesViewModel
    {
        public int StudentId { get; set; } 
        public string StudentName { get; set; }
        public Dictionary<DateTime, string> GradesByDate { get; set; } 
    }


}
