using System.Collections.Generic;
using EntertainmentGuild.Models;

namespace EntertainmentGuild.ViewModels
{
    public class CustomerProductViewModel
    {
        public List<Product> Products { get; set; }
        public List<string> MainCategories { get; set; }
        public Dictionary<string, List<string>> SubCategoryMap { get; set; }
    }
}
