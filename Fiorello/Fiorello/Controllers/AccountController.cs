using Fiorello.Helpers;
using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Appuser> _userManager;
        private readonly SignInManager<Appuser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<Appuser> userManager, SignInManager<Appuser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM register)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            Appuser appUser = new Appuser
            {
                FullName=register.FullName,
                Email=register.Email,
                UserName=register.UserName,
                
            };
           IdentityResult identityResult =await _userManager.CreateAsync(appUser, register.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();

            }
            await _signInManager.SignInAsync(appUser, true);
            await _userManager.AddToRoleAsync(appUser, Helper.Roles.Member.ToString());
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM login)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            Appuser user = await _userManager.FindByNameAsync(login.UserName);
            if (user==null)
            {
                ModelState.AddModelError("", "Username or Userpassword has error");
                return View();
            }
            if (user.IsDeactive)
            {
                ModelState.AddModelError("", "This account has been blocked");
                return View();

            }
            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, login.Password, true, true);
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("UserName", "This account has been blocked for 1min");
                return View();
            }
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Username or Userpassword has error");
                return View();
            }
            return RedirectToAction("Index", "Home");

        }
        //public async Task CreateRolesAsync() 
        //{
        //    if (!(await _roleManager.RoleExistsAsync(Helper.Roles.Admin.ToString())))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole { Name = Helper.Roles.Admin.ToString() });
        //    }
        //    if (!(await _roleManager.RoleExistsAsync(Helper.Roles.Member.ToString())))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole { Name = Helper.Roles.Member.ToString() });
        //    }
        //    if (!(await _roleManager.RoleExistsAsync(Helper.Roles.Manager.ToString())))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole { Name = Helper.Roles.Manager.ToString() });
        //    }
        //}
    }
}
 