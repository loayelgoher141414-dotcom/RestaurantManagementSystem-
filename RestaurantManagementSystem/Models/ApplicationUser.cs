using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Name must be between 3 and 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250,
            ErrorMessage = "Address cannot exceed 250 characters")]
        public string? Address { get; set; }

        public decimal? Salary { get; set; }

        public int? BranchId { get; set; }

        public Branch? Branch { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}