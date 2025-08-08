using Microsoft.AspNetCore.Mvc;
using EntertainmentGuild.Data;

namespace EntertainmentGuild.Controllers
{
    // Controller to serve images for products and top product sections
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor injecting the database context
        public ImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET endpoint to retrieve product image by product ID
        [HttpGet("/Image/Product/{id}")]
        public IActionResult ProductImage(int id)
        {
            var product = _context.Products.Find(id);
            // Return 404 if no image data or MIME type found
            if (product?.ImageData == null || product.ImageMimeType == null)
                return NotFound();

            // Return the image file with correct MIME type
            return File(product.ImageData, product.ImageMimeType);
        }

        // GET endpoint to retrieve carousel top product image by ID
        [HttpGet("/Image/Carousel/{id}")]
        public IActionResult CarouselImage(int id)
        {
            var product = _context.CarouselTopProducts.Find(id);
            if (product?.ImageData == null || product.ImageMimeType == null)
                return NotFound();

            return File(product.ImageData, product.ImageMimeType);
        }

        // GET endpoint to retrieve recommended top product image by ID
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
