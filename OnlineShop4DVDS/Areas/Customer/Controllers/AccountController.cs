using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using System.Security.Claims;
using Ultility;

namespace Identity.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Register()
        {
            RegisterViewModel registerViewModel = new();
            return View(registerViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = registerViewModel.Email,
                    Email = registerViewModel.Email,
                    Name = registerViewModel.Name,
                    PhoneNumber = registerViewModel.PhoneNumber,
                    StreetAddress = registerViewModel.StreetAddress,
                    City = registerViewModel.City,
                    State = registerViewModel.State,
                    PostalCode = registerViewModel.PostalCode
                };

                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, StaticDetail.Role_Customer);
                    var tokenConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callBackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = tokenConfirm }, protocol: HttpContext.Request.Scheme);
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
                    await _emailSender.SendEmailAsync(registerViewModel.Email, "Email Confirmation", emailBody);
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["swalsuccess"] = "Registration successful. Please confirm your email";
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }

                AddErrors(result);
            }

            return View(registerViewModel);
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
            if (result.Succeeded)
            {
                if (User.Identity.IsAuthenticated)
                {
                    await _signInManager.SignOutAsync();
                }

                return RedirectToAction("ConfirmEmailReload", new { userId, code });
            }
            else
            {
                return View("Error");
            }
            
        }

        public async Task<IActionResult> ConfirmEmailReload(string userId, string code)
        {
            return View("ConfirmEmail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Check if returnUrl contains specific paths
            if (!string.IsNullOrEmpty(returnUrl) &&
                (returnUrl.Contains("Customer/Account/ConfirmEmail", StringComparison.OrdinalIgnoreCase) ||
                 returnUrl.Contains("Customer/Account/ResetPassword", StringComparison.OrdinalIgnoreCase)))
            {
                returnUrl = Url.Content("~/");
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(loginViewModel.Email);

                if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    TempData["swalerror"] = "You need to confirm your email before you can log in";
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }

                var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (!roles.Contains("Customer"))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    return LocalRedirect(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    TempData["swalerror"] = "Too many attempts, please try again later.";
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
                else
                {
                    TempData["swalerror"] = "Invalid login attempt.";
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
            }

            var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["swalerror"] = string.Join("<br>", errorMessages);

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel, string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Check if returnUrl contains specific paths
            if (!string.IsNullOrEmpty(returnUrl) &&
                (returnUrl.Contains("Customer/Account/ConfirmEmail", StringComparison.OrdinalIgnoreCase) ||
                 returnUrl.Contains("Customer/Account/ResetPassword", StringComparison.OrdinalIgnoreCase)))
            {
                returnUrl = Url.Content("~/");
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordViewModel.Email);
                if (user == null)
                {
                    TempData["swalerror"] = "Invalid login attempt";
                    return LocalRedirect(returnUrl);
                }

                if (!user.EmailConfirmed)
                {
                    TempData["swalerror"] = "Password reset is not available because your email has not been confirmed";
                    return LocalRedirect(returnUrl);
                }

                var tokenReset = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callBackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = tokenReset }, protocol: HttpContext.Request.Scheme);

                string emailBody = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>Reset Your Password</title>
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
                    <h1>Password Reset Request</h1>
                </div>
                <div class='content'>
                    <p>Hi {user.UserName},</p>
                    <p>We received a request to reset your password. Click the link below to reset your password:</p>
                    <p><a href='{callBackUrl}' target='_blank'>Reset your password</a></p>
                    <p>If you did not request a password reset, please ignore this email or contact support if you have questions.</p>
                    <p>Thank you,<br/>OnlineShop4DVDS</p>
                </div>
                <div class='footer'>
                    <p>&copy; {DateTime.Now.Year} OnlineShop4DVDS. All rights reserved.</p>
                </div>
            </div>
        </body>
        </html>";

                await _emailSender.SendEmailAsync(forgotPasswordViewModel.Email, "Reset Password", emailBody);
                TempData["swalsuccess"] = "Reset email has been sent";
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        public async Task<IActionResult> ResetPassword(string? code)
        {
            if(code == null)
            {
                return View("Error");
            }

            if (User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
            }

            return RedirectToAction("ResetPasswordReload", new { code });
        }

        public async Task<IActionResult> ResetPasswordReload(string code)
        {
            return View("ResetPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
                if (user == null)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Code, resetPasswordViewModel.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
                AddErrors(result);

            }
            return View();
        }

        public async Task<IActionResult> ResetPasswordConfirmation()
        {
            await _signInManager.SignOutAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider)
        {
            // Request to Google
            var redirectUrl = Url.Action("ExternalLoginCallBack", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallBack(string? remoteError)
        {

            if (remoteError != null)
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in user with Google login provider
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded) // User already has login by using Google before
            {
                // Update authentication tokens
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
            else // User has login by using Google for the first time
            {
                ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email, Name = name });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel externalLoginConfirmationViewModel)
        {
            if (ModelState.IsValid)
            {
                // Get the info about the user from external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("Error");
                }
                var user = new ApplicationUser
                {
                    UserName = externalLoginConfirmationViewModel.Email,
                    Email = externalLoginConfirmationViewModel.Email,
                    Name = externalLoginConfirmationViewModel.Name
                };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, StaticDetail.Role_Customer); // Tạo role customer cho user
                    var tokenConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(user); // Tạo token confirm email cho user
                    result = await _userManager.ConfirmEmailAsync(user, tokenConfirm);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            // Update authentication tokens
                            await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                            return RedirectToAction("Index", "Home", new { area = "Customer" });
                        }
                        AddErrors(result);
                    }
                    AddErrors(result);
                }
                AddErrors(result);
            }

            return View(externalLoginConfirmationViewModel);
        }

        //Add error of creating user to View Create
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}
