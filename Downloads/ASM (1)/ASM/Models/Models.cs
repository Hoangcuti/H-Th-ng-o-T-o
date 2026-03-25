using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
    }

    public class FoodItem
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; } // e.g., "Burger", "Drink"
        public bool IsAvailable { get; set; } = true;
    }

    public class Combo
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public virtual ICollection<ComboDetail> ComboDetails { get; set; } = new List<ComboDetail>();
    }

    public class ComboDetail 
    {
        public int Id { get; set; }
        public int ComboId { get; set; }
        public virtual Combo Combo { get; set; }    
        public int FoodItemId { get; set; }
        public virtual FoodItem FoodItem { get; set; }
        public int Quantity { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    public enum OrderStatus
    {
        Pending,
        Delivering,
        Delivered,
        Cancelled
    }

    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        // Can order distinct FoodItems OR Combos via this structure? 
        // For simplicity, let's say OrderDetail points to FoodItems. 
        // Or if we need Combos in Order, we need a flexible structure.
        // Simplest: Flatten Combos into items or have generic Product ID.
        // Or specific ComboId nullable.
        
        public int? FoodItemId { get; set; }
        public virtual FoodItem? FoodItem { get; set; }
        
        public int? ComboId { get; set; }
        public virtual Combo? Combo { get; set; }
        
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Snapshot price at time of order
    }
}
