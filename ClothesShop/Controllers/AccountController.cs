using ClothesShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClothesShop.Models;
using ClothesShop.Repository;
using System.Security.Claims;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClothesShop.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;
        private IUserProfileRepository profileRepository;

        public AccountController(UserManager<IdentityUser> userMgr,
            SignInManager<IdentityUser> signInMgr, IUserProfileRepository userRepo)

        {
            userManager = userMgr;
            signInManager = signInMgr;
            profileRepository = userRepo;
        }

        public ViewResult Login(string returnUrl)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
                if (ModelState.IsValid)
            {
                IdentityUser user = await userManager.FindByNameAsync(loginViewModel.Name);
                if(user != null)
                {
                    await signInManager.SignOutAsync();
                    var result = await signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(loginViewModel?.ReturnUrl ?? "/");
                    }
                }
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(loginViewModel);
        }


        public ViewResult Register(string returnUrl)
        {
            return View(new RegisterViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await userManager.FindByNameAsync(registerViewModel.Name);
                if (user != null)
                {

                    ModelState.AddModelError("", $"User with {registerViewModel.Name} already exists!");
                    return View(registerViewModel);
                }

                string userName = Regex.Replace(registerViewModel?.Name?.Split('@').First(), "[^a-zA-Z0-9]", x => string.Empty);

                user = new IdentityUser(userName)
                {
                    Email = registerViewModel.Name,
                    LockoutEnabled = false,
                    EmailConfirmed = true,
                    TwoFactorEnabled = false
                };
                var result = await userManager.CreateAsync(user, registerViewModel.Password);

                if(result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    var userProfile = new UserProfile();
                    userProfile.AccountId = user.Id;
                    userProfile.Email = user.Email;
                    profileRepository.SaveProfile(userProfile);
                    return Redirect(registerViewModel?.ReturnUrl ?? "/Account/Login");
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(registerViewModel);
                }
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(registerViewModel);
        }


        [Authorize]
        public async Task<RedirectResult> Logout(string returnUrl = "/")
        {
            await signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }

        [HttpGet]
        public ViewResult EditProfile(string returnUrl)
        {
            string email = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Sorry, you must be logged in!");
                Response.Redirect("/Account/Login");
                return View();
            }

            var user = userManager.FindByNameAsync(email).Result;

            if (user == null)
            {
                ModelState.AddModelError("", "Sorry, you must be logged in!");
                return View();
            }

            var userProfile = profileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                ModelState.AddModelError("", "Sorry, you must be logged in!");
                return View();
            }
            return View(userProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile([FromForm]UserProfile profile)
        {
            if (ModelState.IsValid)
            {
                var userProfile = profileRepository.UserProfiles.FirstOrDefault(x => x.Id == profile.Id);

                if(userProfile == null)
                {
                    ModelState.AddModelError("", "Invalid user profile");
                    return View(profile);
                }

                //userProfile.Name = profile.Name;
                userProfile.Address = profile.Address;
                userProfile.City = profile.City;
                userProfile.Country = profile.Country;
                userProfile.State = profile.State;
                userProfile.Zip = profile.Zip;

                profileRepository.SaveProfile(userProfile);

                return View(userProfile);
            }

            ModelState.AddModelError("", "Invalid user profile");
            return View(profile);
        }



    }
}
