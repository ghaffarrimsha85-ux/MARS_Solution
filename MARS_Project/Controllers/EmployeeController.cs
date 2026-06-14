using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;
using System.Data.Entity;


namespace MARS_Project.Controllers
{
    public class EmployeeController : Controller
    {

        private MARSEntities db = new MARSEntities();

        // 🧾 Dashboard — Show assigned repair requests
        public ActionResult Index()
        {
            if (Session["UserID"] == null || (int)Session["UserType"] != 3)
                return RedirectToAction("Login", "Home");

            int employeeId = Convert.ToInt32(Session["UserID"]);

            var assignedRepairs = db.RepairRequests
                .Where(r => r.AssignedEmployeeID == employeeId)
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(assignedRepairs);
        }

        // 🔍 View details of one repair request
        public ActionResult Details(int id)
        {
            var repair = db.RepairRequests.Find(id);
            if (repair == null)
                return HttpNotFound();

            return View(repair);
        }

        // 🛠️ Add repair response (solution, cost, time)
        [HttpPost]
        public ActionResult AddResponse(int id, string solutionDesc, decimal estimatedCost, string estimatedTime)
        {
            var repair = db.RepairRequests.Find(id);
            if (repair == null)
                return HttpNotFound();

            RepairResponse response = new RepairResponse
            {
                RequestID = id,
                EmployeeID = Convert.ToInt32(Session["UserID"]),
                SolutionDesc = solutionDesc,
                EstimatedCost = estimatedCost,
                EstimatedTime = estimatedTime,
                ResponseDate = DateTime.Now
            };

            db.RepairResponses.Add(response);
            repair.Status = "In Progress";
            db.SaveChanges();

            TempData["Message"] = "Repair response submitted successfully!";
            return RedirectToAction("Index");
        }
        public ActionResult AssignedRequests()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int employeeId = Convert.ToInt32(Session["UserID"]);

            // Fetch assigned repair requests — Include("User") loads customer name
            var assignedRequests = db.RepairRequests
                .Include("User")
                .Where(r => r.AssignedEmployeeID == employeeId)
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            return View(assignedRequests);
        }

        // ✅ GET: Respond to a specific repair request
        [HttpGet]
        public ActionResult RespondToRequest(int id)
        {
            var request = db.RepairRequests
                            .Include("User")
                            .FirstOrDefault(r => r.RequestID == id);
            if (request == null)
            {
                return HttpNotFound();
            }

            // Load customer info with the request
            ViewBag.CustomerName = request.User != null ? request.User.Name : "—";
            ViewBag.CustomerEmail = request.User != null ? request.User.Email : "—";
            ViewBag.CustomerContact = request.User != null ? request.User.ContactNumber : "—";

            return View(request);
        }

        // ✅ POST: Save technician response
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RespondToRequest(int id, string SolutionDesc, decimal EstimatedCost, string EstimatedTime)
        {
            var request = db.RepairRequests.Find(id);
            if (request == null)
            {
                return HttpNotFound();
            }

            int empId = Convert.ToInt32(Session["UserID"]);

            // Create response entry
            var response = new RepairResponse
            {
                RequestID = id,
                EmployeeID = empId,
                SolutionDesc = SolutionDesc,
                EstimatedCost = EstimatedCost,
                EstimatedTime = EstimatedTime,
                ResponseDate = DateTime.Now
            };

            db.RepairResponses.Add(response);

            // Update request status
            request.Status = "Responded";
            db.SaveChanges();

            TempData["Message"] = "Repair response submitted successfully!";
            return RedirectToAction("AssignedRequests");
        }


        // ✅ View all responses submitted by this employee
        public ActionResult MyResponses()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int empId = Convert.ToInt32(Session["UserID"]);

            var responses = db.RepairResponses
                              .Include("RepairRequest")
                              .Where(r => r.EmployeeID == empId)
                              .OrderByDescending(r => r.ResponseDate)
                              .ToList();

