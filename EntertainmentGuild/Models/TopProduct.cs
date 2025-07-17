using System.ComponentModel.DataAnnotations;

namespace EntertainmentGuild.Models.Admin
{
    public class TopProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 99999.99)]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string SubCategory { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Image URL")]
        public byte[]? ImageData { get; set; }
        public string? ImageMimeType { get; set; } 


        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Section Type")]
        public string SectionType { get; set; } = "Carousel"; // "Carousel" or "Recommendation"
    }
}
