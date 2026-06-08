using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;

namespace MARS_Project.Controllers
{
    public class CustomerRepairController : Controller
    {
        private MARSEntities db = new MARSEntities();

        // 🧾 Show Repair Request Form
        [HttpGet]
        public ActionResult SubmitRepair()
        {
            ViewBag.Section = "Repair";
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            Session["HasRepair"] = true;
            return View();
        }

        // 🧾 Handle Form Submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitRepair(RepairRequest req, HttpPostedFileBase ProblemImage1File, HttpPostedFileBase ProblemImage2File, HttpPostedFileBase ProblemImage3File)
        {
            ViewBag.Section = "Repair";
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);
            req.CustomerID = customerId;
            req.Status = "Pending";
            req.RequestDate = DateTime.Now;

            // 🖼️ Save images (if uploaded)
            string folder = Server.MapPath("~/Content/RepairImages/");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (ProblemImage1File != null && ProblemImage1File.ContentLength > 0)
            {
                string file1 = Guid.NewGuid() + Path.GetExtension(ProblemImage1File.FileName);
                ProblemImage1File.SaveAs(Path.Combine(folder, file1));
                req.ProblemImage1 = "/Content/RepairImages/" + file1;
            }

            if (ProblemImage2File != null && ProblemImage2File.ContentLength > 0)
            {
                string file2 = Guid.NewGuid() + Path.GetExtension(ProblemImage2File.FileName);
                ProblemImage2File.SaveAs(Path.Combine(folder, file2));
                req.ProblemImage2 = "/Content/RepairImages/" + file2;
            }

            if (ProblemImage3File != null && ProblemImage3File.ContentLength > 0)
            {
                string file3 = Guid.NewGuid() + Path.GetExtension(ProblemImage3File.FileName);
                ProblemImage3File.SaveAs(Path.Combine(folder, file3));
                req.ProblemImage3 = "/Content/RepairImages/" + file3;
            }

            db.RepairRequests.Add(req);
            db.SaveChanges();

            TempData["Message"] = "Repair request submitted successfully!";
            return RedirectToAction("MyRepairs");
        }

        // 📋 View Customer Repair Requests
        public ActionResult MyRepairs()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Home");

            int customerId = Convert.ToInt32(Session["UserID"]);
            var repairs = db.RepairRequests
                            .Where(r => r.CustomerID == customerId)
                            .OrderByDescending(r => r.RequestDate)
                            .ToList();
            return View(repairs);
        }
    
    }
}