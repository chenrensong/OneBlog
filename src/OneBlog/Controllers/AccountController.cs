using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneBlog.Data;
using OneBlog.Uploader;
using OneBlog.Helpers;
using OneBlog.ViewModels;
using OneBlog.Services;
using SS.Toolkit.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMailService _mailService;
        private readonly ILogger _logger;

        public AccountController(AppDbContext context,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IMailService mailService,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _signInManager = signInManager;
            _mailService = mailService;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }


        /// <summary>
        /// 禁止访问
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("AccessDenied")]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            return RedirectToAction("Index", "Root", new { area = "" });
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                return RedirectToLocal(null);
                //return RedirectToAction(nameof(AdminController.Index), "Admin", new { area = "Admin" });
                //return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }
            LoginViewModel model = new LoginViewModel();
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("login")]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    //return Json(new { errno = 0, errmsg = "", returnUrl = returnUrl });
                    return RedirectToLocal(returnUrl);
                }
                ///目前不支持
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "该账户已经被锁定，请稍候尝试...");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "用户名或密码错误，请重试...");
                }
            }
            var modelErrors = ModelState.AllErrors();
            //return Json(new { errno = 1, errmsg = "", returnUrl = returnUrl, errorList = modelErrors });
            // If we got this far, something failed, redisplay form
            return View(model);
        }



        //
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        [Route("register")]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [Route("upload/{username?}")]

        public async Task<IActionResult> Upload(string username)
        {
            var fileUploader = IocContainer.Get<IFileUploder>();
            string avatar = string.Empty;
            if (Request.Form != null && Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files[0];
                var uploadFileBytes = new byte[file.Length];
                try
                {
                    await file.OpenReadStream().ReadAsync(uploadFileBytes, 0, (int)file.Length);
                }
                catch (Exception ex)
                {

                }
                var uploadResult = await fileUploader.InvokeAsync("", "/ugc/" + GuidHelper.Gen().ToString().Replace("-", ""), uploadFileBytes);
                avatar = uploadResult.Url;
                if (!string.IsNullOrEmpty(avatar))
                {
                    var user = await GetCurrentUserAsync();
                    user.Avatar = avatar;
                    await _userManager.UpdateAsync(user);
                }
            }
            return Ok(avatar);


            //long size = 0;
            //foreach (var file in Request.Form.Files)
            //{
            //    var filename = ContentDispositionHeaderValue
            //    .Parse(file.ContentDisposition)
            //    .FileName
            //    .Trim('"');
            //    if (string.IsNullOrEmpty(username))
            //    {
            //        var user = await GetCurrentUserAsync();
            //        username = user.UserName;
            //    }
            //    filename = _env.WebRootPath + $@"\static\img\avatar\{ Md5Helper.Parse(username)}";
            //    size += file.Length;
            //    var directory = Path.GetDirectoryName(filename);
            //    if (!Directory.Exists(directory))
            //    {
            //        Directory.CreateDirectory(directory);
            //    }
            //    using (FileStream fs = System.IO.File.Create(filename))
            //    {
            //        file.CopyTo(fs);
            //        fs.Flush();
            //    }
            //}
            //var photoUrl = Url.Action("Avatar", "Account", new { md5 = Md5Helper.Parse(username) });
            //return Ok(photoUrl);
        }


        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            bool isAjaxRequest = Request.IsAjaxRequest();
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new AppUser { UserName = model.Email, Email = model.Email, DisplayName = model.DisplayName };
                user.Avatar = $"/account/avatar/{SS.Toolkit.Helpers.SecurityHelper.MD5(model.Email)}";
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                    //    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User created a new account with password.");
                    if (isAjaxRequest)
                    {
                        return Json(new { errno = 0, errmsg = "", returnUrl = returnUrl });
                    }
                    else
                    {
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            if (isAjaxRequest)
            {
                var modelErrors = ModelState.AllErrors();
                return Json(new { errno = 1, errmsg = "", errorList = modelErrors });
            }
            return View(model);
        }

        //
        // POST: /Account/LogOff
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        [Route("logoff")]
        public async Task<IActionResult> LogOff(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToLocal(returnUrl);
            }
            return RedirectToAction("Index", "Root");
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                // Update any authentication tokens if login succeeded
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                //var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                //var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                //var portrait = info.Principal.FindFirstValue(ClaimTypes.Actor);
                var email = "";
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return View("ExternalLoginFailure");
            }
            //var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ////var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            //var portrait = info.Principal.FindFirstValue(ClaimTypes.Actor);
            //model.NickName = name;
            //model.Portrait = portrait;
            if (ModelState.IsValid)
            {

                var md5 = SecurityHelper.MD5(model.Email);//头像
                var user = new AppUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);

                        // Update any authentication tokens as well
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUser(string loginType, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { errno = 0, errmsg = "恭喜你，用户名可以使用" });
            }
            else
            {
                return Json(new { errno = 1, errmsg = email + "已被注册" });
            }
        }

        [Authorize]
        [HttpGet("setting")]
        public async Task<IActionResult> Setting()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Root");
            }
            var model = new SettingViewModel();
            model.Email = user.Email;
            model.DisplayName = user.DisplayName;
            model.Signature = user.Signature;
            model.Avatar = user.Avatar;
            model.Sex = user.Sex;
            model.Age = user.Age;
            return View(model);
        }

        [Authorize]
        [HttpPost("setting")]
        public async Task<IActionResult> Setting(SettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                user.DisplayName = model.DisplayName;
                user.Signature = model.Signature;
                if (model.Sex >= 1 && model.Sex <= 3)
                {
                    user.Sex = model.Sex;
                }
                user.Age = model.Age;
                await _userManager.UpdateAsync(user);
                return Json(new { ErrNo = 0, ErrMsg = "更新资料成功~" });
            }
            return Json(new { ErrNo = 1, ErrMsg = "更新资料失败,请重试~" });
        }

        [Authorize]
        [HttpGet("modifypassword")]
        public async Task<IActionResult> ModifyPassword()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Root");
            }
            var model = new ModifyPasswordViewModel();
            model.Email = user.Email;
            model.DisplayName = user.DisplayName;
            model.Signature = user.Signature;
            model.Avatar = user.Avatar;
            return View(model);
        }

        [Authorize]
        [HttpPost("modifypassword")]
        public async Task<IActionResult> ModifyPassword(ModifyPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.Password);
                if (result.Succeeded)
                {
                    return View("ModifyPasswordSuccess");
                }
                ModelState.AddModelError(string.Empty, "密码修改失败,请重试~");
            }
            return View(model);
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        [Route("forgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    //return View("ForgotPasswordConfirmation");
                    ModelState.AddModelError(string.Empty, "该邮箱未注册无法找回密码...");
                    //return Json(new { errno = 1, errmsg = "" });
                }
                else
                {
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                //   "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                //return View("ForgotPasswordConfirmation");
            }
            var modelErrors = ModelState.AllErrors();
            return View("ForgotPasswordConfirmation", model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        //[HttpGet]
        //[AllowAnonymous]
        //[Route("forgotPasswordConfirmation")]
        //public IActionResult ForgotPasswordConfirmation()
        //{
        //    return View();
        //}

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        [Route("ResetPassword")]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            if (model.SelectedProvider == "Authenticator")
            {
                return RedirectToAction(nameof(VerifyAuthenticatorCode), new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            var message = "Your security code is: " + code;
            if (model.SelectedProvider == "Email")
            {
                //await _mailService.SendMail(await _userManager.GetEmailAsync(user), "Security Code", message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                //await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            }

            return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        //
        // GET: /Account/VerifyAuthenticatorCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            return View(new VerifyAuthenticatorCodeViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyAuthenticatorCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // The following code protects for brute force attacks against the two factor codes.
        //    // If a user enters incorrect codes for a specified amount of time then the user account
        //    // will be locked out for a specified amount of time.
        //    var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, model.RememberBrowser);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToLocal(model.ReturnUrl);
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        _logger.LogWarning(7, "User account locked out.");
        //        return View("Lockout");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid code.");
        //        return View(model);
        //    }
        //}

        //
        // GET: /Account/UseRecoveryCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> UseRecoveryCode(string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            return View(new UseRecoveryCodeViewModel { ReturnUrl = returnUrl });
        }

        //[HttpGet("{md5:string}/avatar")]


        //
        // POST: /Account/UseRecoveryCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToLocal(model.ReturnUrl);
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid code.");
        //        return View(model);
        //    }
        //}


        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(RootController.Index), "Root");
            }
        }

        #endregion




        /// <summary>
        //  头像
        /// </summary>
        /// <param name="md5"></param>
        /// <returns></returns>
        [Route("Avatar/{md5?}")]
        public async Task<IActionResult> Avatar(string md5)
        {
            string fileName = Path.Combine(_env.WebRootPath, "static/img/common/noavatar.jpg");

            if (!string.IsNullOrEmpty(md5))
            {
                var avatar = Path.Combine(_env.WebRootPath, "static/img/avatar/" + md5);
                if (System.IO.File.Exists(avatar))
                {
                    fileName = avatar;
                }
            }
            using (var fs = System.IO.File.OpenRead(fileName))
            {
                var buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, buffer.Length);
                return File(buffer, "image/jpeg");
            }
        }
    }
}
