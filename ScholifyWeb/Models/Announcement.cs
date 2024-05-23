using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScholifyWeb.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }
        public string Content { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public Class Class { get; set; }
    }

}
