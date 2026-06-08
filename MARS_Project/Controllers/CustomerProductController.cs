using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;

namespace MARS_Project.Controllers
{
    public class CustomerProductController : Controller
    {
        // GET: CustomerProduct
        private MARSEntities db = new MARSEntities();

        // GET: CustomerProduct/Index
        public ActionResult Index(string search, int? categoryId)
        {
            ViewBag.Section = "Shopping";
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.ProductCategoryID == categoryId.Value);
            }

            ViewBag.Categories = new SelectList(db.ProductCategories.ToList(), "ProductCategoryID", "ProductCategoryName");
            return View(products.ToList());
        }

      

        // ✅ View Profile
        public ActionResult Profile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // ✅ Edit Profile (GET)
        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        // ✅ Edit Profile (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(User updatedUser, HttpPostedFileBase ProfileImageFile)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int userId = Convert.ToInt32(Session["UserID"]);
            var user = db.Users.FirstOrDefault(u => u.UserID == userId);

            if (user != null)
            {
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                user.ContactNumber = updatedUser.ContactNumber;
                user.Address = updatedUser.Address;

                // ✅ Handle Image Upload
                if (ProfileImageFile != null && ProfileImageFile.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(ProfileImageFile.FileName);
                    string path = Server.MapPath("~/Content/ProfileImages/");
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);

                    string fullPath = System.IO.Path.Combine(path, fileName);
                    ProfileImageFile.SaveAs(fullPath);
                    user.ProfileImage = "/Content/ProfileImages/" + fileName;
                }

                db.SaveChanges();
                TempData["Message"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            TempData["Message"] = "Something went wrong!";
            return RedirectToAction("Profile");
        }



    }
}