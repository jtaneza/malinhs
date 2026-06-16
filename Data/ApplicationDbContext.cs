using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Models;
using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Users
        public DbSet<User> Users { get; set; }

        // Students
        public DbSet<Student> Students { get; set; }

        // Teachers
        public DbSet<Teacher> Teachers { get; set; }

        // Sections
        public DbSet<Section> Sections { get; set; }
    }
}