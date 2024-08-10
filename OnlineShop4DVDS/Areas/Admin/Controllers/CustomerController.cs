using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Models;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class CustomerController : Controller
    {
        private readonly DatabaseContext _dbContext;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly SignInManager<IdentityUser> _signInManager;

        public CustomerController(DatabaseContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        public async Task<IActionResult> Edit(string userId)
        {
            var userRoles = await _dbContext.UserRoles.ToListAsync();

            var userUpdated = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (userUpdated == null)
            {
                return NotFound();
            }

            return View(userUpdated);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                var userUpdated = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == applicationUser.Id);
                if (userUpdated == null)
                {
                    return NotFound();
                }

                userUpdated.Name = applicationUser.Name;
                userUpdated.PhoneNumber = applicationUser.PhoneNumber;
                userUpdated.StreetAddress = applicationUser.PhoneNumber;
                userUpdated.City = applicationUser.City;
                userUpdated.State = applicationUser.State;
                userUpdated.PostalCode = applicationUser.PostalCode;

                await _dbContext.SaveChangesAsync();

                TempData["swalsuccess"] = "Customer update successful. Your changes have been saved";
                return RedirectToAction("Index", "Customer", new { area = "Admin" });
            }
            return View(applicationUser);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll(string role)
        {
            var userList = await _dbContext.ApplicationUsers.ToListAsync();
            var userRoles = await _dbContext.UserRoles.ToListAsync();
            var roles = await _dbContext.Roles.ToListAsync();
            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id)?.RoleId;
                if (roleId != null)
                {
                    user.Role = roles.FirstOrDefault(u => u.Id == roleId)?.Name;
                }
            }

            var userData = userList
                .Where(u => u.Role == StaticDetail.Role_Customer)
                .Select(u => new
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    LockOutEnd = u.LockoutEnd.HasValue ? u.LockoutEnd.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null
                });

            return Json(new { data = userData });
        }

        [Authorize(Policy = "AdminOrEditPermission")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(string userId)
        {
            var userUpdated = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (userUpdated == null)
            {
                return NotFound();
            }

            if (userUpdated.LockoutEnd != null && userUpdated.LockoutEnd > DateTime.UtcNow)
            {
                // Unlock user
                userUpdated.LockoutEnd = DateTime.UtcNow;
            }
            else
            {
                userUpdated.LockoutEnabled = true;
                userUpdated.LockoutEnd = DateTime.UtcNow.AddYears(1000);
            }

            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        #endregion
    }
}
