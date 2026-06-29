using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Models;
using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // USERS (ADMIN MODULE)
        public DbSet<User> Users { get; set; }

        // SYSTEM MODULES
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<GradeLevel> GradeLevels { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // DASHBOARD MODULES
        public DbSet<ClassEntity> Classes { get; set; }
        public DbSet<ClassStudent> ClassStudents { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SectionSubject> SectionSubjects { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}