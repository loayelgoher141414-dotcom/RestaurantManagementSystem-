using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "User is required")]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Total price is required")]
        [Range(0.01, 10000000,
            ErrorMessage = "Total price must be greater than 0")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Order status is required")]
        [StringLength(30,
            ErrorMessage = "Order status cannot exceed 30 characters")]
        public string OrderStatus { get; set; } = string.Empty;

        public Payment? Payment { get; set; }

        [Required(ErrorMessage = "Branch is required")]
        public int BranchId { get; set; }

        public Branch Branch { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; }= new List<OrderItem>();
    }
}