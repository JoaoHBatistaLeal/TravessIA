using TravessIA.Application.DTOs.Auth;

namespace TravessIA.Application.Services.Interfaces;

public interface IAuthService
{
    AuthResponse? Login(LoginRequest request);
    AuthResponse Register(RegisterRequest request);
}
