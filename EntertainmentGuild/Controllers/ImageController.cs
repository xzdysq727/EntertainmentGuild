using Microsoft.AspNetCore.Mvc;
using EntertainmentGuild.Data;

namespace EntertainmentGuild.Controllers
{
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/Image/Product/{id}")]
        public IActionResult ProductImage(int id)
        {
            var product = _context.Products.Find(id);
            if (product?.ImageData == null || product.ImageMimeType == null)
                return NotFound();

            return File(product.ImageData, product.ImageMimeType);
        }

        [HttpGet("/Image/Carousel/{id}")]
        public IActionResult CarouselImage(int id)
        {
            var product = _context.CarouselTopProducts.Find(id);
            if (product?.ImageData == null || product.ImageMimeType == null)
                return NotFound();

            return File(product.ImageData, product.ImageMimeType);
        }

        [HttpGet("/Image/Recommendation/{id}")]
        public IActionResult RecommendationImage(int id)
        {
            var product = _context.RecommendedTopProducts.Find(id);
            if (product?.ImageData == null || product.ImageMimeType == null)
                return NotFound();

            return File(product.ImageData, product.ImageMimeType);
        }
    }
}

