using MalikongkongNHS.Models;
using MalikongkongNHS.Models.ViewModels;
using MalikongkongNHS.Repositories.Interfaces;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public User? Login(LoginViewModel model)
    {
        var user =
            _userRepository.GetByUsername(model.Username);

        if (user == null)
            return null;

        if (user.Password != model.Password)
            return null;

        return user;
    }
}