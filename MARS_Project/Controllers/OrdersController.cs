using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;

namespace MARS_Project.Controllers
{
    public class OrdersController : Controller
    {
        private MARSEntities db = new MARSEntities();

        // Customer - list their orders
        public ActionResult Index()
        {
            ViewBag.Section = "Shopping";
            if (Session["UserID"] == null) return RedirectToAction("Login", "Home");
            int userId = (int)Session["UserID"];
            var orders = db.Orders.Where(o => o.CustomerID == userId).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        public ActionResult OrderDetails(int id)
        {
            var order = db.Orders
                          .Include("OrderItems.Product")
                          .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
                return HttpNotFound();

            return View(order);
        }
    }
}