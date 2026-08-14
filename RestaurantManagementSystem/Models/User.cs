using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20,
            ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string UserPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100,
            ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [StringLength(250,
            ErrorMessage = "Address cannot exceed 250 characters")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Employee|Customer)$",
            ErrorMessage = "Role must be Employee or Customer")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch is required")]
        public int BranchId { get; set; }

        public Branch? Branch { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}