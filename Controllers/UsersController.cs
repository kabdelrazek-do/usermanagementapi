using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;
using Microsoft.Extensions.Logging;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        /// <returns>List of all active users</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                _logger.LogInformation("GET /api/users - Retrieving all users");
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users");
                return StatusCode(500, new { error = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("GET /api/users/{Id} - Invalid ID provided: {Id}", id, id);
                    return BadRequest(new { error = "User ID must be a positive integer" });
                }

                _logger.LogInformation("GET /api/users/{Id} - Retrieving user", id);
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                {
                    _logger.LogWarning("GET /api/users/{Id} - User not found", id);
                    return NotFound(new { error = $"User with ID {id} not found." });
                }
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user with ID: {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the user" });
            }
        }

        /// <summary>
        /// Get users by department
        /// </summary>
        /// <param name="department">Department name</param>
        /// <returns>List of users in the specified department</returns>
        [HttpGet("department/{department}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByDepartment(string department)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(department))
                {
                    _logger.LogWarning("GET /api/users/department/{Department} - Empty department provided", department);
                    return BadRequest(new { error = "Department name cannot be empty" });
                }

                _logger.LogInformation("GET /api/users/department/{Department} - Retrieving users by department", department);
                var users = await _userService.GetUsersByDepartmentAsync(department);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users by department: {Department}", department);
                return StatusCode(500, new { error = "An error occurred while retrieving users by department" });
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User details</returns>
        [HttpGet("email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("GET /api/users/email/{Email} - Empty email provided", email);
                    return BadRequest(new { error = "Email cannot be empty" });
                }

                _logger.LogInformation("GET /api/users/email/{Email} - Retrieving user by email", email);
                var user = await _userService.GetUserByEmailAsync(email);
                
                if (user == null)
                {
                    _logger.LogWarning("GET /api/users/email/{Email} - User not found", email);
                    return NotFound(new { error = $"User with email {email} not found." });
                }
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user by email: {Email}", email);
                return StatusCode(500, new { error = "An error occurred while retrieving the user" });
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user details</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> CreateUser(CreateUserDto createUserDto)
        {
            try
            {
                if (createUserDto == null)
                {
                    _logger.LogWarning("POST /api/users - Null user data provided");
                    return BadRequest(new { error = "User data cannot be null" });
                }

                // Check ModelState validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("POST /api/users - Validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                _logger.LogInformation("POST /api/users - Creating new user with email: {Email}", createUserDto.Email);
                
                var user = await _userService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("POST /api/users - Business rule violation: {Message}", ex.Message);
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning("POST /api/users - Invalid argument: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user with email: {Email}", createUserDto?.Email);
                return StatusCode(500, new { error = "An error occurred while creating the user" });
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">User update data</param>
        /// <returns>Updated user details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("PUT /api/users/{Id} - Invalid ID provided: {Id}", id, id);
                    return BadRequest(new { error = "User ID must be a positive integer" });
                }

                if (updateUserDto == null)
                {
                    _logger.LogWarning("PUT /api/users/{Id} - Null update data provided", id);
                    return BadRequest(new { error = "Update data cannot be null" });
                }

                // Check ModelState validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("PUT /api/users/{Id} - Validation failed: {Errors}", id, string.Join(", ", errors));
                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                _logger.LogInformation("PUT /api/users/{Id} - Updating user", id);
                
                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                {
                    _logger.LogWarning("PUT /api/users/{Id} - User not found", id);
                    return NotFound(new { error = $"User with ID {id} not found." });
                }
                
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("PUT /api/users/{Id} - Business rule violation: {Message}", id, ex.Message);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID: {Id}", id);
                return StatusCode(500, new { error = "An error occurred while updating the user" });
            }
        }

        /// <summary>
        /// Delete a user (soft delete)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("DELETE /api/users/{Id} - Invalid ID provided: {Id}", id, id);
                    return BadRequest(new { error = "User ID must be a positive integer" });
                }

                _logger.LogInformation("DELETE /api/users/{Id} - Deleting user", id);
                
                var success = await _userService.DeleteUserAsync(id);
                if (!success)
                {
                    _logger.LogWarning("DELETE /api/users/{Id} - User not found", id);
                    return NotFound(new { error = $"User with ID {id} not found." });
                }
                
                return Ok(new { message = $"User with ID {id} has been deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID: {Id}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the user" });
            }
        }
    }
}
