using DigitalSignClient.Models;
using System.Threading.Tasks;

namespace DigitalSignClient.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}
