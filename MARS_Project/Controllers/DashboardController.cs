using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;

namespace MARS_Project.Controllers
{
    public class DashboardController : Controller
    {
        private MARSEntities db = new MARSEntities();

        // GET: Dashboard
      

        public ActionResult AdminDashboard()
        {
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCustomers = db.Users.Count(u => u.UsertypeID == 3); // adjust ID for customer
            ViewBag.TotalEmployees = db.Users.Count(u => u.UsertypeID == 2); // adjust ID for employee
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalRequests = db.RepairRequests.Count();

            // sample data for chart - requests by status
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.PendingUsers = db.Users.Count(u => u.Status == "Pending");
            ViewBag.PendingRequests = db.RepairRequests.Count(r => r.Status == "Pending");
            ViewBag.AssignedRequests = db.RepairRequests.Count(r => r.Status == "Assigned");
            ViewBag.RespondedRequests = db.RepairRequests.Count(r => r.Status == "Responded");
            ViewBag.CompletedRequests = db.RepairRequests.Count(r => r.Status == "Completed");

            return View();
        }

        public ActionResult EmployeeDashboard()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int employeeId = Convert.ToInt32(Session["UserID"]);

            // ✅ Safely load data (adjust table names below if needed)
            ViewBag.TotalUsers = db.Users?.Count() ?? 0;

            // Change this line according to your real Product table name
            ViewBag.TotalProducts = db.Products?.Count() ?? 0;

            // Only show requests assigned to this employee
            ViewBag.TotalRequests = db.RepairRequests?
                .Count(r => r.AssignedEmployeeID == employeeId) ?? 0;

            ViewBag.TotalReports = db.RepairResponses?
                .Count(r => r.EmployeeID == employeeId) ?? 0;

            // Count based on user type IDs (adjust if needed)
            ViewBag.TotalEmployees = db.Users?
                .Count(u => u.UsertypeID == 3) ?? 0;

            ViewBag.TotalCustomers = db.Users?
                .Count(u => u.UsertypeID == 2) ?? 0;

            return View();
        }
        public ActionResult CustomerDashboard()
        {
            return View();
        }
    }
}