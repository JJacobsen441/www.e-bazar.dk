using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.Models.Identity;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        //private DAL dal = new DAL();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string user_type)
        {
            ViewBag.returnUrl = returnUrl;
            //ViewBag.user_type = user_type;
            return View("Login", new LoginViewModel());
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
             if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            if(!ThisSession.Cookie)
            {
                Dictionary<string, SYSTEM_MESSAGE> err = Setup.CheckCookieLogin(false);
                Dictionary<string, string> errors = new Dictionary<string, string>();
                if (err.ToList()[0].Value != SYSTEM_MESSAGE.NO_MESSAGE)
                    errors.Add(err.ToList()[0].Key, Texts.GetSystemMessageValue(err.ToList()[0].Key.ToString()));//price
                if (ThisSession.Json_Messages != null)
                    ThisSession.Json_Messages = errors;

                return RedirectToRoute("Marketplace");
            }
            
            var result = SignInManager.PasswordSignIn(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    //ApplicationUser user = UserManager.Find(model.Email, model.Password);
                    //ApplicationUser user = _userManager.FindByEmailAsync(model.Email).Result;
                    ApplicationUser user = UserManager.FindByEmail(model.Email);
                    if (UserManager.GetRoles(user.Id).Contains("Administrator"))
                        return RedirectToRoute("UserProfile");

                    if (!UserManager.IsEmailConfirmed(user.Id))
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        ModelState.AddModelError("", "Bekræft venligst email.");
                        return View(model);
                    }
                    string type = "Salesman";
                    if (UserManager.GetRoles(user.Id).Contains("Customer"))
                        type = "Customer";
                    CurrentUser.GetInstance().Login(user.Id, model.Email, true, type);
                    poco_person current_user = CurrentUser.GetInstance().GetCurrentUser(false, false, false);
                    if (current_user == null)
                        return HttpNotFound();
                    if (current_user.iscreated == false)
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        return RedirectToRoute("Home1");
                    }

                    //if (model.user_type == "Salesman") //ved at lave if() begge gange bliver der kun oprettet ved første login
                    //    UserManager.AddToRole(user.Id, "Salesman");
                    //else if(model.user_type == "Customer")
                    //    UserManager.AddToRole(user.Id, "Customer");
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    //CurrentUser.GetInstance().Login("", "", false);
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    //CurrentUser.GetInstance().Login("", "", false);
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    //CurrentUser.GetInstance().Login("", "", false);
                    ModelState.AddModelError("", "Invalid login forsøg.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
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
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.user_type))
                    return HttpNotFound();
                if (model.user_type != "Salesman" && model.user_type != "Customer")
                    return HttpNotFound();

                //if (string.IsNullOrEmpty(model.user_type))
                //    return RedirectToRoute("Register1");

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                    result = await UserManager.AddToRoleAsync(user.Id, model.user_type);
                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.RouteUrl("ConfirmEmail1", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //await UserManager.SendEmailAsync(user.Id, "Bekræft konto", "Bekræft din konto ved at klikke <a href=\"" + callbackUrl + "\"> her </a>");
                    if (model.user_type == "Salesman")
                    {
                        poco_salesman person_poco = new poco_salesman();
                        person_poco.person_id = user.Id;
                        person_poco.email = user.Email;
                        person_poco.booth_pocos = new List<poco_booth>();

                        DAL.GetInstance(/*true*/).SavePerson<poco_salesman>(person_poco);
                    }
                    else if (model.user_type == "Customer")
                    {
                        poco_customer person_poco = new poco_customer();
                        person_poco.person_id = user.Id;
                        person_poco.email = user.Email;

                        DAL.GetInstance(/*true*/).SavePerson<poco_customer>(person_poco);
                    }
                    //if (!string.IsNullOrEmpty(model.user_type) && model.user_type != "Administrator")
                    //{
                    //    if (model.user_type == "Salesman")
                    //    {
                    //        poco_salesman person_poco = (poco_salesman)dal.GetPersonPOCO<poco_salesman>(user.Id, true, true, true);
                    //        if (person_poco == null)
                    //        {
                    //            person_poco = new poco_salesman();
                    //            person_poco.person_id = user.Id;
                    //            person_poco.email = user.Email;
                    //            person_poco.booth_pocos = new List<poco_booth>();
                    //        }
                    //        dal.SavePerson<poco_salesman>(person_poco);
                    //    }
                    //    else if (model.user_type == "Customer")
                    //    {
                    //        poco_customer person_poco = (poco_customer)dal.GetPersonPOCO<poco_customer>(user.Id, true, true, true);
                    //        if (person_poco == null)
                    //        {
                    //            person_poco = new poco_customer();
                    //            person_poco.person_id = user.Id;
                    //            person_poco.email = user.Email;
                    //        }
                    //        dal.SavePerson<poco_customer>(person_poco);
                    //    }
                    //}

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    string subject = "Oprettelse af ny konto";
                    string body = "Kære " + model.Email + ", <br /><br />" +
                               $"For at validere din mail, tryk her: <a href='{callbackUrl}'>link</a>.<br /><br />" +
                               "Med venlig hilsen<br />" +
                               "e-bazar.dk";
                    Admin.Notification.Run("mail@e-bazar.dk", model.Email, "mail@e-bazar.dk", subject, body);

                    Dictionary<string, SYSTEM_MESSAGE> err = Setup.SetupRegisterFromClient(true);
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != SYSTEM_MESSAGE.NO_MESSAGE)
                        errors.Add(err.ToList()[0].Key, Texts.GetSystemMessageValue(err.ToList()[0].Key.ToString()));//price
                    if (HttpContext != null)
                        HttpContext.Session["Json_Messages"] = errors;

                    //return RedirectToAction("UserProfile", "Administration", new { coming_from = "Register"});
                    return RedirectToRoute("Home1");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            
            if (result.Succeeded)
            {
                string subject = "Velkommen til e-bazar.dk";
                string body = "Kære " + UserManager.GetEmail(userId) + ", <br /><br />" +
                            "Velkommen til e-bazar.dk.<br /><br />" +
                            "Dette er en opstart.<br />" +
                            "* Siden er GRATIS<br />" +
                            "* Siden er uden annoncering.<br />" +
                            "* Siden drives af donationer fra sælgerne.<br />" +
                            "Opstår problemer kontakt venligst: " + Settings.Basic.EMAIL_ADMIN() + "<br /><br />" +
                            "Med venlig hilsen<br />" +
                            Settings.Basic.SITENAME_SHORT();
                Admin.Notification.Run(Settings.Basic.EMAIL_ADMIN(), UserManager.GetEmail(userId), Settings.Basic.EMAIL_ADMIN(), subject, body);

                subject = "Admin: ny bruger";
                body = "Der er oprette ny bruger på e-bazar.dk <br /><br />" +
                            "Med venlig hilsen <br />" +
                            "e-bazar.dk(Admin)";
                Admin.Notification.Run(Settings.Basic.EMAIL_ADMIN(), Settings.Basic.EMAIL_ADMIN(), Settings.Basic.EMAIL_ADMIN(), subject, body);
            }
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null /*|| !(await UserManager.IsEmailConfirmedAsync(user.Id))*/)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                
                //await UserManager.SendEmailAsync(user.Id, "Ændring af kodeord", "For at ændre kodeord, tryk her: <a href=\"" + callbackUrl + "\">here</a>");
                
                string subject = "Ændring af kodeord";
                string body = "Kære bruger, <br /><br />" +
                           "For at ændre kodeord, tryk her: <a href=\"" + callbackUrl + "\">link</a>.<br /><br />" +
                           "Med venlig hilsen<br />" +
                           "e-bazar.dk";
                Admin.Notification.Run("no-reply@e-bazar.dk", model.Email, "no-reply@e-bazar.dk", subject, body);

                return RedirectToRoute("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToRoute("ResetPasswordConfirmation");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToRoute("Home1");

            //HomeController con = DependencyResolver.Current.GetService<HomeController>();
            //con.ControllerContext = new ControllerContext(Request.RequestContext, con);
            //return con.Index();
            ////ActionResult action = new HomeController().Index();
            ////return action;
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToRoute("Home1");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}