using System.ComponentModel.DataAnnotations;

namespace EntertainmentGuild.Models
{
    public class RecommendedTopProduct
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Category { get; set; } = "";

        [Required]
        public string SubCategory { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public byte[]? ImageData { get; set; }
        public string? ImageMimeType { get; set; }


        [Required]
        public decimal Price { get; set; }
    }
}
