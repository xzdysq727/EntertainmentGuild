using System.ComponentModel.DataAnnotations;

namespace EntertainmentGuild.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string SubCategory { get; set; }

        public string? Description { get; set; }

        
        public string ? ImageUrl { get; set; } = string.Empty;

    }
}
