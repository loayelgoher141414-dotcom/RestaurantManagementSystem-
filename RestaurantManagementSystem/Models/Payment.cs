using System.ComponentModel.DataAnnotations;

namespace RestaurantManagementSystem.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Payment amount is required")]
        [Range(0.01, 10000000,
            ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(30,
            ErrorMessage = "Payment method cannot exceed 30 characters")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment status is required")]
        [StringLength(30,
            ErrorMessage = "Payment status cannot exceed 30 characters")]
        public string PaymentStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Order is required")]
        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;
    }
}