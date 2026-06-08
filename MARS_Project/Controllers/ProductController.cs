using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;

namespace MARS_Project.Controllers
{
    public class ProductController : Controller
    {
        private MARSEntities db = new MARSEntities();

        // ── READ
        public ActionResult ProductIndex()
        {
            var products = db.Products.Include("ProductCategory").ToList();
            return View(products);
        }

        // ── CREATE (GET)
        public ActionResult ProductCreate()
        {
            ViewBag.ProductCategoryID = new SelectList(
                db.ProductCategories, "ProductCategoryID", "ProductCategoryName");
            return View();
        }

        // ── CREATE (POST)
        // FIX: View sends file as HttpPostedFileBase "PRO_PIC" — accept as parameter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductCreate(Product product, HttpPostedFileBase PRO_PIC)
        {
            if (ModelState.IsValid)
            {
                if (PRO_PIC != null && PRO_PIC.ContentLength > 0)
                {
                    string folder = Server.MapPath("~/Content/ProductImages/");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    // Use Guid to avoid duplicate filenames
                    string fileName = Guid.NewGuid() + Path.GetExtension(PRO_PIC.FileName);
                    PRO_PIC.SaveAs(Path.Combine(folder, fileName));
                    product.ImageURL = "/Content/ProductImages/" + fileName;
                }
                else
                {
                    product.ImageURL = "~/Content/Images/andoridePhon.jpg";
                }

                db.Products.Add(product);
                db.SaveChanges();

                TempData["Message"] = "Product added successfully!";
                return RedirectToAction("ProductIndex");
            }

            ViewBag.ProductCategoryID = new SelectList(
                db.ProductCategories, "ProductCategoryID", "ProductCategoryName",
                product.ProductCategoryID);
            return View(product);
        }

        // ── EDIT (GET)
        public ActionResult ProductEdit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.ProductCategoryID = new SelectList(
                db.ProductCategories, "ProductCategoryID", "ProductCategoryName",
                product.ProductCategoryID);
            return View(product);
        }

        // ── EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductEdit(Product product, HttpPostedFileBase PRO_PIC)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Products.Find(product.ProductID);
                if (existing == null) return HttpNotFound();

                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.Stock = product.Stock;
                existing.ProductCategoryID = product.ProductCategoryID;

                // Update image only if new one uploaded
                if (PRO_PIC != null && PRO_PIC.ContentLength > 0)
                {
                    string folder = Server.MapPath("~/Content/ProductImages/");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(PRO_PIC.FileName);
                    PRO_PIC.SaveAs(Path.Combine(folder, fileName));
                    existing.ImageURL = "/Content/ProductImages/" + fileName;
                }
                // else keep old ImageURL

                db.SaveChanges();
                TempData["Message"] = "Product updated successfully!";
                return RedirectToAction("ProductIndex");
            }

            ViewBag.ProductCategoryID = new SelectList(
                db.ProductCategories, "ProductCategoryID", "ProductCategoryName",
                product.ProductCategoryID);
            return View(product);
        }

        // ── DELETE (GET — direct delete, no separate page needed)
        // FIX: Properly handles linked CartItems & OrderItems before deleting
        [HttpGet]
        public ActionResult ProductDelete(int id)
        {
            try
            {
                var product = db.Products.Find(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found!";
                    return RedirectToAction("ProductIndex");
                }

                // Remove linked CartItems first
                var cartItems = db.CartItems.Where(c => c.ProductID == id).ToList();
                if (cartItems.Any())
                    db.CartItems.RemoveRange(cartItems);

                // Remove linked OrderItems first
                var orderItems = db.OrderItems.Where(o => o.ProductID == id).ToList();
                if (orderItems.Any())
                    db.OrderItems.RemoveRange(orderItems);

                // Delete image file from disk (optional cleanup)
                if (!string.IsNullOrEmpty(product.ImageURL)
                    && !product.ImageURL.Contains("andoridePhon.jpg"))
                {
                    string fullPath = Server.MapPath(product.ImageURL);
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                db.Products.Remove(product);
                db.SaveChanges();

                TempData["Message"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Could not delete product: " + ex.Message;
            }

            return RedirectToAction("ProductIndex");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}