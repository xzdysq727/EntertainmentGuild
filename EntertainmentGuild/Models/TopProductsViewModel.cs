using System.Collections.Generic;

namespace EntertainmentGuild.Models
{
    public class TopProductsViewModel
    {
        public List<CarouselTopProduct> Carousel { get; set; } = new();
        public List<RecommendedTopProduct> Recommendations { get; set; } = new();
    }
}
