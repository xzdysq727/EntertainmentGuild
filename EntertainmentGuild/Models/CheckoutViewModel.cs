using EntertainmentGuild.Models;
using System.Collections.Generic;

namespace EntertainmentGuild.ViewModels
{
    public class CheckoutViewModel
    {
        public string UserEmail { get; set; }

        public List<Address> Addresses { get; set; } = new();
        public List<CreditCard> Cards { get; set; }

        public List<CartItemViewModel> CartItems { get; set; } = new();

        public decimal Subtotal { get; set; }

        public decimal Tax { get; set; }

        public decimal Total => Subtotal + Tax;
    }

    public class CartItemViewModel
    {
        public int CartId { get; set; }            
        public Product Product { get; set; }      
        public int Quantity { get; set; }
    }

}
