using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models.ViewModels;
using Models;
using Ultility;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;

namespace OnlineShop4DVDS.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
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
                    await _userManager.AddToRoleAsync(applicationUser, roleNameUpdated);
                }

                userUpdated.Name = applicationUser.Name;
                userUpdated.PhoneNumber = applicationUser.PhoneNumber;
                userUpdated.StreetAddress = applicationUser.StreetAddress;
                userUpdated.City = applicationUser.City;
                userUpdated.State = applicationUser.State;
                userUpdated.PostalCode = applicationUser.PostalCode;
                await _dbContext.SaveChangesAsync();

                if (!roleCheck.Contains("Customer"))
                {
                    TempData["success"] = "Account update successful";
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

                    // Logout if user is the current user
                    if (userUpdated.Id == _userManager.GetUserId(User))
                    {
                        await _signInManager.SignOutAsync();
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
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

        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        //Add error to ModelState
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
