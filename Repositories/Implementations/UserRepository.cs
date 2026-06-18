using MalikongkongNHS.Data;
using MalikongkongNHS.Models;
using MalikongkongNHS.Repositories.Interfaces;
using System.Linq;

namespace MalikongkongNHS.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // LOGIN
    public User? GetByUsername(string username)
    {
        return _context.Users
            .FirstOrDefault(x => x.Username == username);
    }

    // GET ALL USERS
    public List<User> GetAll()
    {
        return _context.Users.ToList();
    }

    // GET BY ID
    public User? GetById(int id)
    {
        return _context.Users.FirstOrDefault(x => x.UserId == id);
    }

    // CREATE USER
    public void Create(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    // UPDATE USER
    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    // DELETE USER
    public void Delete(int id)
    {
        var user = _context.Users.FirstOrDefault(x => x.UserId == id);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}