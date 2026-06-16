using MalikongkongNHS.Models;
using MalikongkongNHS.Models.ViewModels;

namespace MalikongkongNHS.Services.Interfaces;

public interface IUserService
{
    User? Login(LoginViewModel model);
}