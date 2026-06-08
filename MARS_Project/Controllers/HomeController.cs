// =====================================================
//  HomeController.cs — REGISTER & LOGIN (Updated)
//  MARS Project
// =====================================================
//  CHANGES:
//  1. Register: UserType dropdown REMOVED
//     - Har naya user automatically Customer (UsertypeID = 3) ban jata hai
//     - Status automatically "Active" hota hai — Admin approval ki zaroorat nahi
//  2. Admin Employee banayega — alag action "CreateEmployee" add karo Admin panel mein
//  3. Login logic same hai — UsertypeID se automatic redirect hota hai
// =====================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MARS_Project.Models;

namespace MARS_Project.Controllers
{
    public class HomeController : Controller
    {
        private MARSEntities db = new MARSEntities();

        // -----------------------------------------------
        // GET: Register
        // -----------------------------------------------
        public ActionResult Register()
        {
            return View();
            // NOTE: ViewBag.UserTypes ki zaroorat nahi — dropdown hata diya
        }

        // -----------------------------------------------
        // POST: Register
        // Customer only — auto approved, no admin needed
        // -----------------------------------------------
        [HttpPost]
        public ActionResult Register(User model)
        {
            using (MARSEntities db = new MARSEntities())
            {
                // Email already registered check
                var existingUser = db.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "This email is already registered. Please login.";
                    return View(model);
                }

                // Auto-set: Customer role + Active status
                model.UsertypeID = 5;          // 3 = Customer (as per your UserType table)
                model.Status = "Active";   // Auto approved — no admin needed
                model.CreatedDate = DateTime.Now;

                db.Users.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Account created successfully! Please login.";
                return RedirectToAction("Login", "Home");
            }
        }

        // -----------------------------------------------
        // GET: Login
        // -----------------------------------------------
        public ActionResult Login()
        {
            return View();
        }

        // -----------------------------------------------
        // POST: Login
        // Auto-redirect by role (Customer / Employee / Admin)
        // -----------------------------------------------
        [HttpPost]
        public ActionResult Login(string useremail, string password)
        {
            if (string.IsNullOrEmpty(useremail) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Please enter your Email and Password.";
                return View();
            }

            using (MARSEntities db = new MARSEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == useremail && u.Password == password);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Invalid Email or Password.";
                    return View();
                }

                if (user.Status == "Pending")
                {
                    ViewBag.ErrorMessage = "Your account is pending Admin approval.";
                    return View();
                }

                if (user.Status == "Rejected" || user.Status == "Inactive")
                {
                    ViewBag.ErrorMessage = "Your account has been deactivated. Contact Admin.";
                    return View();
                }

                // Save session info
                int userId = user.UserID;
                Session["UserID"] = userId;
                Session["UserName"] = user.Name;
                Session["Email"] = user.Email;
                Session["UsertypeID"] = user.UsertypeID;
                Session["UserType"] = user.UserType.UserTypeName;

                Session["HasOrder"] = db.Orders.Any(o => o.CustomerID == userId);
                Session["HasRepair"] = db.RepairRequests.Any(r => r.CustomerID == userId);

                var cart = db.Carts.FirstOrDefault(c => c.CustomerID == userId);
                Session["CartCount"] = cart != null ? cart.CartItems.Count : 0;

                // Redirect by role — auto detect
                switch (user.UsertypeID)
                {
                    case 4: return RedirectToAction("AdminDashboard", "Dashboard");
                    case 5: return RedirectToAction("Index", "CustomerProduct");
                    case 6: return RedirectToAction("EmployeeDashboard", "Dashboard");
                    default: return RedirectToAction("Login", "Home");
                }
            }
        }

        // -----------------------------------------------
        // Logout
        // -----------------------------------------------
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        // -----------------------------------------------
        // Home
        // -----------------------------------------------
        public ActionResult Home()
        {
            return View();
        }

        // =====================================================
        //  PASSWORD RESET FLOW
        // =====================================================

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == Email);
            if (user != null)
            {
                string resetCode = Guid.NewGuid().ToString();
                bool sent = SendResetEmail(user.Email, user.Name, resetCode);

                ViewBag.Message = sent
                    ? "Password reset link sent! Please check your email."
                    : "Could not send email. Please try again later.";
            }
            else
            {
                ViewBag.Message = "No account found with this email address.";
            }

            return View();
        }

        private bool SendResetEmail(string toEmail, string userName, string resetCode)
        {
            try
            {
                string fromEmail = "abdolgafar087@gmail.com";
                string appPassword = "xsuc tlhi xouf dumg";

                string resetLink = Url.Action("ResetPassword", "Home",
                    new { email = toEmail, code = resetCode }, Request.Url.Scheme);

                string body = $@"
                    <div style='font-family:sans-serif;max-width:480px;margin:auto;'>
                        <h2 style='color:#C0392B;'>MARS System</h2>
                        <p>Hi <strong>{userName}</strong>,</p>
                        <p>Click the button below to reset your password:</p>
                        <a href='{resetLink}'
                           style='display:inline-block;padding:12px 28px;background:#C0392B;
                                  color:white;border-radius:8px;text-decoration:none;font-weight:600;'>
                           Reset Password
                        </a>
                        <p style='margin-top:20px;color:#888;font-size:12px;'>
                            If you didn't request this, ignore this email.
                        </p>
                    </div>";

                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true
                };

                var msg = new MailMessage();
                msg.From = new MailAddress(fromEmail, "MARS System");
                msg.To.Add(toEmail);
                msg.Subject = "Reset Your MARS Password";
                msg.Body = body;
                msg.IsBodyHtml = true;

                smtp.Send(msg);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Email Error: " + ex.Message);
                return false;
            }
        }

        [HttpGet]
        public ActionResult ResetPassword(string email, string code)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                ViewBag.Message = "Invalid or expired reset link.";
                return View();
            }
            ViewBag.Email = email;
            ViewBag.Code = code;
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string email, string code, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "Passwords do not match. Please try again.";
                ViewBag.Email = email;
                ViewBag.Code = code;
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.Password = newPassword;
                db.SaveChanges();
                ViewBag.Message = "Password reset successfully! You can now login.";
            }
            else
            {
                ViewBag.Message = "Invalid email or link expired.";
            }

            return View();
        }
    }
}