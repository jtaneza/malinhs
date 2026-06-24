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

builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// ── Auto-seed database on startup (SAFE FIXED VERSION) ─────────────
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Only run seeding in development (prevents hosting crash)
        if (app.Environment.IsDevelopment())
        {
            db.Database.EnsureCreated();
            DbSeeder.Seed(db);
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning("DB seed skipped due to error: {Message}", ex.Message);
    }
}
// ────────────────────────────────────────────────────────────────────


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