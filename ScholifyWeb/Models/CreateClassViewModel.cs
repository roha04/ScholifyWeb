namespace ScholifyWeb.Models
{
    public class CreateClassViewModel
    {
        public string ClassName { get; set; }
        public int TeacherId { get; set; } 
        public List<StudentViewModel> Students { get; set; } = new List<StudentViewModel>();
    }
}
