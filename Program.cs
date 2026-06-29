using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;

using MalikongkongNHS.Repositories.Interfaces;
using MalikongkongNHS.Repositories.Implementations;

using MalikongkongNHS.Services.Interfaces;
using MalikongkongNHS.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ISectionService, SectionService>();

builder.Services.AddScoped<IGradeLevelRepository, GradeLevelRepository>();
builder.Services.AddScoped<IGradeLevelService, GradeLevelService>();

builder.Services.AddScoped<ITeacherService, TeacherService>();

builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// ── Auto-migrate + seed ────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (app.Environment.IsDevelopment())
        {
            var oldMigrations = new[]
            {
                "20260617085138_InitialCreate",
                "20260617095013_AddSubjectsAndAccountFields",
                "20260618134405_SyncFix",
                "20260618134556_RebuildMissingTables",
                "20260622031634_AddAttendance",
                "20260624134030_AddTeacherCredentials",
                "20260624152651_AddSectionSubjects",
                "20260625105955_AddCredentialsToTeacher",
                "20260626042537_AddTimeAndRoomToSectionSubject",
                "20260628095813_CreatePaymentsTable"
            };

            // Ensure __EFMigrationsHistory exists first
            // (no interpolation here — safe to keep as ExecuteSqlRaw)
#pragma warning disable EF1002
            db.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                               WHERE TABLE_NAME = '__EFMigrationsHistory')
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId]    nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32)  NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );");
#pragma warning restore EF1002

            // Use ExecuteSql with FormattableString to avoid SQL injection warning
            foreach (var mig in oldMigrations)
            {
                db.Database.ExecuteSql($@"
                    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {mig})
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ({mig}, '8.0.11');");
            }

            // Now run Migrate() — only truly new migrations will apply
            db.Database.Migrate();
            DbSeeder.Seed(db);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning("DB startup skipped due to error: {Message}", ex.Message);
    }
}
// ───────────────────────────────────────────────────────────────────


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();