using DataAccess.Data;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Mono.TextTemplating;
using System.Data;
using Ultility;

namespace OnlineShop4DVDS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "NotCustomerPermission")]
    public class UserController : Controller
    {
        private readonly DatabaseContext _dbContext;

        private readonly UserManager<IdentityUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly IEmailSender _emailSender;

        public UserController(DatabaseContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        public async Task<IActionResult> Index()
        {
            var roles = await _dbContext.Roles.Where(r => r.Name != StaticDetail.Role_Customer).Select(r => r.Name).Distinct().ToListAsync();
            ViewBag.Roles = roles;
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            CreateUserViewModel createUserViewModel = new();
            var roles = await _dbContext.ApplicationRoles.Where(r => r.Name != StaticDetail.Role_Customer && r.Status).ToListAsync();
            ViewBag.RoleList = new SelectList(roles, "Id", "Name", "Id");
            return View(createUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel createUserViewModel)
        {
            if (ModelState.IsValid)
            {
                // Add user
                var user = new ApplicationUser
                {
                    UserName = createUserViewModel.Email,
                    Email = createUserViewModel.Email,
                    Name = createUserViewModel.Name,
                    PhoneNumber = createUserViewModel.PhoneNumber,
                    StreetAddress = createUserViewModel.StreetAddress,
                    City = createUserViewModel.City,
                    State = createUserViewModel.State,
                    PostalCode = createUserViewModel.PostalCode
                };

                var result = await _userManager.CreateAsync(user, createUserViewModel.Password);

                if (result.Succeeded)
                {
                    // Add role
                    var role = await _roleManager.FindByIdAsync(createUserViewModel.RoleId);
                    if (role == null)
                    {
                        return NotFound();
                    }

                    var roleName = role.Name;

                    var resultCheck = await _userManager.AddToRoleAsync(user, roleName);
                    var tokenConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callBackUrl = Url.Action("ConfirmEmail", "Account", new { area = "Customer", userId = user.Id, code = tokenConfirm }, protocol: HttpContext.Request.Scheme);
                    string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Confirm Your Email</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: #007bff;
            color: #ffffff;
            padding: 10px 20px;
            text-align: center;
        }}
        .content {{
            margin: 20px 0;
            line-height: 1.6;
        }}
        .content a {{
            color: #007bff;
            text-decoration: none;
        }}
        .footer {{
            text-align: center;
            color: #888888;
            font-size: 12px;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Email Confirmation</h1>
        </div>
        <div class='content'>
            <p>Hi {user.UserName},</p>
            <p>Thank you for registering. Please confirm your email by clicking the link below:</p>
            <p><a href='{callBackUrl}' target='_blank'>Confirm your email</a></p>
            <p>If you did not register, please ignore this email or contact support if you have questions.</p>
            <p>Thank you,<br/>OnlineShop4DVDS</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} OnlineShop4DVDS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
                    await _emailSender.SendEmailAsync(createUserViewModel.Email, "Email Confirmation", emailBody);
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["swalsuccess"] = "Create user successful. Please confirm email";
                    return RedirectToAction("Index", "User", new { area = "Admin" });
                }

                AddErrors(result);
            }
            var roles = await _dbContext.ApplicationRoles.Where(r => r.Name != StaticDetail.Role_Customer && !r.Status).ToListAsync();
            ViewBag.RoleList = new SelectList(roles, "Id", "Name", "Id");
            return View(createUserViewModel);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [Authorize(Policy = "AdminOrOnlySelfPermission")]
        public async Task<IActionResult> Edit(string userId)
        {
            var roles = await _dbContext.ApplicationRoles.Where(r => r.Name != StaticDetail.Role_Customer && r.Status).ToListAsync();
            ViewBag.RoleList = new SelectList(roles, "Id", "Name", "Id");

            var userRoles = await _dbContext.UserRoles.ToListAsync();

            var userUpdated = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (userUpdated == null)
            {
                return NotFound();
            }

            var role = userRoles.FirstOrDefault(r => r.UserId == userUpdated.Id);
            if (role != null)
            {
                var roleInfo = roles.FirstOrDefault(u => u.Id == role.RoleId);
                if (roleInfo != null)
                {
                    userUpdated.RoleId = roleInfo.Id;
                    userUpdated.Role = roleInfo.Name;
                }
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

                var user = await _userManager.FindByIdAsync(userUpdated.Id);
                var roleCheck = await _userManager.GetRolesAsync(user);
                if (!roleCheck.Contains(StaticDetail.Role_Customer))
                {
                    var userRole = await _dbContext.UserRoles.FirstOrDefaultAsync(u => u.UserId == userUpdated.Id);
                    if (userRole != null)
                    {
                        var roleNameRemoved = await _dbContext.Roles.Where(r => r.Id == userRole.RoleId).Select(c => c.Name).FirstOrDefaultAsync();
                        await _userManager.RemoveFromRoleAsync(userUpdated, roleNameRemoved);
                    }

                    var roleNameUpdated = await _dbContext.Roles.Where(r => r.Id == applicationUser.RoleId).Select(c => c.Name).FirstOrDefaultAsync();
                    var result = await _userManager.AddToRoleAsync(applicationUser, roleNameUpdated);
					await _userManager.UpdateSecurityStampAsync(user);

				}

                userUpdated.Name = applicationUser.Name;
                userUpdated.PhoneNumber = applicationUser.PhoneNumber;
                userUpdated.StreetAddress = applicationUser.StreetAddress;
                userUpdated.City = applicationUser.City;
                userUpdated.State = applicationUser.State;
                userUpdated.PostalCode = applicationUser.PostalCode;
                var resultCheck = await _dbContext.SaveChangesAsync();

                if (!roleCheck.Contains("Customer"))
                {
                    TempData["swalsuccess"] = "Account update successful. Your changes have been saved";
                    return RedirectToAction("Index", "User", new { area = "Admin" });
                }

                TempData["swalsuccess"] = "Account update successful. Your changes have been saved";
                return RedirectToAction("Edit", "User", new { area = "Customer", userId = userUpdated.Id });
            }
            var roles = await _dbContext.ApplicationRoles.Where(r => r.Name != StaticDetail.Role_Customer && r.Status).ToListAsync();
            ViewBag.RoleList = new SelectList(roles, "Id", "Name", "Id");
            return View(applicationUser);
        }

//        [Authorize(Policy = "AdminOrOnlySelfPermission")]
//        public async Task<IActionResult> ChangeEmailAddress(string userId)
//        {
//            var userUpdated = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
//            if (userUpdated == null)
//            {
//                return NotFound();
//            }

//            ChangeEmailAddressViewModel changeEmailAddressViewModel = new()
//            {
//                UserId = userId,
//                OldEmailAddress = userUpdated.Email
//            };

//            return View(changeEmailAddressViewModel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> ChangeEmailAddress(ChangeEmailAddressViewModel changeEmailAddressViewModel)
//        {
//            if (ModelState.IsValid)
//            {
//                var userUpdated = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == changeEmailAddressViewModel.UserId);
//                if (userUpdated == null)
//                {
//                    return NotFound();
//                }

//                var passwordCheck = await _userManager.CheckPasswordAsync(userUpdated, changeEmailAddressViewModel.Password);
//                if (!passwordCheck)
//                {
//                    ModelState.AddModelError(string.Empty, "Incorrect password");
//                    return View(changeEmailAddressViewModel);
//                }

//                var emailExistInDatabase = await _dbContext.ApplicationUsers.AnyAsync(u => u.Email == changeEmailAddressViewModel.NewEmailAddress);

//                if (emailExistInDatabase)
//                {
//                    ModelState.AddModelError(string.Empty, "The email address is already registered.");
//                    return View(changeEmailAddressViewModel);
//                }


//                if (changeEmailAddressViewModel.NewEmailAddress.Equals(changeEmailAddressViewModel.OldEmailAddress))
//                {
//                    ModelState.AddModelError(string.Empty, "The new email address must be different from the old email address");
//                    return View(changeEmailAddressViewModel);
//                }

//                userUpdated.Email = changeEmailAddressViewModel.NewEmailAddress;
//                userUpdated.UserName = changeEmailAddressViewModel.NewEmailAddress;
//                userUpdated.EmailConfirmed = false;
//                var result = await _userManager.UpdateAsync(userUpdated);

//                if (result.Succeeded)
//                {
//                    var tokenConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(userUpdated);
//                    var callBackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userUpdated.Id, code = tokenConfirm }, protocol: HttpContext.Request.Scheme);
//                    string emailBody = $@"
//<!DOCTYPE html>
//<html>
//<head>
//    <meta charset='UTF-8'>
//    <title>Confirm Your Email</title>
//    <style>
//        body {{
//            font-family: Arial, sans-serif;
//            background-color: #f4f4f4;
//            margin: 0;
//            padding: 0;
//        }}
//        .container {{
//            width: 100%;
//            max-width: 600px;
//            margin: 0 auto;
//            background-color: #ffffff;
//            padding: 20px;
//            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
//        }}
//        .header {{
//            background-color: #007bff;
//            color: #ffffff;
//            padding: 10px 20px;
//            text-align: center;
//        }}
//        .content {{
//            margin: 20px 0;
//            line-height: 1.6;
//        }}
//        .content a {{
//            color: #007bff;
//            text-decoration: none;
//        }}
//        .footer {{
//            text-align: center;
//            color: #888888;
//            font-size: 12px;
//            margin-top: 20px;
//        }}
//    </style>
//</head>
//<body>
//    <div class='container'>
//        <div class='header'>
//            <h1>Email Confirmation</h1>
//        </div>
//        <div class='content'>
//            <p>Hi {userUpdated.UserName},</p>
//            <p>Thank you for registering. Please confirm your email by clicking the link below:</p>
//            <p><a href='{callBackUrl}' target='_blank'>Confirm your email</a></p>
//            <p>If you did not register, please ignore this email or contact support if you have questions.</p>
//            <p>Thank you,<br/>OnlineShop4DVDS</p>
//        </div>
//        <div class='footer'>
//            <p>&copy; {DateTime.Now.Year} OnlineShop4DVDS. All rights reserved.</p>
//        </div>
//    </div>
//</body>
//</html>";
//                    await _emailSender.SendEmailAsync(changeEmailAddressViewModel.NewEmailAddress, "Email Confirmation", emailBody);
//                    await _signInManager.SignInAsync(userUpdated, isPersistent: false);
//                    TempData["swalsuccess"] = "Email update successful. Please confirm your email";

//                    var user = await _userManager.FindByIdAsync(userUpdated.Id);
//                    var roleCheck = await _userManager.GetRolesAsync(user);

//                    if (!roleCheck.Contains("Customer"))
//                    {
//                        return RedirectToAction("Index", "User", new { area = "Admin" });
//                    }

//                    return RedirectToAction("ChangeEmailAddress", "User", new { area = "Customer", userId = userUpdated.Id });

//                }
//                AddErrors(result);
//            }
//            return View(changeEmailAddressViewModel);
//        }

        [Authorize(Policy = "AdminOrOnlySelfPermission")]
        public async Task<IActionResult> ChangePassword(string userId)
        {
            var userUpdated = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (userUpdated == null)
            {
                return NotFound();
            }

            ChangePasswordViewModel changePasswordViewModel = new()
            {
                UserId = userId
            };

            return View(changePasswordViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var userUpdated = await _userManager.FindByIdAsync(changePasswordViewModel.UserId);
                if (userUpdated == null)
                {
                    return NotFound();
                }

                // Check if new password is the same as the current password
                var passwordCheck = _userManager.PasswordHasher.VerifyHashedPassword(userUpdated, userUpdated.PasswordHash, changePasswordViewModel.Password);
                if (passwordCheck == PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError(string.Empty, "New password must be different from the current password");
                    ChangePasswordViewModel samePasswordError = new()
                    {
                        UserId = userUpdated.Id
                    };
                    return View(samePasswordError);
                }

                var result = await _userManager.ChangePasswordAsync(userUpdated, changePasswordViewModel.CurrentPassword, changePasswordViewModel.Password);

                if (result.Succeeded)
                {
                    TempData["swalsuccess"] = "Account update successful. Your password has been changed";

                    // Logout if the deleted user is the current user
                    if (userUpdated.Id == _userManager.GetUserId(User))
                    {
                        await _signInManager.SignOutAsync();
                        return RedirectToAction("Index", "Home", new { area = "Customer"});
                    }
                    else
                    {
                        var user = await _userManager.FindByIdAsync(userUpdated.Id);
                        var roleCheck = await _userManager.GetRolesAsync(user);

                        if (!roleCheck.Contains("Customer"))
                        {
                            TempData["success"] = "Change password successful";
                            return RedirectToAction("Index", "User", new { area = "Admin" });
                        }

                        return RedirectToAction("ChangePassword", "User", new { area = "Customer", userId = userUpdated.Id });
                    }
                }
                AddErrors(result);
                ChangePasswordViewModel changePasswordError = new()
                {
                    UserId = userUpdated.Id,
                };
                return View(changePasswordError);

            }
            ChangePasswordViewModel modelStateInvalid = new()
            {
                UserId = changePasswordViewModel.UserId,
            };
            return View(modelStateInvalid);
        }

        [Authorize(Policy = "AdminOrOnlySelfPermission")]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        //Add error of creating user to View Create
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll(string role)
        {
            var userList = await _dbContext.ApplicationUsers.Where(u => u.Email != "admin@admin.com").ToListAsync();
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
                .Where(u => u.Role != StaticDetail.Role_Customer)
                .Where(u => string.IsNullOrEmpty(role) || u.Role == role)
                .Select(u => new
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    LockOutEnd = u.LockoutEnd.HasValue ? u.LockoutEnd.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null
                });

            return Json(new { data = userData });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> ChangeStatus(string userId)
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            // Check if the user is an current user
            if (currentUser.Id == userId)
            {
                return Json(new { success = false, message = "You cannot block yourself" });
            }

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
                if (userUpdated.Id == _userManager.GetUserId(User))
                {
                    await _signInManager.SignOutAsync();
                }
            }

            await _dbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Status changed successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string userId)
        {
            var userDeleted = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (userDeleted == null)
            {
                return NotFound();
            }

            //Check if the user is an current user
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (currentUser.Id == userId)
            {
                return Json(new { success = false, message = "You cannot delete yourself" });
            }

            // Check if the user is an admin
            var userRoles = await _userManager.GetRolesAsync(userDeleted);
            if (userRoles.Contains(StaticDetail.Role_Admin))
            {
                // Check if this is the last admin
                var adminRoleId = _dbContext.Roles.FirstOrDefault(r => r.Name == StaticDetail.Role_Admin).Id;
                if(adminRoleId != null)
                {
                    var adminCount = await _dbContext.UserRoles.CountAsync(ur => ur.RoleId == adminRoleId);
                    if (adminCount == 2)
                    {
                        return Json(new { success = false, message = "Cannot delete the last admin account" });
                    }
                }
            }

            var result = await _userManager.DeleteAsync(userDeleted);
            if (result.Succeeded)
            {
                // Logout if the deleted user
                if (userDeleted.Id == _userManager.GetUserId(User))
                {
                    await _signInManager.SignOutAsync();
                    return Json(new { success = true, message = "User deleted successfully", logout = true });
                }
                else
                {
                    return Json(new { success = true, message = "User deleted successfully" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Error deleting user" });
            }
        }

        #endregion
    }
}
