using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MARS_Project.Models.ViewModels
{
    public class CartViewModels
    {
        public class CartItemViewModel
        {
            public int CartItemID { get; set; }
            public int ProductID { get; set; }
            public string ProductName { get; set; }
            public string ImageURL { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal SubTotal { get; set; }
        }

        public class CartViewModel
        {
            public int CartID { get; set; }
            public List<CartItemViewModel> Items { get; set; }
            public decimal Total { get; set; }
        }
    }
}