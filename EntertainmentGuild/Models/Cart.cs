using System;
using System.ComponentModel.DataAnnotations;

namespace EntertainmentGuild.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int ProductId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;

        // 可选：导航属性
        public Product Product { get; set; }
    }
}
