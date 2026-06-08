using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;
using System.Data.Entity;

namespace MARS_Project.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin


        private MARSEntities db = new MARSEntities();


        // Show all users
        public ActionResult PendingUsers()
        {
            var pendingUsers = db.Users
                .Where(u => u.Status == "Pending" && u.UsertypeID != null) // show only pending users (not admin)
                .OrderBy(u => u.CreatedDate)
                .ToList();

            return View(pendingUsers);
        }
        public ActionResult ManageUsers()
        {
            var users = db.Users.Include(u => u.UserType).ToList();
            return View(users);
        }

        public ActionResult ManageEmployees()
        {
            var employees = db.Users
                              .Where(u => u.UsertypeID == 6)
                              .ToList();
            return View(employees);
        }

        // 👤 Manage Customers
        public ActionResult ManageCustomers()
        {
            var customers = db.Users
                              .Where(u => u.UsertypeID == 5)
                              .ToList();
            return View(customers);
        }
        // GET: Edit User
        public ActionResult EditUser(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            ViewBag.UsertypeID = new SelectList(db.UserTypes, "UsertypeID", "UserTypeName", user.UsertypeID);
            return View(user);
        }

        // POST: Edit User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(User model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(model.UserID);
                if (user == null) return HttpNotFound();

                // Update only editable fields
                user.Name = model.Name;
                user.Email = model.Email;
                user.UsertypeID = model.UsertypeID;
                user.Status = model.Status;

                db.SaveChanges();
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("ManageUsers");
            }

            ViewBag.UsertypeID = new SelectList(db.UserTypes, "UsertypeID", "UserTypeName", model.UsertypeID);
            return View(model);
        }

        // ═══════════════════════════════════════════════════════════
        // AdminController mein yeh 4 actions add karo
        // EXISTING code ke saath — replace mat karna, sirf add karna
        // DeleteUser action ke baad paste karo yeh sab
        // ═══════════════════════════════════════════════════════════

        // ── CREATE EMPLOYEE (GET)
        public ActionResult CreateEmployee()
        {
            return View();
        }

        // ── CREATE EMPLOYEE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEmployee(string Name, string Email, string Password,
                                            string ContactNumber, string Address)
        {
            // Check if email already exists
            var existing = db.Users.FirstOrDefault(u => u.Email == Email);
            if (existing != null)
            {
                TempData["ErrorMessage"] = "This email is already registered!";
                return RedirectToAction("CreateEmployee");
            }

            var emp = new User
            {
                Name = Name,
                Email = Email,
                Password = Password,
                ContactNumber = ContactNumber,
                Address = Address,
                UsertypeID = 6,              // 6 = Employee (as per your UserType table)
                Status = "Active",       // Active by default
                CreatedDate = DateTime.Now
            };

            db.Users.Add(emp);
            db.SaveChanges();

            TempData["Message"] = "Employee account created successfully!";
            return RedirectToAction("ManageEmployees");
        }

        // ── ACTIVATE EMPLOYEE
        public ActionResult ActivateEmployee(int id)
        {
            var emp = db.Users.Find(id);
            if (emp != null)
            {
                emp.Status = "Active";
                db.SaveChanges();
                TempData["Message"] = emp.Name + " has been activated successfully!";
            }
            return RedirectToAction("ManageEmployees");
        }

        // ── DEACTIVATE EMPLOYEE
        public ActionResult DeactivateEmployee(int id)
        {
            var emp = db.Users.Find(id);
            if (emp != null)
            {
                emp.Status = "Inactive";
                db.SaveChanges();
                TempData["Message"] = emp.Name + " has been deactivated!";
            }
            return RedirectToAction("ManageEmployees");
        }

        // GET: Delete User
        public ActionResult DeleteUser(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
                TempData["Message"] = "User deleted successfully!";
            }
            return RedirectToAction("ManageUsers");
        }


        /*/ POST: Confirm Delete
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = "User deleted successfully!";
                return RedirectToAction("ManageUsers");
            }
            return RedirectToAction("ManageUsers");
        }*/

        // APPROVE
        public ActionResult ApproveUser(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                user.Status = "Approved";
                db.SaveChanges();
            }
            return RedirectToAction("ManageUsers");
        }
        // REJECT
        public ActionResult RejectUser(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                user.Status = "Rejected";
                db.SaveChanges();
            }
            return RedirectToAction("ManageUsers");
        }

        //************************************************************************
        //Manages order

        public ActionResult ManageOrders()
        {
            var orders = db.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        public ActionResult OrderDetailsAdmin(int id)
        {
            var order = db.Orders.Include(o => o.OrderItems.Select(oi => oi.Product)).FirstOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Processing", "Completed", "Cancelled" }, order.Status);
            return View(order);
        }

        [HttpPost]
        public ActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Order status updated.";
            }
            return RedirectToAction("ManageOrders");
        }






        //************************************************************************
        // list repair requests
        public ActionResult RepairRequests()
        {
            var reqs = db.RepairRequests.Include(r => r.User).ToList();
            return View(reqs);
        }

        // Assign GET
        public ActionResult AssignRepair(int id)
        {
            var req = db.RepairRequests.Find(id);
            if (req == null) return HttpNotFound();
            // get employees
            var employees = db.Users.Where(u => u.UsertypeID == 2 && u.Status == "Approved").ToList();
            ViewBag.Employees = new SelectList(employees, "UserID", "Name", req.AssignedEmployeeID);
            return View(req);
        }

        // Assign POST
        [HttpPost]
        public ActionResult AssignRepair(int RequestID, int AssignedEmployeeID)
        {
            var req = db.RepairRequests.Find(RequestID);
            if (req == null) return HttpNotFound();
            req.AssignedEmployeeID = AssignedEmployeeID;
            req.Status = "Assigned";
            db.SaveChanges();
            TempData["SuccessMessage"] = "Repair assigned.";
            return RedirectToAction("RepairRequests");
        }


        //************************************************************************
    

    // 🧾 Show All Orders
    public ActionResult AllOrders()
        {
            var orders = db.Orders
                           .Include(o => o.User)
                           .OrderByDescending(o => o.OrderDate)
                           .ToList();
            return View(orders);
        }

        // 🗑️ Delete an Order
        public ActionResult DeleteOrder(int id)
        {
            try
            {
                var order = db.Orders.Find(id);
                if (order != null)
                {
                    db.Orders.Remove(order);
                    db.SaveChanges();
                    TempData["Message"] = "Order deleted successfully!";
                }
                else
                {
                    TempData["Message"] = "Order not found.";
                }
            }
            catch
            {
                TempData["Message"] = "Cannot delete this order — it may be linked to items.";
            }

            return RedirectToAction("AllOrders");
        }

        // ═══════════════════════════════════════════════════════════
        // AdminController mein yeh actions add karo
        // Last action ke baad, Dispose() se pehle paste karo
        // ═══════════════════════════════════════════════════════════

        // ── PROFILE (GET)
        public ActionResult Profile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // ── EDIT PROFILE (GET)
        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null) return HttpNotFound();

            return View(user);
        }

        // ── EDIT PROFILE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(User model, HttpPostedFileBase ProfileImageFile)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);

            if (user != null)
            {
                user.Name = model.Name;
                user.ContactNumber = model.ContactNumber;
                user.Address = model.Address;
                // Email NOT updated — readonly

                // Handle image upload
                if (ProfileImageFile != null && ProfileImageFile.ContentLength > 0)
                {
                    string folder = Server.MapPath("~/Content/ProfileImages/");
                    if (!System.IO.Directory.Exists(folder))
                        System.IO.Directory.CreateDirectory(folder);

                    string fileName = System.Guid.NewGuid().ToString()
                                      + System.IO.Path.GetExtension(ProfileImageFile.FileName);
                    ProfileImageFile.SaveAs(System.IO.Path.Combine(folder, fileName));
                    user.ProfileImage = "/Content/ProfileImages/" + fileName;

                    // Update session image
                    Session["UserImage"] = user.ProfileImage;
                }

                // Update session name
                Session["UserName"] = user.Name;

                db.SaveChanges();
                TempData["Message"] = "Profile updated successfully!";
            }

            return RedirectToAction("Profile");
        }

        // ── CHANGE PASSWORD (GET)
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            return View();
        }

        // ── CHANGE PASSWORD (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);

            if (user == null)
            {
                TempData["Message"] = "User not found!";
                return RedirectToAction("ChangePassword");
            }

            // Check current password
            if (user.Password != CurrentPassword)
            {
                TempData["Message"] = "Current password is incorrect!";
                return RedirectToAction("ChangePassword");
            }

            // Check new password match
            if (NewPassword != ConfirmPassword)
            {
                TempData["Message"] = "New passwords do not match!";
                return RedirectToAction("ChangePassword");
            }

            // Check minimum length
            if (NewPassword.Length < 6)
            {
                TempData["Message"] = "New password must be at least 6 characters!";
                return RedirectToAction("ChangePassword");
            }

            // Save new password
            user.Password = NewPassword;
            db.SaveChanges();

            TempData["Message"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }

        // ✅ Update Order Status
        [HttpPost]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                order.Status = status;
                db.SaveChanges();
                TempData["Message"] = "Order status updated successfully!";
            }

            return RedirectToAction("AllOrders");
        }
        // ═══════════════════════════════════════════════════════════
        // AdminController mein yeh 2 actions add karo
        // CreateEmployee actions ke baad paste karo
        // ═══════════════════════════════════════════════════════════

        // ── EDIT EMPLOYEE (GET)
        public ActionResult EditEmployee(int id)
        {
            var emp = db.Users.Find(id);
            if (emp == null) return HttpNotFound();
            return View(emp);
        }

        // ── EDIT EMPLOYEE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEmployee(User model)
        {
            var emp = db.Users.Find(model.UserID);
            if (emp == null) return HttpNotFound();

            emp.Name = model.Name;
            emp.ContactNumber = model.ContactNumber;
            emp.Address = model.Address;
            emp.Status = model.Status;
            // Email intentionally NOT updated — readonly field

            db.SaveChanges();
            TempData["Message"] = "Employee updated successfully!";
            return RedirectToAction("ManageEmployees");
        }
        public ActionResult GetOrderDetails(int id)
        {
            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .AsNoTracking()  // prevents proxy wrapping
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
                return HttpNotFound();

            // Create clean data (no proxy type names)
            var cleanOrder = new MARS_Project.Models.Order
            {
                OrderID = order.OrderID,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                User = new MARS_Project.Models.User
                {
                    Name = order.User?.Name,
                    Email = order.User?.Email
                },
                OrderItems = order.OrderItems.Select(i => new MARS_Project.Models.OrderItem
                {
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Product = new MARS_Project.Models.Product
                    {
                        Name = i.Product?.Name
                    }
                }).ToList()
            };

            return PartialView("_OrderDetailsPartial", cleanOrder);
        }


    }
}