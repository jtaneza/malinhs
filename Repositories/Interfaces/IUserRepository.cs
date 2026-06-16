using MalikongkongNHS.Models;

namespace MalikongkongNHS.Repositories.Interfaces;

public interface IUserRepository
{
    User? GetByUsername(string username);
}