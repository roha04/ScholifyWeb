using Microsoft.EntityFrameworkCore;
using ScholifyWeb.Models;

namespace SchoolLife.Data
{
    public class ScholifyDataContext : DbContext
    {
        public ScholifyDataContext(DbContextOptions<ScholifyDataContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        public DbSet<Gradebook> Gradebooks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Class>()
                .HasMany(c => c.Students)
                .WithOne(s => s.Class)
                .HasForeignKey(s => s.ClassId);

            modelBuilder.Entity<Schedule>()
                .Property(s => s.Date)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Class)
                .WithMany()
                .HasForeignKey(s => s.ClassId);

            modelBuilder.Entity<Grade>()
                   .HasOne(g => g.Student)
                   .WithMany(s => s.Grades)
                   .HasForeignKey(g => g.StudentId);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Schedule)
                .WithMany() // If Schedule doesn't directly reference Grades
                .HasForeignKey(g => g.ScheduleId);

            modelBuilder.Entity<Grade>()
                .HasOne(g => g.Gradebook)
                .WithMany(gb => gb.Grades)
                .HasForeignKey(g => g.GradebookId)
                .IsRequired(false); // Since GradebookId is nullable
        }
    }
}
