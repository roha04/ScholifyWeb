namespace ScholifyWeb.Models
{
    public class GradeViewModel
    {
        public int GradeId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTimeOffset ScheduleDate { get; set; }
        public int Score { get; set; }
        public int ScheduleId { get; set; }
        public int GradebookId { get; set; }
    }

}
