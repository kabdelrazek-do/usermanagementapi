using UserManagementAPI.Models;

namespace UserManagementAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(CreateUserDto createUserDto);
        Task<User?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
