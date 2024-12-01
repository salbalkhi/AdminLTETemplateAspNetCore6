using Tadawi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Tadawi.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext _context, RoleManager<IdentityRole> roleManager ) : base(userManager, signInManager, _context, roleManager)
        {
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(UserLoginModel userLoginModel, bool adminLogin)
        {
            if (ModelState.IsValid || adminLogin)
            {
                ApplicationUser user = null;

                if (adminLogin)
                {
                    user = await userManager.FindByNameAsync("admin");
                }
                else
                {
                    user = await userManager.FindByEmailAsync(userLoginModel.Email);
                }

                if (user != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, userLoginModel.Password, userLoginModel.RememberMe, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Your password is incorrect.");
                        ViewBag.Message = "Your password is incorrect.";
                        return View(userLoginModel);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No user is registered with this email address.");
                    ViewBag.Message = "No user is registered with this email address.";
                }
            }

            return View(userLoginModel);
        }


        public IActionResult SignUp()
        {
            return View();
        }


        public IActionResult LogOut()
        {
            signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }


        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegisterModel userRegisterModel)
        {
            if (ModelState.IsValid)
            {
                if (userRegisterModel.UserName.ToLower() == "admin" || userRegisterModel.UserName.ToLower() == "administrator")
                {
                    ModelState.AddModelError("", "Registration with this username is not allowed.");
                    ViewBag.Message = "Registration with this username is not allowed.";
                    return View(userRegisterModel);
                }

                if (userManager.Users.Any(u => u.Email == userRegisterModel.Email))
                {
                    ModelState.AddModelError("", "This email address is already registered.");
                    ViewBag.Message = "This email address is already registered.";
                    return View(userRegisterModel);
                }

                if (userManager.Users.Any(u => u.PhoneNumber == userRegisterModel.PhoneNumber))
                {
                    ModelState.AddModelError("", "This phone number is already registered.");
                    ViewBag.Message = "This phone number is already registered.";
                    return View(userRegisterModel);
                }

                ApplicationUser user = new ApplicationUser();
                user.UserName = userRegisterModel.UserName;
                user.Email = userRegisterModel.Email;
                user.PhoneNumber = userRegisterModel.PhoneNumber;
                user.FirstName = userRegisterModel.FirstName;
                user.LastName = userRegisterModel.LastName;

                IdentityResult result = await userManager.CreateAsync(user, userRegisterModel.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Stuff");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Message = "Operation did not succeed.";
                    AddModelError(result);
                }
            }

            return View(userRegisterModel);
        }


        public ActionResult ForgetPassword()
        {
            ViewBag.Success = false;
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPassword(UserLoginModel userLoginModel)
        {
            //Add reset password logic here
            throw new NotImplementedException();
        }
    }
}
