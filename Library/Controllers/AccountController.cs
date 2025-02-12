using Library.Models;
using Library.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;

        }


        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task< IActionResult> Login(LoginVM model ,string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // login
                var results =  await signInManager.PasswordSignInAsync(model.Username!, model.Password!,model.RememberMe, false);
                if (results.Succeeded)
                {
                    //return RedirectToAction("Index", "Home");
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError("", "Inavalid Login Attempt");
                return View(model);
            }
            return View(model);
        }

        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task< IActionResult> Register(RegisterVM model,  string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                AppUser user = new()
                {
                    Name = model.Name,
                    UserName = model.Email,
                    Email = model.Email,
                    Address = model.Address
                };

                var result = await userManager.CreateAsync(user, model.Password!);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    return RedirectToLocal(returnUrl);
                    //return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task< IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> GenerateToken(AppUser user)
        {
            var token = GenerateRefreshToken();
            var result = await userManager.SetAuthenticationTokenAsync(user, "YourProvider", "RefreshToken", token);

      
            if (result.Succeeded)
            {
                return Ok("Token stored successfully!");
            }

            return BadRequest("Token storage failed!");
        }

        private string GenerateRefreshToken()
        {
            
            return Guid.NewGuid().ToString();
        }




        private IActionResult RedirectToLocal(string? returnUrl)
        {
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(HomeController.Index),"Home");
        }

    }
}
