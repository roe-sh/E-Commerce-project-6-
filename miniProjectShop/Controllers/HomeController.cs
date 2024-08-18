using miniProjectShop.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace miniProjectShop.Controllers
{
    public class HomeController : Controller
    {
        private duckshopEntities db = new duckshopEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Cart()
        {
            // Retrieve the cart items from the session
            var cartItems = Session["CartItems"] as List<CartItem>;
            if (cartItems == null)
            {
                cartItems = new List<CartItem>();
            }

            return View(cartItems);
        }

        public ActionResult RemoveFromCart(int id)
        {
            // Retrieve the cart items from the session
            var cartItems = Session["CartItems"] as List<CartItem>;
            if (cartItems != null)
            {
                // Find the item to remove by its CartItemID
                var itemToRemove = cartItems.FirstOrDefault(c => c.CartItemID == id);
                if (itemToRemove != null)
                {
                    cartItems.Remove(itemToRemove);
                    // Update the session with the modified cart
                    Session["CartItems"] = cartItems;
                }
            }

            return RedirectToAction("Cart");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int quantity)
        {
            // Retrieve the product from the database
            var product = db.Products.Find(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Retrieve the cart from the session, or create a new one if it doesn't exist
            var cartItems = Session["CartItems"] as List<CartItem>;
            if (cartItems == null)
            {
                cartItems = new List<CartItem>();
            }

            // Generate a unique CartItemID (use a more sophisticated method if needed)
            int cartItemId = cartItems.Count > 0 ? cartItems.Max(c => c.CartItemID) + 1 : 1;

            // Check if the product is already in the cart
            var existingCartItem = cartItems.FirstOrDefault(c => c.ProductID == productId);
            if (existingCartItem != null)
            {
                // Update the quantity if the product is already in the cart
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // Add the product to the cart if it's not already there
                var cartItem = new CartItem
                {
                    CartItemID = cartItemId, // Assign a unique CartItemID
                    ProductID = product.ProductID,
                    Product = product,
                    Quantity = quantity,
                    CreatedAt = DateTime.Now
                };

                cartItems.Add(cartItem);


                // Save the cart back to the session
                Session["CartItems"] = cartItems;

                return RedirectToAction("Cart", "Home");
            }

            // Save the cart back to the session
            Session["CartItems"] = cartItems;

            return RedirectToAction("Cart", "Home");
        }

        public ActionResult Profilepage()
        {
            // Get the current logged-in user's email
            string email = User.Identity.Name;
            User user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profilepage(User model)
        {
            if (ModelState.IsValid)
            {
                // Get the current logged-in user's email
                string email = User.Identity.Name;
                User user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Update the fields that you allow to be edited
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                // Add other fields as necessary, but be careful with fields like Email and Password

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                ViewBag.Message = "Profile updated successfully!";
                return RedirectToAction("Profilepage"); // Redirect to the same action to show the updated profile
            }

            return View(model);
        }


        public ActionResult About()
        {
          
            return View();
        }

        public ActionResult Shop()
        {
            var products = db.Products.ToList() ?? new List<Product>();
            return View(products);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        public ActionResult Contact()
        {
            
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User model)
        {
            if (ModelState.IsValid)
            {
                // Log the email and password for debugging (be cautious with logging plain text passwords)
                System.Diagnostics.Debug.WriteLine($"Email: {model.Email}, Password: {model.PasswordHash}");

                // Find the user by email and plain text password (no hashing)
                var user = db.Users.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == model.PasswordHash);
                if (user != null)
                {
                    // If user is found, set the session variables
                    Session["UserID"] = user.UserID;
                    Session["UserEmail"] = user.Email;
                    Session["UserName"] = user.FirstName + " " + user.LastName;

                    // Redirect to the home page or wherever appropriate
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    // If authentication fails, set an error message
                    TempData["ErrorMessage"] = "Invalid email or password.";
                }
            }

            // If we got this far, something failed, redisplay the form
            return View(model);
        }

        // GET: Home/Register
        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Directly save the plain text password (not recommended for production)
                    db.Users.Add(user);
                    db.SaveChanges();

                    // Redirect to the login page after successful registration
                    return RedirectToAction("Login", "Home");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            // Log the error details - you can also use a logging framework here
                            System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }

                    // Add a general error message for the user
                    ModelState.AddModelError("", "There was an error saving the user. Please check the input values.");
                }
            }

            return View(user);
        }


    }
}