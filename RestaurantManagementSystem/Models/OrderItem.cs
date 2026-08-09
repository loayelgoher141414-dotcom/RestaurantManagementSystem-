using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required(ErrorMessage = "Menu item is required")]
        public int ItemId { get; set; }

        public MenuItem Item { get; set; } = null!;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000,
            ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Item price is required")]
        [Range(0.01, 1000000,
            ErrorMessage = "Item price must be greater than 0")]
        public decimal ItemPrice { get; set; }

        [Required(ErrorMessage = "Order is required")]
        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;
    }
}