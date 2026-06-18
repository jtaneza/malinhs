using MalikongkongNHS.Models;

namespace MalikongkongNHS.Repositories.Interfaces;

public interface IUserRepository
{
    // LOGIN
    User? GetByUsername(string username);

    // CRUD (USER MANAGEMENT)
    List<User> GetAll();
    User? GetById(int id);
    void Create(User user);
    void Update(User user);
    void Delete(int id);
}