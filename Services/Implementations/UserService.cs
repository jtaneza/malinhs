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
    // ======================
    // USER MANAGEMENT (NEW)
    // ======================

    public List<UserM> GetAllUsers()
{
    return _userRepository.GetAll()
        .Select(u => new UserM
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Username = u.Username,
            Password = u.Password,
            Role = u.Role
        }).ToList();
}

public UserM? GetUserById(int id)
{
    var user = _userRepository.GetById(id);
    if (user == null) return null;

    return new UserM
    {
        UserId = user.UserId,
        FullName = user.FullName,
        Username = user.Username,
        Password = user.Password,
        Role = user.Role
    };
}

public void CreateUser(UserM model)
{
    var user = new User
    {
        FullName = model.FullName,   // ✅ ADD THIS
        Username = model.Username,
        Password = model.Password,
        Role = model.Role
    };

    _userRepository.Create(user);
}

    public void UpdateUser(UserM model)
    {
        var user = _userRepository.GetById(model.UserId);
        if (user == null) return;

        user.FullName = model.FullName;   // ✅ ADD THIS
        user.Username = model.Username;
        user.Password = model.Password;
        user.Role = model.Role;

        _userRepository.Update(user);
    }

    public void DeleteUser(int id)
    {
        _userRepository.Delete(id);
    }
}
