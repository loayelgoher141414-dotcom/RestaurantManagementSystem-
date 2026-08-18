using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(250, MinimumLength = 5,
            ErrorMessage = "Address must be between 5 and 250 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20,
            ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string BranchPhoneNumber { get; set; } = string.Empty;

        public ICollection<ApplicationUser> Users { get; set; }
            = new List<ApplicationUser>();

        public ICollection<Order> Orders { get; set; }
            = new List<Order>();
    }
}