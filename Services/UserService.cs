using UserManagementAPI.Models;
using Microsoft.Extensions.Logging;

namespace UserManagementAPI.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users;
        private int _nextId = 1;
        private readonly object _lock = new object();
        private readonly ILogger<UserService> _logger;

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
            _users = new List<User>
            {
                new User
                {
                    Id = _nextId++,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@techhive.com",
                    PhoneNumber = "555-0101",
                    Department = "IT",
                    Position = "Software Developer",
                    HireDate = new DateTime(2023, 1, 15),
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-100),
                    UpdatedAt = null
                },
                new User
                {
                    Id = _nextId++,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@techhive.com",
                    PhoneNumber = "555-0102",
                    Department = "HR",
                    Position = "HR Manager",
                    HireDate = new DateTime(2022, 6, 20),
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-200),
                    UpdatedAt = null
                },
                new User
                {
                    Id = _nextId++,
                    FirstName = "Mike",
                    LastName = "Johnson",
                    Email = "mike.johnson@techhive.com",
                    PhoneNumber = "555-0103",
                    Department = "IT",
                    Position = "System Administrator",
                    HireDate = new DateTime(2023, 3, 10),
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-50),
                    UpdatedAt = null
                }
            };
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all active users");
                
                lock (_lock)
                {
                    var users = _users.Where(u => u.IsActive).ToList();
                    _logger.LogInformation("Retrieved {Count} active users", users.Count);
                    return Task.FromResult<IEnumerable<User>>(users);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users");
                throw;
            }
        }

        public Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid user ID provided: {Id}", id);
                    return Task.FromResult<User?>(null);
                }

                _logger.LogInformation("Retrieving user with ID: {Id}", id);
                
                lock (_lock)
                {
                    var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
                    if (user == null)
                    {
                        _logger.LogWarning("User with ID {Id} not found", id);
                    }
                    else
                    {
                        _logger.LogInformation("User with ID {Id} retrieved successfully", id);
                    }
                    return Task.FromResult(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with ID: {Id}", id);
                throw;
            }
        }

        public Task<User> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                if (createUserDto == null)
                {
                    throw new ArgumentNullException(nameof(createUserDto), "User data cannot be null");
                }

                // Sanitize input data
                var sanitizedDto = SanitizeCreateUserDto(createUserDto);

                _logger.LogInformation("Creating new user with email: {Email}", sanitizedDto.Email);

                lock (_lock)
                {
                    // Check if email already exists
                    if (_users.Any(u => u.Email.Equals(sanitizedDto.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("Attempt to create user with existing email: {Email}", sanitizedDto.Email);
                        throw new InvalidOperationException($"User with email {sanitizedDto.Email} already exists.");
                    }

                    // Validate hire date
                    if (sanitizedDto.HireDate > DateTime.Today)
                    {
                        _logger.LogWarning("Attempt to create user with future hire date: {HireDate}", sanitizedDto.HireDate);
                        throw new InvalidOperationException("Hire date cannot be in the future.");
                    }

                    var user = new User
                    {
                        Id = _nextId++,
                        FirstName = sanitizedDto.FirstName,
                        LastName = sanitizedDto.LastName,
                        Email = sanitizedDto.Email,
                        PhoneNumber = sanitizedDto.PhoneNumber,
                        Department = sanitizedDto.Department,
                        Position = sanitizedDto.Position,
                        HireDate = sanitizedDto.HireDate,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = null
                    };

                    _users.Add(user);
                    _logger.LogInformation("User created successfully with ID: {Id}", user.Id);
                    return Task.FromResult(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user with email: {Email}", createUserDto?.Email);
                throw;
            }
        }

        public Task<User?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid user ID provided for update: {Id}", id);
                    return Task.FromResult<User?>(null);
                }

                if (updateUserDto == null)
                {
                    _logger.LogWarning("Update data is null for user ID: {Id}", id);
                    return Task.FromResult<User?>(null);
                }

                _logger.LogInformation("Updating user with ID: {Id}", id);

                lock (_lock)
                {
                    var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
                    if (user == null)
                    {
                        _logger.LogWarning("User with ID {Id} not found for update", id);
                        return Task.FromResult<User?>(null);
                    }

                    // Sanitize input data
                    var sanitizedDto = SanitizeUpdateUserDto(updateUserDto);

                    // Check if email is being changed and if it already exists
                    if (!string.IsNullOrEmpty(sanitizedDto.Email) && 
                        !user.Email.Equals(sanitizedDto.Email, StringComparison.OrdinalIgnoreCase) &&
                        _users.Any(u => u.Email.Equals(sanitizedDto.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("Attempt to update user with existing email: {Email}", sanitizedDto.Email);
                        throw new InvalidOperationException($"User with email {sanitizedDto.Email} already exists.");
                    }

                    // Validate hire date if provided
                    if (sanitizedDto.HireDate.HasValue && sanitizedDto.HireDate.Value > DateTime.Today)
                    {
                        _logger.LogWarning("Attempt to update user with future hire date: {HireDate}", sanitizedDto.HireDate.Value);
                        throw new InvalidOperationException("Hire date cannot be in the future.");
                    }

                    // Update properties if provided
                    if (!string.IsNullOrEmpty(sanitizedDto.FirstName))
                        user.FirstName = sanitizedDto.FirstName;
                    
                    if (!string.IsNullOrEmpty(sanitizedDto.LastName))
                        user.LastName = sanitizedDto.LastName;
                    
                    if (!string.IsNullOrEmpty(sanitizedDto.Email))
                        user.Email = sanitizedDto.Email;
                    
                    if (!string.IsNullOrEmpty(sanitizedDto.PhoneNumber))
                        user.PhoneNumber = sanitizedDto.PhoneNumber;
                    
                    if (!string.IsNullOrEmpty(sanitizedDto.Department))
                        user.Department = sanitizedDto.Department;
                    
                    if (!string.IsNullOrEmpty(sanitizedDto.Position))
                        user.Position = sanitizedDto.Position;
                    
                    if (sanitizedDto.HireDate.HasValue)
                        user.HireDate = sanitizedDto.HireDate.Value;
                    
                    if (sanitizedDto.IsActive.HasValue)
                        user.IsActive = sanitizedDto.IsActive.Value;

                    user.UpdatedAt = DateTime.Now;

                    _logger.LogInformation("User with ID {Id} updated successfully", id);
                    return Task.FromResult<User?>(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID: {Id}", id);
                throw;
            }
        }

        public Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid user ID provided for deletion: {Id}", id);
                    return Task.FromResult(false);
                }

                _logger.LogInformation("Deleting user with ID: {Id}", id);

                lock (_lock)
                {
                    var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
                    if (user == null)
                    {
                        _logger.LogWarning("User with ID {Id} not found for deletion", id);
                        return Task.FromResult(false);
                    }

                    user.IsActive = false;
                    user.UpdatedAt = DateTime.Now;

                    _logger.LogInformation("User with ID {Id} deleted successfully", id);
                    return Task.FromResult(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID: {Id}", id);
                throw;
            }
        }

        public Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(department))
                {
                    _logger.LogWarning("Empty or null department provided for search");
                    return Task.FromResult<IEnumerable<User>>(Enumerable.Empty<User>());
                }

                var sanitizedDepartment = department.Trim();
                _logger.LogInformation("Retrieving users by department: {Department}", sanitizedDepartment);

                lock (_lock)
                {
                    var users = _users.Where(u => 
                        u.Department.Equals(sanitizedDepartment, StringComparison.OrdinalIgnoreCase) && u.IsActive).ToList();
                    
                    _logger.LogInformation("Retrieved {Count} users from department {Department}", users.Count, sanitizedDepartment);
                    return Task.FromResult<IEnumerable<User>>(users);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users by department: {Department}", department);
                throw;
            }
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Empty or null email provided for search");
                    return Task.FromResult<User?>(null);
                }

                var sanitizedEmail = email.Trim().ToLowerInvariant();
                _logger.LogInformation("Retrieving user by email: {Email}", sanitizedEmail);

                lock (_lock)
                {
                    var user = _users.FirstOrDefault(u => 
                        u.Email.Equals(sanitizedEmail, StringComparison.OrdinalIgnoreCase) && u.IsActive);
                    
                    if (user == null)
                    {
                        _logger.LogWarning("User with email {Email} not found", sanitizedEmail);
                    }
                    else
                    {
                        _logger.LogInformation("User with email {Email} retrieved successfully", sanitizedEmail);
                    }
                    
                    return Task.FromResult(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user by email: {Email}", email);
                throw;
            }
        }

        private CreateUserDto SanitizeCreateUserDto(CreateUserDto dto)
        {
            return new CreateUserDto
            {
                FirstName = dto.FirstName?.Trim() ?? string.Empty,
                LastName = dto.LastName?.Trim() ?? string.Empty,
                Email = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty,
                PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
                Department = dto.Department?.Trim() ?? string.Empty,
                Position = dto.Position?.Trim() ?? string.Empty,
                HireDate = dto.HireDate
            };
        }

        private UpdateUserDto SanitizeUpdateUserDto(UpdateUserDto dto)
        {
            return new UpdateUserDto
            {
                FirstName = dto.FirstName?.Trim(),
                LastName = dto.LastName?.Trim(),
                Email = dto.Email?.Trim().ToLowerInvariant(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Department = dto.Department?.Trim(),
                Position = dto.Position?.Trim(),
                HireDate = dto.HireDate,
                IsActive = dto.IsActive
            };
        }
    }
}
