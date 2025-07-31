using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntertainmentGuild.Data;
using EntertainmentGuild.Models;
using EntertainmentGuild.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EntertainmentGuild.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CustomerController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var vm = new TopProductsViewModel
            {
                Carousel = await _context.CarouselTopProducts.ToListAsync(),
                Recommendations = await _context.RecommendedTopProducts.ToListAsync()
            };

            // 随机展示 8 个商品
            ViewBag.RandomProducts = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .OrderBy(p => Guid.NewGuid())
                .Take(8)
                .ToListAsync();

            // 如果用户输入了搜索关键词
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var results = await _context.Products
                    .Where(p =>
                        (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(keyword)) ||
                        (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(keyword)) ||
                        (!string.IsNullOrEmpty(p.SubCategory) && p.SubCategory.Contains(keyword)))
                    .ToListAsync();

                ViewBag.Keyword = keyword;
                ViewBag.SearchResults = results;
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Search(string keyword)
        {
            ViewBag.Keyword = keyword;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return View("Search", new List<Product>());
            }

            var results = _context.Products
                .Where(p =>
                    EF.Functions.Like(p.Name, $"%{keyword}%") ||
                    EF.Functions.Like(p.Description, $"%{keyword}%") ||
                    EF.Functions.Like(p.SubCategory, $"%{keyword}%"))
                .ToList();

            return View("Search", results);
        }

        [HttpGet]
        public JsonResult GetRandomProducts()
        {
            var products = _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .OrderBy(p => Guid.NewGuid())
                .Select(p => p.Name)
                .Take(6)
                .ToList();

            return Json(products);
        }



        [HttpGet]
        public IActionResult Product(string? category, string? subCategory)
        {
            var products = _context.Products.ToList(); 

            if (!string.IsNullOrEmpty(category))
            {
                products = products
                    .Where(p => !string.IsNullOrEmpty(p.Category) &&
                                p.Category.Trim().Equals(category.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(subCategory))
            {
                products = products
                    .Where(p => !string.IsNullOrEmpty(p.SubCategory) &&
                                p.SubCategory.Trim().Equals(subCategory.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.SelectedCategory = category;
            ViewBag.SelectedSubCategory = subCategory;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var user = await _userManager.GetUserAsync(User);
            return View("CustomerAccount", user);
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(string Address, string City, string State, string PostalCode)
        {
            var user = await _userManager.GetUserAsync(User);
            var address = new Address
            {
                UserId = user.Id,
                AddressLine = Address,
                City = City,
                State = State,
                PostalCode = PostalCode
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            TempData["AddressSaved"] = "Address saved successfully.";
            return View("CustomerAccount", user);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAddress()
        {
            var user = await _userManager.GetUserAsync(User);
            var addresses = await _context.Addresses.Where(a => a.UserId == user.Id).ToListAsync();
            return View("ViewAddress", addresses);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ViewAddress");
        }

        [HttpPost]
        public async Task<IActionResult> AddCard(string CardHolder, string CardNumber, string ExpiryMonth, string ExpiryYear, string CardType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            int month = int.Parse(ExpiryMonth);
            int year = int.Parse(ExpiryYear);

            var card = new CreditCard
            {
                UserId = user.Id,
                CardHolder = CardHolder,
                CardNumber = CardNumber,
                ExpiryMonth = month,
                ExpiryYear = year,
                CardType = CardType
            };

            _context.CreditCards.Add(card);
            await _context.SaveChangesAsync();

            TempData["CardSaved"] = "Card added successfully.";
            return RedirectToAction("Account");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCard(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var card = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (card != null)
            {
                _context.CreditCards.Remove(card);
                await _context.SaveChangesAsync();
                TempData["CardDeleted"] = "Card deleted successfully.";
            }

            return RedirectToAction("ViewCards");
        }

        [HttpGet]
        public async Task<IActionResult> ViewCards()
        {
            var user = await _userManager.GetUserAsync(User);
            var cards = await _context.CreditCards
                                      .Where(c => c.UserId == user.Id)
                                      .ToListAsync();
            return View(cards);
        }

        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("ChangePassword");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                TempData["CartMessage"] = "Product not found.";
                return RedirectToAction("Product");
            }

            if (quantity > product.Quantity)
            {
                TempData["CartMessage"] = "Requested quantity exceeds available stock.";
                return RedirectToAction("ProductDetails", new { id = productId });
            }

            var userId = _userManager.GetUserId(User);

            // 👇 先查有没有已存在相同商品在购物车
            var existingCartItem = _context.Carts
                .FirstOrDefault(c => c.ProductId == productId && c.UserId == userId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                _context.Carts.Update(existingCartItem);
            }
            else
            {
                var cartItem = new Cart
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = userId
                };
                _context.Carts.Add(cartItem);
            }

            _context.SaveChanges();

            TempData["CartMessage"] = $"{quantity} item(s) added to cart!";
            return RedirectToAction("ProductDetails", new { id = productId });
        }


        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.Carts.Include(c => c.Product).Where(c => c.UserId == user.Id).ToListAsync();
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelectedFromCart(List<int> selectedCartIds)
        {
            var user = await _userManager.GetUserAsync(User);
            var itemsToDelete = await _context.Carts
                .Where(c => selectedCartIds.Contains(c.Id) && c.UserId == user.Id)
                .ToListAsync();

            _context.Carts.RemoveRange(itemsToDelete);
            await _context.SaveChangesAsync();

            return RedirectToAction("Cart");
        }


        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var addresses = await _context.Addresses.Where(a => a.UserId == user.Id).ToListAsync();
            var cartItems = await _context.Carts.Include(c => c.Product).Where(c => c.UserId == user.Id).ToListAsync();
            var cards = await _context.CreditCards.Where(c => c.UserId == user.Id).ToListAsync();
            decimal subtotal = cartItems.Sum(c => c.Product.Price);
            decimal tax = subtotal * 0.08m;

            var vm = new CheckoutViewModel
            {
                UserEmail = user.Email,
                Addresses = addresses,
                CartItems = cartItems.Select(c => new CartItemViewModel
                {
                    CartId = c.Id,
                    Product = c.Product
                }).ToList(),
                Cards = cards,
                Subtotal = subtotal,
                Tax = tax
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(List<int> selectedCartIds)
        {
            var user = await _userManager.GetUserAsync(User);

            var selectedCartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => selectedCartIds.Contains(c.Id) && c.UserId == user.Id)
                .ToListAsync();

            var addresses = await _context.Addresses.Where(a => a.UserId == user.Id).ToListAsync();
            var cards = await _context.CreditCards.Where(c => c.UserId == user.Id).ToListAsync();

            decimal subtotal = selectedCartItems.Sum(ci => (ci.Product.Price * ci.Quantity));
            decimal tax = subtotal * 0.1m;

            var vm = new CheckoutViewModel
            {
                UserEmail = user.Email,
                Addresses = addresses,
                Cards = cards,
                CartItems = selectedCartItems.Select(ci => new CartItemViewModel
                {
                    CartId = ci.Id,
                    Product = ci.Product,
                    Quantity = ci.Quantity     // ✅ 加上数量
                }).ToList(),
                Subtotal = subtotal,
                Tax = tax
            };

            return View("Checkout", vm);
        }


        [HttpPost]
        public async Task<IActionResult> PayNow(List<int> SelectedCartIds, int AddressId, int CardId, string ShippingMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && SelectedCartIds.Contains(c.Id))
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Cart");
            }

            decimal subtotal = cartItems.Sum(c => c.Product.Price * c.Quantity);
            decimal shippingFee = decimal.Parse(ShippingMethod); // 👈 注意这里接收的值就是金额数字 0 / 8 / 15
            decimal tax = subtotal * 0.1m;
            decimal total = subtotal + shippingFee + tax;

            var order = new Order
            {
                UserId = userId,
                UserEmail = user.Email,
                AddressId = AddressId,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                Tax = tax,
                Total = total,
                ShippingMethod = shippingFee == 0 ? "Standard" :
                                 shippingFee == 8 ? "Express" : "Next-Day Delivery",
                PaymentMethod = "Card",
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }



        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return View("History", orders);
        }

        [HttpGet]
        public async Task<IActionResult> ViewShipping(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)  
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);  
        }

    }
}