            return View(responses);
        }

        // ✅ Edit a specific response
        [HttpGet]
        public ActionResult EditResponse(int id)
        {
            var response = db.RepairResponses.Find(id);
            if (response == null)
                return HttpNotFound();

            return View(response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditResponse(RepairResponse model)
        {
            var response = db.RepairResponses.Find(model.ResponseID);
            if (response == null)
                return HttpNotFound();

            response.SolutionDesc = model.SolutionDesc;
            response.EstimatedCost = model.EstimatedCost;
            response.EstimatedTime = model.EstimatedTime;
            db.SaveChanges();

            TempData["Message"] = "Response updated successfully!";
            return RedirectToAction("MyResponses");
        }

        // ✅ Delete a response
        public ActionResult DeleteResponse(int id)
        {
            var response = db.RepairResponses.Find(id);
            if (response == null)
                return HttpNotFound();

            db.RepairResponses.Remove(response);
            db.SaveChanges();

            TempData["Message"] = "Response deleted successfully!";
            return RedirectToAction("MyResponses");
        }
        public ActionResult Reports()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int empId = Convert.ToInt32(Session["UserID"]);

            var assigned = db.RepairRequests
                             .Include("User")
                             .Include("User1")
                             .Where(r => r.AssignedEmployeeID == empId)
                             .ToList();

            var responses = db.RepairResponses
                              .Where(r => r.EmployeeID == empId)
                              .ToList();

            ViewBag.TotalAssigned = assigned.Count();
            ViewBag.Pending = assigned.Count(r => r.Status == "Pending");
            ViewBag.Responded = assigned.Count(r => r.Status == "Responded");
            ViewBag.Completed = assigned.Count(r => r.Status == "Completed");

            ViewBag.TotalEstimatedCost = responses.Sum(r => (decimal?)r.EstimatedCost) ?? 0;

            ViewBag.PendingCount = ViewBag.Pending;
            ViewBag.RespondedCount = ViewBag.Responded;
            ViewBag.CompletedCount = ViewBag.Completed;

            // Send assigned list to view for DataTable
            return View(assigned);
        }

        // 🧍‍♂️ View Employee Profile
        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int employeeId = Convert.ToInt32(Session["UserID"]);
            var employee = db.Users.FirstOrDefault(u => u.UserID == employeeId);

            if (employee == null)
                return HttpNotFound();

            return View(employee);
        }

        // ✏️ Edit Profile (GET)
        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int employeeId = Convert.ToInt32(Session["UserID"]);
            var employee = db.Users.FirstOrDefault(u => u.UserID == employeeId);

            if (employee == null)
                return HttpNotFound();

            return View(employee);
        }

        // 💾 Edit Profile (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(User model, HttpPostedFileBase ProfileImageFile)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int empId = Convert.ToInt32(Session["UserID"]);
            var emp = db.Users.Find(empId);

            if (emp == null)
            {
                TempData["Message"] = "Employee not found!";
                return RedirectToAction("Profile");
            }

            // ✅ Update only allowed fields
            emp.Name = model.Name;
            emp.Email = model.Email;
            emp.ContactNumber = model.ContactNumber;
            emp.Address = model.Address;

            // 🖼️ Update image if uploaded
            if (ProfileImageFile != null && ProfileImageFile.ContentLength > 0)
            {
                string folder = Server.MapPath("~/Content/ProfileImages/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(ProfileImageFile.FileName);
                ProfileImageFile.SaveAs(Path.Combine(folder, fileName));
                emp.ProfileImage = "/Content/ProfileImages/" + fileName;
            }

            // ❌ Don’t touch Password or CreatedDate here
            db.Entry(emp).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ✅ Product List
        public ActionResult ProductList()
        {
            var products = db.Products
                             .Include(p => p.ProductCategory)
                             .ToList();
            return View(products);
        }

        // ✅ Requests List
        public ActionResult RequestsList()
        {
            var requests = db.RepairRequests
                             .Include(r => r.User)
                             .ToList();
            return View(requests);
        }

        // ✅ Employee List
        public ActionResult EmployeeList()
        {
            var employees = db.Users
                .Where(u => u.UsertypeID == 2) // assuming 2 = Employee
                .ToList();
            return View(employees);
        }

        // ✅ Customer List
        public ActionResult CustomerList()
        {
            var customers = db.Users
                .Where(u => u.UsertypeID == 3)
                .OrderBy(u => u.Name)
                .ToList();
            return View(customers);
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int empId = Convert.ToInt32(Session["UserID"]);
            var emp = db.Users.Find(empId);
            if (emp == null)
                return HttpNotFound();

            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int empId = Convert.ToInt32(Session["UserID"]);
            var emp = db.Users.Find(empId);

            if (emp == null)
            {
                TempData["Message"] = "Employee not found.";
                return RedirectToAction("EditProfile");
            }

            if (emp.Password != CurrentPassword)
            {
                TempData["Message"] = "❌ Current password is incorrect.";
                return RedirectToAction("EditProfile");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Message"] = "⚠️ New password and confirm password do not match.";
                return RedirectToAction("EditProfile");
            }

            emp.Password = NewPassword;
            db.SaveChanges();

            TempData["Message"] = "✅ Password updated successfully!";
            return RedirectToAction("EditProfile");
        }

    }
}