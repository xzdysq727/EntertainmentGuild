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

        public async Task<IActionResult> Index()
        {
            var vm = new TopProductsViewModel
            {
                Carousel = await _context.CarouselTopProducts.ToListAsync(),
                Recommendations = await _context.RecommendedTopProducts.ToListAsync()
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult Product(string? subCategory)
        {
            var products = _context.Products.ToList();
            if (!string.IsNullOrEmpty(subCategory))
            {
                products = products.Where(p => !string.IsNullOrEmpty(p.SubCategory) &&
                    string.Equals(p.SubCategory.Trim(), subCategory.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }
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

            var card = await _context.CreditCards
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

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
        public async Task<IActionResult> AddToCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItem = new Cart { UserId = user.Id, ProductId = productId };
            _context.Carts.Add(cartItem);
            await _context.SaveChangesAsync();
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
        public async Task<IActionResult> DeleteFromCart(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);
            if (item != null)
            {
                _context.Carts.Remove(item);
                await _context.SaveChangesAsync();
            }
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
                    CartId = c.Id,                // ✅ 关键：保留购物车项 ID
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
            var selectedCartItems = await _context.Carts.Include(c => c.Product).Where(c => selectedCartIds.Contains(c.Id) && c.UserId == user.Id).ToListAsync();
            var addresses = await _context.Addresses.Where(a => a.UserId == user.Id).ToListAsync();
            var cards = await _context.CreditCards.Where(c => c.UserId == user.Id).ToListAsync();

            var vm = new CheckoutViewModel
            {
                UserEmail = user.Email,
                Addresses = addresses,
                Cards = cards,
                CartItems = selectedCartItems.Select(ci => new CartItemViewModel
                {
                    CartId = ci.Id,                // ✅ 关键：保留购物车项 ID
                    Product = ci.Product
                }).ToList(),
                Subtotal = selectedCartItems.Sum(ci => ci.Product.Price),
                Tax = selectedCartItems.Sum(ci => ci.Product.Price) * 0.1m
            };
            return View("Checkout", vm);
        }

        [HttpPost]
        public async Task<IActionResult> PayNow(List<int> SelectedCartIds)
        {
            var userId = _userManager.GetUserId(User);

            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && SelectedCartIds.Contains(c.Id))
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Cart");
            }

            decimal subtotal = cartItems.Sum(c => c.Product.Price);
            decimal shipping = 10;
            decimal tax = subtotal * 0.1m;
            decimal total = subtotal + shipping + tax;

            var order = new Order
            {
                UserId = userId,
                Subtotal = subtotal,
                ShippingFee = shipping,
                Tax = tax,
                Total = total,
                ShippingMethod = "Standard",
                PaymentMethod = "Card",
                AddressId = 1,
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = 1
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.Carts.RemoveRange(cartItems); // ✅ 只移除选中的
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

    }
}
