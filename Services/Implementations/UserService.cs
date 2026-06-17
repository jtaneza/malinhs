using MalikongkongNHS.Data;
using MalikongkongNHS.Models;
using MalikongkongNHS.Models.ViewModels;
using MalikongkongNHS.Repositories.Interfaces;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;

    public UserService(IUserRepository userRepository, ApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public User? Login(LoginViewModel model)
    {
        // 1. Check Users table (Admin/Cashier) by Username
        var user = _userRepository.GetByUsername(model.Username);

        if (user != null && user.Password == model.Password)
            return user;

        // 2. Check Teachers table by Email
        var teacher = _context.Teachers
            .FirstOrDefault(t => t.Email == model.Username);

        if (teacher != null && teacher.Password == model.Password)
        {
            return new User
            {
                UserId = teacher.TeacherId,
                Username = teacher.Email ?? teacher.FullName,
                Password = teacher.Password ?? string.Empty,
                Role = "Teacher"
            };
        }

        // 3. Check Students table by Email
        var student = _context.Students
            .FirstOrDefault(s => s.Email == model.Username);

        if (student != null && student.Password == model.Password)
        {
            return new User
            {
                UserId = student.StudentId,
                Username = student.Email ?? $"{student.FirstName} {student.LastName}",
                Password = student.Password ?? string.Empty,
                Role = "Student"
            };
        }

        return null;
    }
}