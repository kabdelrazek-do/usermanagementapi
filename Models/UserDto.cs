using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserManagementAPI.Models
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        [RegularExpression(@"^[\+]?[0-9\s\-\(\)]+$", ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Department is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-&]+$", ErrorMessage = "Department can only contain letters, spaces, hyphens, and ampersands")]
        public string Department { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Position is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Position must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-&]+$", ErrorMessage = "Position can only contain letters, spaces, hyphens, and ampersands")]
        public string Position { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Hire date is required")]
        [DataType(DataType.Date)]
        [HireDateValidation(ErrorMessage = "Hire date cannot be in the future")]
        public DateTime HireDate { get; set; } = DateTime.Now;
    }

    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? FirstName { get; set; }
        
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? LastName { get; set; }
        
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        [RegularExpression(@"^[\+]?[0-9\s\-\(\)]+$", ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }
        
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-&]+$", ErrorMessage = "Department can only contain letters, spaces, hyphens, and ampersands")]
        public string? Department { get; set; }
        
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Position must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-&]+$", ErrorMessage = "Position can only contain letters, spaces, hyphens, and ampersands")]
        public string? Position { get; set; }
        
        [DataType(DataType.Date)]
        [HireDateValidation(ErrorMessage = "Hire date cannot be in the future")]
        public DateTime? HireDate { get; set; }
        
        public bool? IsActive { get; set; }
    }

    // Custom validation attribute for hire date
    public class HireDateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is DateTime hireDate)
            {
                if (hireDate > DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage ?? "Hire date cannot be in the future");
                }
            }

            return ValidationResult.Success;
        }
    }
}
