using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class MenuItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Item name must be between 2 and 100 characters")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Image path cannot exceed 500 characters")]
        public string? ItemImage { get; set; }

        [Required(ErrorMessage = "Item price is required")]
        [Range(0.01, 1000000,
            ErrorMessage = "Price must be greater than 0")]
        public decimal ItemPrice { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Availability is required")]
        public bool Availability { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Category must be between 2 and 50 characters")]
        public string Category { get; set; } = string.Empty;

        public ICollection<OrderItem> OrderItems { get; set; }= new List<OrderItem>();
    }
}
