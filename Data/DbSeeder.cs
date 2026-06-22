using MalikongkongNHS.Models;
using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        // ─────────────────────────────────────────
        // 1. ADMIN & CASHIER ACCOUNTS (Users table)
        // ─────────────────────────────────────────
        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    FullName = "System Administrator",
                    Username = "admin",
                    Password = "admin123",
                    Role    = "Admin"
                },
                new User
                {
                    FullName = "School Cashier",
                    Username = "cashier",
                    Password = "cashier123",
                    Role    = "Cashier"
                }
            );
            context.SaveChanges();
        }

        // ─────────────────────────────────────────
        // 2. GRADE LEVELS
        // ─────────────────────────────────────────
        if (!context.GradeLevels.Any())
        {
            context.GradeLevels.AddRange(
                new GradeLevel { Name = "Grade 7"  },
                new GradeLevel { Name = "Grade 8"  },
                new GradeLevel { Name = "Grade 9"  },
                new GradeLevel { Name = "Grade 10" },
                new GradeLevel { Name = "Grade 11" },
                new GradeLevel { Name = "Grade 12" }
            );
            context.SaveChanges();
        }

        // ─────────────────────────────────────────
        // 3. SUBJECTS
        // ─────────────────────────────────────────
        if (!context.Subjects.Any())
        {
            context.Subjects.AddRange(
                new Subject { Name = "English"              },
                new Subject { Name = "Filipino"             },
                new Subject { Name = "Mathematics"          },
                new Subject { Name = "Science"              },
                new Subject { Name = "Araling Panlipunan"   },
                new Subject { Name = "MAPEH"                },
                new Subject { Name = "TLE"                  },
                new Subject { Name = "Values Education"     },
                new Subject { Name = "Computer"             }
            );
            context.SaveChanges();
        }

        // ─────────────────────────────────────────
        // 4. TEACHERS
        // ─────────────────────────────────────────
        if (!context.Teachers.Any())
        {
            context.Teachers.AddRange(
                new Teacher
                {
                    FullName  = "Sample Teacher",
                    Email     = "teacher@mali.com",
                    Password  = "teacher123"
                }
            );
            context.SaveChanges();
        }

        // ─────────────────────────────────────────
        // 5. SECTIONS (requires at least 1 teacher)
        // ─────────────────────────────────────────
        if (!context.Sections.Any())
        {
            var teacher = context.Teachers.First();

            context.Sections.AddRange(
                new Section
                {
                    SectionName = "Hope",
                    GradeLevel  = "Grade 7",
                    Adviser     = teacher.FullName,
                    Capacity    = 50,
                    IsActive    = true
                },
                new Section
                {
                    SectionName = "Faith",
                    GradeLevel  = "Grade 7",
                    Adviser     = teacher.FullName,
                    Capacity    = 50,
                    IsActive    = true
                }
            );
            context.SaveChanges();
        }
    }
}
