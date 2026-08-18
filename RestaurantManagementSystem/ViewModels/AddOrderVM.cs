namespace RestaurantManagementSystem.Models
{
    public class AddOrderVM
    {
        public int UserId { get; set; }

        public int BranchId { get; set; }
        public List<Branch> Branches { get; set; } = new();

        public List<MenuItem> AvailableMenuItems { get; set; } = new();

        public List<OrderItemInput> Items { get; set; } = new();
    }

        public class OrderItemInput
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public bool IsSelected { get; set; }
    }

}
