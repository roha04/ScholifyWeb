namespace ScholifyWeb.Models
{
    public class GradebookViewModel
    {
        public int ScheduleId { get; set; } 
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public Dictionary<DateTimeOffset, int?> Grades { get; set; }
    }


}
