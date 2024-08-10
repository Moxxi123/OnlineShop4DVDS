using DataAccess.Data;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using System.Security.Claims;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly DatabaseContext _dbContext;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(DatabaseContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UpSert(string? roleId)
        {
            if (String.IsNullOrEmpty(roleId))
            {

                RoleClaimViewModel roleClaimViewModel = new();

                foreach (Claim claim in ClaimStore.claimList)
                {
                    RoleClaim userClaim = new()
                    {
                        ClaimType = claim.Type
                    };

                    roleClaimViewModel.Claims.Add(userClaim);
                }

                return View(roleClaimViewModel);
            }
            else
            {
                var role = await _dbContext.ApplicationRoles.FirstOrDefaultAsync(r => r.Id == roleId);

                if (role == null)
                {
                    return NotFound();
                }

                RoleClaimViewModel roleClaimViewModel = new()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Claims = ClaimStore.claimList.Select(claim => new RoleClaim
                    {
                        ClaimType = claim.Type,
                        IsSelected = _dbContext.RoleClaims.Any(rc => rc.RoleId == role.Id && rc.ClaimType == claim.Type)
                    }).ToList(),
                    Status = role.Status
                };

                return View(roleClaimViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpSert(RoleClaimViewModel roleClaimViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(roleClaimViewModel);
            }

            if (string.IsNullOrEmpty(roleClaimViewModel.RoleId))
            {
                // Create a new role
                ApplicationRole roleAdded = new()
                {
                    Name = roleClaimViewModel.RoleName.Substring(0, 1).ToUpper() + roleClaimViewModel.RoleName.Substring(1).ToLower(),
                    Status = true
                };

                var result = await _roleManager.CreateAsync(roleAdded);
                if (result.Succeeded)
                {
                    // Add claims to the new role
                    foreach (var claim in roleClaimViewModel.Claims.Where(c => c.IsSelected))
                    {
                        var claimResult = await _roleManager.AddClaimAsync(roleAdded, new Claim(claim.ClaimType, claim.ClaimType));
                        if (!claimResult.Succeeded)
                        {
                            // Handle the error and return the view with the model
                            foreach (var error in claimResult.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View(roleClaimViewModel);
                        }
                    }
                    TempData["success"] = "Role created successfully";
                    return RedirectToAction("Index");
                }

                // Handle the error and return the view with the model
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(roleClaimViewModel);
            }
            else
            {
                // Update existing role
                var role = await _roleManager.FindByIdAsync(roleClaimViewModel.RoleId);
                if (role == null)
                {
                    return NotFound();
                }

                role.Name = roleClaimViewModel.RoleName.Substring(0, 1).ToUpper() + roleClaimViewModel.RoleName.Substring(1).ToLower();
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    // Remove all existing claims
                    var existingClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var existingClaim in existingClaims)
                    {
                        await _roleManager.RemoveClaimAsync(role, existingClaim);
                    }

                    // Add new claims
                    foreach (var claim in roleClaimViewModel.Claims.Where(c => c.IsSelected))
                    {
                        var claimResult = await _roleManager.AddClaimAsync(role, new Claim(claim.ClaimType, claim.ClaimType));
                        if (!claimResult.Succeeded)
                        {
                            // Handle the error and return the view with the model
                            foreach (var error in claimResult.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View(roleClaimViewModel);
                        }
                    }
                    TempData["success"] = "Role updated successfully";
                    return RedirectToAction("Index");
                }

                // Handle the error and return the view with the model
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(roleClaimViewModel);
            }
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roleClaimViewModelList = new List<RoleClaimViewModel>();
            var roleList = await _dbContext.ApplicationRoles.Where(r => r.Name != StaticDetail.Role_Customer).ToListAsync();
            foreach (var role in roleList)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                RoleClaimViewModel roleClaimViewModel = new ()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Claims = claims.Select(c => new RoleClaim
                    {
                        ClaimType = c.Type,
                        IsSelected = true
                    }).ToList(),
                    Status = role.Status
                };

                roleClaimViewModelList.Add(roleClaimViewModel);
            }

            var roleData = roleClaimViewModelList.Select(r => new
            {
                Id = r.RoleId,
                Name = r.RoleName,
                Claims = string.Join(", ", r.Claims.Select(c => c.ClaimType)),
                Status = r.Status
            }).ToList();

            return Json(new { data = roleData });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(string roleId)
        {
            var roleUpdated = await _dbContext.ApplicationRoles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (roleUpdated == null)
            {
                return NotFound();
            }

            roleUpdated.Status = !roleUpdated.Status;
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string roleId)
        {
            var roleDeleted = await _dbContext.ApplicationRoles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (roleDeleted == null)
            {
                return NotFound();
            }

            var userHasRole = _dbContext.UserRoles.Any(u => u.RoleId == roleId);
            if (userHasRole)
            {
                return Json(new { success = false, message = "Role cannot be deleted because users are assigned to it" });
            }

            var result = await _roleManager.DeleteAsync(roleDeleted);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Role deleted successfully" });
            }
            else
            {
                return Json(new { success = false, message = "Error deleting role" });
            }
        }

        #endregion
    }
}
