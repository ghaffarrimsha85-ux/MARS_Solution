using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using MARS_Project.Models;

namespace MARS_Project.Controllers
{
    public class ProductCategoriesController : Controller
    {
        private MARSEntities db = new MARSEntities();

        // ── INDEX
        public ActionResult Index()
        {
            return View(db.ProductCategories.ToList());
        }

        // ── CREATE (GET)
        public ActionResult Create()
        {
            return View();
        }

        // ── CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductCategoryID,ProductCategoryName")] ProductCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                db.ProductCategories.Add(productCategory);
                db.SaveChanges();
                TempData["Message"] = "Category added successfully!";
                return RedirectToAction("Index");
            }
            return View(productCategory);
        }

        // ── EDIT (GET)
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var productCategory = db.ProductCategories.Find(id);
            if (productCategory == null) return HttpNotFound();
            return View(productCategory);
        }

        // ── EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductCategoryID,ProductCategoryName")] ProductCategory productCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productCategory).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Message"] = "Category updated successfully!";
                return RedirectToAction("Index");
            }
            return View(productCategory);
        }

        // ── DELETE (GET — direct delete with SweetAlert from Index)
        // FIX: Was commented out before — now properly deletes and redirects back
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Invalid request.";
                return RedirectToAction("Index");
            }

            try
            {
                var productCategory = db.ProductCategories.Find(id);
                if (productCategory == null)
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("Index");
                }

                // Set products in this category to null (uncategorized) before deleting
                var products = db.Products.Where(p => p.ProductCategoryID == id).ToList();
                foreach (var p in products)
                {
                    p.ProductCategoryID = null;
                }

                db.ProductCategories.Remove(productCategory);
                db.SaveChanges();
                TempData["Message"] = "Category deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Could not delete category: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}