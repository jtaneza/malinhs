using MalikongkongNHS.Models;
using MalikongkongNHS.Models.ViewModels;

namespace MalikongkongNHS.Services.Interfaces;

public interface IUserService
{
    User? Login(LoginViewModel model);

    // USER MANAGEMENT (NEW)
    List<UserM> GetAllUsers();
    UserM? GetUserById(int id);
    void CreateUser(UserM user);
    void UpdateUser(UserM user);
    void DeleteUser(int id);
}