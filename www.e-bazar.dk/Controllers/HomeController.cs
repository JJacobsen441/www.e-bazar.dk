using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using PostgreSQL.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.Models.Identity;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Controllers
{
    public class HomeController : Controller
    {
        private Access access;
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public HomeController()
        {
            string guid = Guid.NewGuid().ToString();
            access = new Access("index", guid);
            
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));

        }

        private void SetupCurrentUser()
        {
            string current_id = "";
            string current_name = "";
            bool current_auth = false;
            string current_type = "none";

            if (User != null)
            {
                current_id = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : "";
                current_name = User.Identity.IsAuthenticated ? User.Identity.GetUserName() : "";
                current_auth = User.Identity.IsAuthenticated;
                current_type =
                    User.IsInRole("Administrator") ? "Administrator" :
                    User.IsInRole("Salesman") ? "Salesman" :
                    User.IsInRole("Customer") ? "Customer" : "none";
            }
            
            CurrentUser.GetInstance().Login(current_id, current_name, current_auth, current_type);
        }

        private void SendNotifications(Stats stats)
        {
            string msg;
            string ip_str = stats.first ? " [" + stats.ip + "]" : " ";

            if (StaticsHelper.IsDebug)
                return;
            if (CheckHelper.Generel.IsAdmin(stats.ip))
                return;
            else
                msg = "Der har været en besøgende! - URL" + ip_str + " [" + stats.users_per_day + "]";

            string subject = msg;
            string body = "IP: " + stats.ip + "<br />";

            AdminHelper.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                if (StaticsHelper.Maintenance)
                    return View("Maintenance");

                if (ThisSession.Json_Messages != null)
                {
                    ViewBag.JSON_SYSTEM_MESSAGE = "";
                    string s2 = JsonConvert.SerializeObject(ThisSession.Json_Messages);
                    if (!string.IsNullOrEmpty(s2) && s2 != "{}")
                        ViewBag.JSON_SYSTEM_MESSAGE = s2;
                    ThisSession.Json_Messages = null;
                }

                SetupCurrentUser();
                ThisSession.Catalog = "";

                RelevantHelper.Create(true);

                HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
                string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);

                Stats stats_res = StatisticsHelper.GetStatistics();
                ViewBag.Stats = stats_res;
                
                SendNotifications(stats_res);

                List<dto_booth> booth_newest = DAL.GetInstance().GetNewestBoothDTOs(0, 5);
                ViewBag.Newest= booth_newest;

                CurrentUser user = CurrentUser.GetInstance();
                dto_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;
                
                return View("Index");
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Beklager, der skete en fejl!";
                TempData["err_msg"] = ErrorHelper.HandleError(ERROR.MARKETPLACE, e);
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
                catch (Exception e)
                {
                    string subject = "Fejl i finally!";
                    string body = ErrorHelper.FormatError(e);
                    AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult Login()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}