using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;
using static MARS_Project.Models.ViewModels.CartViewModels;

namespace MARS_Project.Controllers
{
    public class CartController : Controller
    {
        private MARSEntities db = new MARSEntities();

        // ✅ Add product to cart
        public ActionResult AddToCart(int id)
        {
            ViewBag.Section = "Shopping";
           
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);

            // Get or create cart
            Session["HasOrder"] = true;
            var cart = db.Carts.FirstOrDefault(c => c.CustomerID == customerId && c.Status == "Open");
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerID = customerId,
                    CreatedDate = DateTime.Now,
                    Status = "Open"
                };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            // Add / update item
            var existingItem = db.CartItems.FirstOrDefault(ci => ci.CartID == cart.CartID && ci.ProductID == id);
            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var product = db.Products.Find(id);
                if (product != null)
                {
                    var newItem = new CartItem
                    {
                        CartID = cart.CartID,
                        ProductID = product.ProductID,
                        Quantity = 1,
                        Price = product.Price
                    };
                    db.CartItems.Add(newItem);
                }
            }

            db.SaveChanges();

            // Update session cart count
            Session["CartCount"] = db.CartItems.Count(ci => ci.CartID == cart.CartID);

            return RedirectToAction("ViewCart");
        }

        // ✅ View cart
        public ActionResult ViewCart()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);
            var cart = db.Carts.FirstOrDefault(c => c.CustomerID == customerId && c.Status == "Open");

            return View(cart);
        }

        // ✅ Remove item from cart
        public ActionResult RemoveItem(int id)
        {
            var item = db.CartItems.Find(id);
            if (item != null)
            {
                db.CartItems.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("ViewCart");
        }

        // ✅ Checkout page
        public ActionResult Checkout()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);
            var cart = db.Carts.FirstOrDefault(c => c.CustomerID == customerId && c.Status == "Open");

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("ViewCart");

            return View(cart);
        }

        // ✅ Place Order
        [HttpPost]
        public ActionResult PlaceOrder()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);
            var cart = db.Carts.FirstOrDefault(c => c.CustomerID == customerId && c.Status == "Open");

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("ViewCart");

            // Create order
            var order = new Order
            {
                CustomerID = customerId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cart.CartItems.Sum(x => x.Price * x.Quantity)
            };
            db.Orders.Add(order);
            db.SaveChanges();

            // Save order items
            foreach (var ci in cart.CartItems)
            {
                var oi = new OrderItem
                {
                    OrderID = order.OrderID,
                    ProductID = ci.ProductID,
                    Quantity = ci.Quantity,
                    Price = ci.Price
                };
                db.OrderItems.Add(oi);
            }

            // Close cart
            cart.Status = "Closed";
            db.SaveChanges();

            // Reset session cart count
            Session["CartCount"] = 0;

            TempData["Message"] = "Your order has been placed successfully!";
            return RedirectToAction("MyOrders");

        }

        // ✅ My Orders
        public ActionResult MyOrders()
        {
            // ✅ Check if user is logged in
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);

            // ✅ Get orders directly from Orders table, not from Cart
            var orders = db.Orders
                           .Where(o => o.CustomerID == customerId)
                           .OrderByDescending(o => o.OrderDate)
                           .ToList();

            return View(orders);
        }

        


        public ActionResult IncreaseQty(int id)
        {
            var item = db.CartItems.Find(id);
            if (item != null)
            {
                item.Quantity += 1;
                db.SaveChanges();
            }
            return RedirectToAction("ViewCart");
        }

        public ActionResult DecreaseQty(int id)
        {
            var item = db.CartItems.Find(id);
            if (item != null)
            {
                if (item.Quantity > 1)
                    item.Quantity -= 1;
                else
                    db.CartItems.Remove(item); // if qty = 1 → remove from cart
                db.SaveChanges();
            }
            return RedirectToAction("ViewCart");
        }


        public ActionResult CancelOrder(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            var order = db.Orders.Find(id);

            if (order != null && order.Status == "Pending")
            {
                order.Status = "Cancelled";
                db.SaveChanges();

                // ✅ Show success message on next page
                TempData["Message"] = $"Order #{order.OrderID} has been cancelled successfully.";
            }
            else
            {
                TempData["Message"] = "Unable to cancel this order.";
            }

            return RedirectToAction("MyOrders");
        }

        public ActionResult DeleteOrder(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);
            var order = db.Orders.FirstOrDefault(o => o.OrderID == id && o.CustomerID == customerId);

            if (order != null)
            {
                if (order.Status == "Cancelled")
                {
                    db.OrderItems.RemoveRange(order.OrderItems);
                    db.Orders.Remove(order);
                    db.SaveChanges();

                    // ✅ SweetAlert success message
                    TempData["Message"] = $"Order #{order.OrderID} deleted successfully.";
                }
                else
                {
                    TempData["Message"] = "You can only delete cancelled orders.";
                }
            }

            return RedirectToAction("MyOrders");
        }

        public ActionResult DeleteAllOrders()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);
            var orders = db.Orders.Where(o => o.CustomerID == customerId && o.Status == "Cancelled").ToList();

            foreach (var order in orders)
            {
                db.OrderItems.RemoveRange(order.OrderItems);
                db.Orders.Remove(order);
            }

            db.SaveChanges();

            // ✅ Show total count deleted
            TempData["Message"] = $"{orders.Count} cancelled orders have been deleted successfully.";

            return RedirectToAction("MyOrders");
        }


        public ActionResult OrderDetails(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);

            // Find order with related items
            var order = db.Orders
                          .Include("OrderItems.Product")
                          .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customerId);

            if (order == null)
            {
                TempData["Message"] = "Order not found.";
                return RedirectToAction("MyOrders");
            }

            return View(order);
        }


    }
}