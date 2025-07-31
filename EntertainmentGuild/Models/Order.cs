using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EntertainmentGuild.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string ShippingMethod { get; set; }
        public string PaymentMethod { get; set; }
        public int AddressId { get; set; }

        public List<OrderItem> Items { get; set; } = new();

        public string UserEmail { get; set; }
        [NotMapped]
        public ICollection<OrderItem> OrderItems => Items;
        [NotMapped]
        public DateTime OrderDate => CreatedAt;

        public string? ShippingStatus { get; set; }

        public string? Courier { get; set; }

        public string? TrackingNumber { get; set; }

        public string? Remarks { get; set; }
    }
}
