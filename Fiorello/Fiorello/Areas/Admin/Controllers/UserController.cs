using Fiorello.Helpers;
using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<Appuser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(UserManager<Appuser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            List<Appuser> users = await _userManager.Users.ToListAsync();
            List<UserVM> userVMs = new List<UserVM>();
            foreach (Appuser user in users)
            {
                UserVM userVM = new UserVM
                {
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Id = user.Id,
                    IsDeactive = user.IsDeactive,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
                };
                userVMs.Add(userVM);
            }
            return View(userVMs);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterVM register)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            Appuser appUser = new Appuser
            {
                FullName = register.FullName,
                Email = register.Email,
                UserName = register.UserName,

            };
            IdentityResult identityResult = await _userManager.CreateAsync(appUser, register.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();

            }
            await _userManager.AddToRoleAsync(appUser, Helper.Roles.Member.ToString());

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ChangeRole(string id)
        {
            if (id==null)
            {
                return NotFound();
            }
            Appuser appUser = await _userManager.FindByIdAsync(id);
            if (appUser==null)
            {
                return BadRequest();

            }
            List<string> roles = new List<string>();
            roles.Add(Helper.Roles.Admin.ToString());
            roles.Add(Helper.Roles.Member.ToString());
            roles.Add(Helper.Roles.Manager.ToString());
            string oldRole = (await _userManager.GetRolesAsync(appUser)).FirstOrDefault();
            ChangeRoleVM changeRole = new ChangeRoleVM
            {
                Username = appUser.UserName,
                Role=oldRole,
                Roles=roles

            
            };
            return View(changeRole);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string id,string newRole)
        {
            if (id == null)
            {
                return NotFound();
            }
            Appuser appUser = await _userManager.FindByIdAsync(id);
            if (appUser == null)
            {
                return BadRequest();

            }
            List<string> roles = new List<string>();
            roles.Add(Helper.Roles.Admin.ToString());
            roles.Add(Helper.Roles.Member.ToString());
            roles.Add(Helper.Roles.Manager.ToString());
            string oldRole = (await _userManager.GetRolesAsync(appUser)).FirstOrDefault();

            ChangeRoleVM changeRole = new ChangeRoleVM
            {
                Username = appUser.UserName,
                Role = oldRole,
                Roles = roles


            };
            IdentityResult addIdentityResult = await _userManager.AddToRoleAsync(appUser , newRole);
            if (!addIdentityResult.Succeeded)
            {
                ModelState.AddModelError("", "Error");
                return View(changeRole);
            }
            IdentityResult removeIdentityResult = await _userManager.RemoveFromRoleAsync(appUser, oldRole);
            if (!removeIdentityResult.Succeeded)
            {
                ModelState.AddModelError("", "Error");
                return View(changeRole);
            }
            return RedirectToAction("Index");
        }
    }
}
