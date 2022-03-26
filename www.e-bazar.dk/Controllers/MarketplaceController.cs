using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using PostgreSQL.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.Models.Identity;
using www.e_bazar.dk.SharedClasses;
using static www.e_bazar.dk.SharedClasses.Statics;

namespace www.e_bazar.dk.Controllers
{
    public class MarketplaceController : Controller
    {
        List<poco_params> _params = null;
        private string c_orig = "";
        private string c_search = "";
        private string c_url = "";
        private string search = "";
        private int zip = 0;
        private int til = 999999;
        private int fra = 0;
        private bool kun_med_fast = false;

        private Access access;
        private ErrorHandler err = new ErrorHandler();
        private www.e_bazar.dk.Extensions.Paginator pag;
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        
        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public MarketplaceController()
        {
            string guid = Guid.NewGuid().ToString();
            access = new Access("entry", guid);
            

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
        
        public ActionResult AdminGet()
        {
            SetupCurrentUser();

            CurrentUser user = CurrentUser.GetInstance();
            poco_person current_user = user.GetCurrentUser(false, true, true);
            if (user.CurrentUserName != "admin@e-bazar.dk")
            {
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
            }
            ViewBag.CurrentUser = current_user;
            
            IList<string> role_names = UserManager.GetRoles(User.Identity.GetUserId());
            string role_name = role_names.Contains("Administrator") ? "Administrator" : role_names.Contains("Salesman") ? "Salesman" : "Customer";


            if (user.CurrentIsAuthenticated && role_name == "Administrator")
            {
                HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
                string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);
                if (Check.Generel.IsAdmin(ip))
                    return View("AdminGet");// StatusCodes.Status200OK;
            }

            return RedirectToRoute("Marketplace");

        }

        [HttpPost]
        public ActionResult AdminPost(string pwd, string cmd, string value1, string bool1, string bool2)
        {
            try
            {
                access.Queue();
                HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
                string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);
                string ip_str = " [" + ip + "]";

                string subject = "Admin(run)" + ip_str;
                string body = "IP: " + ip;
                Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (user.CurrentUserName != "admin@e-bazar.dk")
                {
                    if (current_user == null)
                        throw new Exception("A-OK, handled.");
                }
                ViewBag.CurrentUser = current_user;

                IList<string> role_names = UserManager.GetRoles(User.Identity.GetUserId());
                string role_name = role_names.Contains("Administrator") ? "Administrator" : role_names.Contains("Salesman") ? "Salesman" : "Customer";

                if (user.CurrentIsAuthenticated && role_name == "Administrator")
                {
                    if (Check.Generel.IsAdmin(ip))
                    {
                        if (pwd == "asDf1234")
                        {
                            int status;
                            switch (cmd)
                            {
                                case "categorys":
                                    Admin.Commands.Categorys(bool.Parse(bool1), bool.Parse(bool2), DAL.GetInstance(/*true*/).GetContext());
                                    status = (int)HttpStatusCode.OK;
                                    break;
                                case "deleteuser":
                                    status = (int)HttpStatusCode.NotFound;
                                    if (Admin.Commands.DeleteUser("test@e-bazar.dk", UserManager, DAL.GetInstance(/*true*/).GetContext()))
                                        status = (int)HttpStatusCode.OK;
                                    break;
                                default:
                                    status = (int)HttpStatusCode.NotFound;
                                    break;
                            }

                            subject = "Admin(is me?)";
                            body = "IP: " + ip;
                            Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);

                            return View("AdminPost", status);
                        }
                    }
                }
                return View("AdminPost", (int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e) 
            {
                Statics.Log(err.HandleError(ERROR.MARKETPLACE, e));
                TempData["err_msg"] = err.HandleError(ERROR.MARKETPLACE, e);
                TempData["ErrorMessage"] = "";
                return ErrorPage();
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        private void SendNotifications(Stats stats, int zip, int fra, int til, string g = "false", string s = "", string c = "", string _c = "") 
        {
            string msg;
            string ip_str = stats.first ? " [" + stats.ip + "]" : " ";

            if (Statics.IsDebug)
                return;
            if (Check.Generel.IsAdmin(stats.ip))
                return;// msg = "Der har været en besøgende! - Admin" + ip_str + " [" + stats.users_per_day + "]";
            else
                msg = "Der har været en besøgende! - URL" + ip_str + " [" + stats.users_per_day + "]";

            string subject = msg;
            string body = "IP: " + stats.ip + "<br />"
                        + "Search: " + s + "<br />"
                        + "Cat (_c): " + _c + "<br />"
                        //+ "Cat (c): " + c + "<br />"
                        + "Område: " + Security.ListToString(Areas.selected, ';') + "<br />"
                        + "Zip: " + zip + "<br />"
                        + "From: " + fra + "<br />"
                        + "To: " + til + "<br />"
                        + "G: " + g;
            Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);            
        }

        private dto_marketplace Default() 
        {
            ViewBag.AreasChecked = "dk";
            Areas.selected = new List<string>() { "dk" };

            int zip = 0;
            int til = 999999;
            int fra = 0;
            bool kun_med_fast = false;
            string s = "";
            string c = "alle";
            int page = 1;

            HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
            string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);
            
            Statistics stats = new Statistics();
            Stats stats_res = new Stats();
            
            int num_per_page = 6;
            int count = 0;
            List<poco_booth> booth_list = new List<poco_booth>();
            List<poco_booth> booth_newest = DAL.GetInstance(/*true*/).GetNewestBoothPOCOs(0, 5);
            
            SetupCurrentUser();
            CurrentUser user = CurrentUser.GetInstance();
            poco_person current_user = null;
            ViewBag.CurrentUser = current_user;

            poco_category cat_poco = new poco_category();
            List<poco_category> cats = cat_poco._GetAll(true);

            Dictionary<string, Dictionary<string, List<poco_params>>> param_a = Categorys.s_Params();
            ViewBag.Subs = param_a;
            ViewBag.Param = ""; //(string)Request.Query["p"];

            pag = new Paginator(count, num_per_page);
            pag.GotoPage(page);

            return new dto_marketplace(booth_list, count, num_per_page, booth_newest, Areas.selected, cats, c, zip, fra, til, kun_med_fast, stats_res, s != "");

        }

        [AllowAnonymous]
        public ActionResult Marketplace(List<string> area_selected, string a = "", string s = "", string c = "", string p = "", string z = "0", string f = "0", string t = "999999", string gra = "true", int page = 1)
        {
            try
            {
                //return View("Marketplace", Default());
                if (!access.Queue())
                    throw new Exception();


                if (Statics.Maintenance)
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

                HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
                string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);

                c_orig = c;
                search = s;
                
                if (string.IsNullOrEmpty(c))
                    c = "alle";
                
                if (!Security.First(area_selected, a, c, p, out c_url, out c_search, out _params))
                    return View("Marketplace", Default());
                
                if (!Security.Second(ip, c))
                    return View("Marketplace", Default());

                Statistics stats = new Statistics();
                Stats stats_res = stats.GetStatistics(ip);
                
                bool ok;
                search = StringHelper.OnlyAlphanumeric(s, false, true, "notag", Characters.Limited(true), out ok);
                zip = int.TryParse(z, out zip) && zip >= 0 && zip <= 10000 ? zip : 0;
                zip = Areas.selected.Contains("dk") ? zip : 0;
                til = int.TryParse(t, out til) && til >= 0 && til <= 999999 ? til : 999999;
                fra = int.TryParse(f, out fra) && fra >= 0 && fra <= til ? fra : 0;
                kun_med_fast = gra == "true";
                
                if (ThisSession.Cookie)
                {
                    ThisSession.Search = search;
                    ThisSession.Category = c_search;
                    ThisSession.Params = _params;
                    ThisSession.Zip = zip;
                    ThisSession.Fra = fra;
                    ThisSession.Til = til;
                    ThisSession.FastPris = kun_med_fast;
                }

                RelevantHelper.Create(true);
                
                SendNotifications(stats_res, zip, fra, til, gra, s, c_url, c_orig);
                
                int num_per_page = 18;
                int count;
                List<poco_booth> booth_list = DAL.GetInstance(/*true*/).GetBoothPOCOs(num_per_page * (page - 1), num_per_page, out count);
                
                List<poco_booth> booth_newest = DAL.GetInstance(/*true*/).GetNewestBoothPOCOs(0, 5);
                
                poco_category cat_poco = new poco_category();
                List<poco_category> cats = cat_poco._GetAll(true);
                Dictionary<string, Dictionary<string, List<poco_params>>> param_a = Categorys.s_Params();
                ViewBag.Subs = param_a;
                ViewBag.Param = p; //(string)Request.Query["p"];

                pag = new Paginator(count, num_per_page);
                pag.GotoPage(page);
                
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;
                ViewBag.AreasChecked = Security.ListToString(Areas.selected, '-');
                

                dto_marketplace marketplace = new dto_marketplace(booth_list, count, num_per_page, booth_newest, Areas.selected, cats, c_search, zip, fra, til, kun_med_fast, stats_res, s != "");

                return View("Marketplace", marketplace);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Beklager, der skete en fejl!";
                TempData["err_msg"] = err.HandleError(ERROR.MARKETPLACE, e);
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetCats(string ok)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                List<poco_category> cats = Categorys.CatsYesNo;

                string res = "{\"list\":[";
                int c = 0;
                foreach (poco_category entry in cats)
                {
                    string name1 = Security.Format(entry.name, "_", false);
                    string routes_check = Security.EncodeCats_MD5(true, entry.name);

                    if (c != 0)
                        res += ",";
                    string obj = "{\"top\":\"" + name1 + "\",\"sub\":\"empty\",\"value\":\"" + routes_check + "\"}";
                    res += obj;
                    foreach (poco_category entry2 in entry.children)
                    {
                        string name2 = Security.Format(entry2.name, "_", false);
                        routes_check = Security.EncodeCats_MD5(false, entry.name, name2);

                        res += ",";
                        obj = "{\"top\":\"" + name1 + "\",\"sub\":\"" + name1 + "_" + name2 + "\",\"value\":\"" + routes_check + "\"}";
                        res += obj;
                        c++;
                    }
                }
                res += "]}";
                return Json(new { success = true, arr = res });
            }
            catch (Exception e)
            {
                Statics.Log(err.HandleError(ERROR.MARKETPLACE, e));
                return AjaxErrorReturn("Beklager der skete en fejl!");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult Booth(int id, string a_sub = "", string b_sub = "")
        {

            try
            {

                if (!access.Queue())
                    throw new Exception();

                if (Statics.Maintenance)
                    return View("Maintenance");
                
                SetupCurrentUser();

                a_sub = a_sub.Replace("_", " ");
                b_sub = b_sub.Replace("_", " ");

                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);

                poco_booth booth_poco = DAL.GetInstance(/*true*/).GetBoothPOCO(id, a_sub, b_sub, false, true, true, false, false, false, false);
                
                ViewBag.Booth = booth_poco;
                ViewBag.Address = booth_poco.fulladdress ? booth_poco.street_address.ToLower() + ", " +
                                                            booth_poco.region_poco.zip + " " + 
                                                            booth_poco.region_poco.town.ToLower() + ", " +
                                                            booth_poco.country.ToLower() :
                                                            booth_poco.region_poco.zip + " " +
                                                            booth_poco.region_poco.town.ToLower() + ", " +
                                                            booth_poco.country.ToLower(); 
                ViewBag.CurrentUser = current_user;
                ViewBag.Full = booth_poco.fulladdress ? "true" : "false";

                List<poco_booth> other_booths = DAL.GetInstance(/*true*/).GetBoothsByPersonPOCOs(booth_poco.salesman_id);
                other_booths = other_booths.Randomize<poco_booth>().Skip(0).Take(3).ToList();

                int count;
                List<poco_booth> chance = DAL.GetInstance(/*true*/).GetBoothPOCOs(-1, -1, out count);
                chance = chance.Randomize<poco_booth>().Skip(0).Take(5).ToList();
                
                List<poco_folder> folder_pocos = DAL.GetInstance(/*true*/).GetFolderTree(booth_poco.booth_id, false);
                string folders_selected = ThisSession.Catalog as string;
                dto_folders folders_dto = new dto_folders(folder_pocos, folders_selected != null ? folders_selected : "");
                
                poco_category cat_poco = new poco_category();
                List<poco_category> cats = cat_poco._GetAll(false);
                
                boothrating br = null;
                if(current_user != null)
                    br = current_user.boothrating.Where(x => x.booth_id == booth_poco.booth_id).FirstOrDefault();
                short rating = br != null ? (short)br.rating : (short)-1;
                bool is_owner = current_user != null ? booth_poco.salesman_poco.person_id == current_user.person_id : false;

                dto_booth dto = new dto_booth(booth_poco, folders_dto, cats, other_booths, chance, null, is_owner, rating/*, s, c, zip, fra, til, kun_med_fast*/);
                return View(dto);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Boden kunne ikke findes!";
                TempData["err_msg"] = err.HandleError(ERROR.BOOTH, e);
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult SelectCatelog(int id, string a_sub, string b_sub, string catelog)
        {
            if (string.IsNullOrEmpty(a_sub))
                a_sub = "";
            if (string.IsNullOrEmpty(b_sub))
                b_sub = "";
            if (string.IsNullOrEmpty(catelog))
                catelog = "";

            ThisSession.Catalog = catelog;

            return RedirectToRoute("Booth", new { id = id, a_sub = a_sub, b_sub = b_sub });
        }

        [AllowAnonymous]
        public ActionResult Product(long id)
        {

            try
            {
                string s = "";
                s = s + "";
                Console.WriteLine(s);

                if (!access.Queue())
                    throw new Exception();

                if (Statics.Maintenance)
                    return View("Maintenance");
                
                SetupCurrentUser();

                poco_product product_poco = DAL.GetInstance(/*true*/).GetProductPOCO(id, true, true, true, false, true);
                
                List<IBoothItem> other = DAL.GetInstance(/*true*/).GetItemPOCOs((int)product_poco.booth_id);
                other = other.Randomize<IBoothItem>().Skip(0).Take(4).ToList();

                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;

                dto_product dto;// = new dto_product(product_poco, other, current_user != null ? product_poco.booth_poco.salesman_poco.person_id == current_user.person_id : false);
                if (current_user != null)
                    dto = new dto_product(product_poco, other, product_poco.booth_poco.salesman_poco.person_id == current_user.person_id);
                else
                    dto = new dto_product(product_poco, other);
                return View(dto);
                
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Varen kunne ikke findes!";
                TempData["err_msg"] = err.HandleError(ERROR.PRODUCT, e);
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult Collection(int id)
        {

            try
            {
                if (!access.Queue())
                    throw new Exception();

                if (Statics.Maintenance)
                    return View("Maintenance");

                SetupCurrentUser();

                poco_collection collection_poco = DAL.GetInstance(/*true*/).GetCollectionPOCO(id, true, true, false, true, true);
                
                List<IBoothItem> other = DAL.GetInstance(/*true*/).GetItemPOCOs((int)collection_poco.booth_id);
                other = other.Randomize<IBoothItem>().Skip(0).Take(4).ToList();

                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;

                dto_collection dto;
                if (current_user != null)
                    dto = new dto_collection(collection_poco, other, collection_poco.booth_poco.salesman_poco.person_id == current_user.person_id);
                else
                    dto = new dto_collection(collection_poco, other);
                return View(dto);
                
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Varen kunne ikke findes!";
                TempData["err_msg"] = err.HandleError(ERROR.COLLECTION, e);
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }
        
        private dto_message MessageView(long? id, string conn_owner_id, TYPE type)
        {
            TYPE typeEnum = type;

            CurrentUser user = CurrentUser.GetInstance();
            poco_person current_user = CurrentUser.GetInstance().GetCurrentUser(false, true, true);
            if (current_user == null)
                throw new Exception("A-OK, Check.");
            poco_salesman salesman_dao = new poco_salesman();
            poco_booth shop_dao = new poco_booth(/*db, */);
            poco_product product_dao = new poco_product();
            poco_collection collection_dao = new poco_collection();
            if (typeEnum == TYPE.PRODUCT)
            {
                product_dao = DAL.GetInstance(/*true*/).GetProductPOCO((long)id, true, false, false, false, false);
                collection_dao = null;
                shop_dao = null;

                salesman_dao = product_dao.booth_poco.salesman_poco;
            }
            else if (typeEnum == TYPE.COLLECTION)
            {
                product_dao = null;
                collection_dao = DAL.GetInstance(/*true*/).GetCollectionPOCO((int)id, false, true, false, true, false);
                shop_dao = null;

                salesman_dao = collection_dao.booth_poco.salesman_poco;
            }
            else if (typeEnum == TYPE.BOOTH)
            {
                product_dao = null;
                collection_dao = null;
                shop_dao = DAL.GetInstance(/*true*/).GetBoothPOCO((int)id, "", "", true, false, false, false, false, false, false);

                salesman_dao = shop_dao.salesman_poco;
            }

            dto_message message_dto = new dto_message();
            message_dto.type = typeEnum;
            message_dto.id = (long)id;
            message_dto.conversation = DAL.GetInstance(/*true*/).GetConversation((long)id, conn_owner_id != salesman_dao.person_id ? conn_owner_id : "", typeEnum);

            message_dto.conversation.product_poco = type == TYPE.PRODUCT ? product_dao : null;
            message_dto.conversation.collection_poco = type == TYPE.COLLECTION ? collection_dao : null;
            message_dto.conversation.booth_poco = type == TYPE.BOOTH ? shop_dao : null;
            message_dto.conversation_id = message_dto.conversation.conversation_id;

            message_dto.product_owner = salesman_dao;

            message_dto.other = message_dto.conversation.GetPersonOther();//vil altid være den første

            message_dto.product_owner_id = salesman_dao.person_id;
            message_dto.other_id = message_dto.other.person_id;

            message_dto.product_owner_email = salesman_dao.email;
            message_dto.other_email = message_dto.other.email;

            message_dto.product_owner_firstname = salesman_dao.firstname;
            message_dto.other_firstname = message_dto.other.firstname;
            return message_dto;

        }

        /* MessageGet er indgang fra front */
        [HttpGet]
        [AllowAnonymous]
        public ActionResult MessageA(long id, string type)//der skal være både GET og POST, da der GET(booth) ikke kender userid
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    return RedirectToRoute("Login1");
                ViewBag.CurrentUser = current_user;
                
                TYPE e_type;
                bool ok = Enum.TryParse(type.ToUpper(), out e_type);

                if (e_type == TYPE.PRODUCT)
                {
                    if (user.OwnsProduct((long)id))
                        return RedirectToRoute("UserProfile");
                }
                if (e_type == TYPE.COLLECTION)
                {
                    if (user.OwnsCollection((int)id))
                        return RedirectToRoute("UserProfile");
                }
                if (e_type == TYPE.BOOTH)
                {
                    if (user.OwnsBooth((int)id) /*|| user.IsEmployee((long)id, -1, TYPE.SHOP)*/)
                        return RedirectToRoute("UserProfile");
                }

                string conn_owner_id = user.CurrentUserID;//conn_owner_id vil altid være CurrentUser, da der logges ind fra booth
                dto_message dto = MessageView(id, conn_owner_id, e_type);
                if (dto == null)
                    return new HttpNotFoundResult();

                return View("MessageView", dto);
            }
            catch (Exception e)
            {
                Statics.Log(err.HandleError(ERROR.MESSAGE, e));
                TempData["err_msg"] = err.HandleError(ERROR.MESSAGE, e);
                TempData["ErrorMessage"] = "";
                return ErrorPage();
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult MessageB(long id, string owner, string type)//der skal være både GET og POST, da der GET(booth) ikke kender userid
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    return RedirectToRoute("Login1");
                
                ViewBag.CurrentUser = current_user;
                
                TYPE e_type;
                bool ok = Enum.TryParse(type.ToUpper(), out e_type);

                
                string conn_owner_id = owner;//conn_owner_id vil altid være CurrentUser, da der logges ind fra booth
                dto_message dto = MessageView(id, conn_owner_id, e_type);
                if (dto == null)
                    return new HttpNotFoundResult();

                return View("MessageView", dto);
            }
            catch (Exception e)
            {
                Statics.Log(err.HandleError(ERROR.MESSAGE, e));
                TempData["err_msg"] = err.HandleError(ERROR.MESSAGE, e);
                TempData["ErrorMessage"] = "";
                return ErrorPage();
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        /* MessagePost er indgang fra adminstration */
        /*[HttpGet]
        public ActionResult MessageB(long id, string owner, string type)
        {
            try
            {
                access.Queue();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, Check.");
                //if (current_user.test == "TEST")
                //    return RedirectToRoute("UserProfile");

                ViewBag.CurrentUser = current_user;//ikke brugt
                ViewBag.User = user;
                //ViewBag.Session = session;

                dto_message dto;
                dto = MessageView(mess.id, mess.conn_owner_id, mess.type);
                if (dto == null)
                    return new HttpNotFoundResult();
                return View("MessageView", dto);
            }
            catch (Exception e)
            {
                Statics.Log(err.HandleError(ERROR.MESSAGE, e));
                TempData["err_msg"] = err.HandleError(ERROR.MESSAGE, e);
                TempData["ErrorMessage"] = "";
                return ErrorPage();
            }
            finally
            {
                try { db?.Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    string subject = "Fejl i finally!";
                    string body = "";
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }*/

        [HttpPost]
        public ActionResult Message(dto_message mess)//Save
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, Check.");
                
                ViewBag.CurrentUser = current_user;//ikke brugt
                ViewBag.User = user;
                
                if (!user.CurrentIsAuthenticated)
                    return RedirectToRoute("Login", new { returnUrl = "/front/markedsplads" });

                dto_message dto;
                bool ok;
                mess.message = StringHelper.OnlyAlphanumeric(mess.message, true, true, "notag", Characters.All(true), out ok);

                string conn_owner_id = "";
                if (!string.IsNullOrEmpty(mess.message))
                    conn_owner_id = DAL.GetInstance(/*true*/).SaveMessage(mess.conversation_id, mess.id, user.CurrentUserID, mess.message, mess.type);

                dto = MessageView(mess.id, conn_owner_id, mess.type);
                if (dto == null)
                    return new HttpNotFoundResult();
                dto.message = "";

                string selfmailaddress = current_user.email;
                string othermailaddress = mess.other_email;
                string selfname = current_user.firstname;
                string othername = mess.other_firstname;

                string subject = "Du har sendt en besked";
                string body = "Kære " + @selfname + ", <br /><br />" +
                                "Du har sendt en besked.<br />" +
                                "besked: " + mess.message + "<br /><br />" +
                                "Med venlig hilsen<br />" +
                                Settings.Basic.SITENAME_SHORT_CAP();
                Admin.Notification.Run(Settings.Basic.EMAIL_NO_REPLY(), selfmailaddress, Settings.Basic.EMAIL_NO_REPLY(), subject, body);

                subject = "Besked fra " + Settings.Basic.SITENAME_SHORT() + "";
                body = "Kære " + @othername + ", <br /><br />" +
                                    "Du har modtaget en besked på " + Settings.Basic.SITENAME_SHORT() + ".<br />" +
                                    "besked: " + mess.message + "<br />" +
                                    "Login på din på din profil for at svare beskeden: <a href='https://www.e-bazar.dk/konto/login?returnUrl=%2Fadministration%2Fredigerprofil'>Klik her</a><br /><br />" +
                                    "Med venlig hilsen<br />" +
                                    Settings.Basic.SITENAME_SHORT();
                Admin.Notification.Run(Settings.Basic.EMAIL_NO_REPLY(), othermailaddress, Settings.Basic.EMAIL_NO_REPLY(), subject, body);

                return View("MessageView", dto);
            }
            catch (Exception e)
            {
                Statics.Log(err.HandleError(ERROR.MESSAGE, e));
                TempData["err_msg"] = err.HandleError(ERROR.MESSAGE, e);
                TempData["ErrorMessage"] = "";
                return ErrorPage();
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult Info() 
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;//ikke brugt
                ViewBag.User = user;
            
                return View("_Info");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.INFO, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult Features()
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;//ikke brugt
                ViewBag.User = user;

                return View("_Features");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.FEATURES, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult Conditions()
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;//ikke brugt
                ViewBag.User = user;

                return View("_Conditions");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.CONDITIONS, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult AddFavorite(long product_id, int collection_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                DAL.GetInstance(/*true*/).AddFavorite(product_id, collection_id);
                if (product_id != -1)
                    return RedirectToRoute("Product", new { id = product_id });
                else
                    return RedirectToRoute("Collection", new { id = collection_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDFAVORITE, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult RemoveFavorite(long product_id, int collection_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                DAL.GetInstance(/*true*/).RemoveFavorite(product_id, collection_id);
                if (product_id != -1)
                    return RedirectToRoute("Product", new { id = product_id });
                else
                    return RedirectToRoute("Collection", new { id = collection_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDFAVORITE, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult AddFollowing(int booth_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                DAL.GetInstance(/*true*/).AddFollowing(booth_id);
                return RedirectToRoute("Booth", new { id = booth_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDFAVORITE, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult RemoveFollowing(int booth_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                DAL.GetInstance(/*true*/).RemoveFollowing(booth_id);
                return RedirectToRoute("Booth", new { id = booth_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDFAVORITE, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage1");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [HttpPost]
        public JsonResult AddRating(string booth_id, string person_id, string rating)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                short r;
                int b;
                bool ok = short.TryParse(rating, out r);
                if (ok)
                    ok = int.TryParse(booth_id, out b);
                if (ok)
                {
                    ok = DAL.GetInstance(/*true*/).AddRating(int.Parse(booth_id), person_id, short.Parse(rating));
                    return Json(new { success = ok, rating = rating });
                }
                else
                    return Json(new { success = false });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDRATING, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        /*[HttpPost]
        [AllowAnonymous]
        public JsonResult IsMobile(int width)
        {
            //SetupCurrentUser();

            try
            {
                access.Queue();

                //if(!ThisSession.IsMobile)
                ThisSession.IsMobile = width < 768 ? "true" : "false";
                string res = ThisSession.IsMobile == "true" ? "mobile" : "not mobile";
                return Json(new { success = true, mobile = res });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ISMOBILE, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    string subject = "Fejl i finally!";
                    string body = "";
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }*/

        [HttpPost]
        [AllowAnonymous]
        public JsonResult Cookie()
        {

            try
            {
                if (!access.Queue())
                    throw new Exception();
                SetupCurrentUser();

                ThisSession.Cookie = true;
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.GETTAGS, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                try { DAL.GetInstance().Dispose(); access.UnQueue(); }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult ErrorPage()
        {
            HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
            string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);
            string err_msg = TempData["err_msg"] as string;

            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;
            ViewBag.ErrorStack = TempData["err_msg"] as string;

            //if(string.IsNullOrEmpty(ViewBag.ErrorMessage) && string.IsNullOrEmpty(err_msg))
            //    return HttpNotFound(HttpStatusCode.NotFound.ToString());

            //if (err_msg.Contains("A-OK, Handled."))
            //    return HttpNotFound(HttpStatusCode.NotFound.ToString());

            if (!string.IsNullOrEmpty(err_msg))
            {
                string subject = "Der er sket en fejl!";
                string body = "ViewBag: " + ViewBag.ErrorMessage + "<br />" +
                        "IP: " + ip + "<br />" +
                        "Cat Orig: " + c_orig + "<br />" +
                        "Cat Search: " + c_search + "<br />" +
                        "Cat Url: " + c_url + "<br />" +
                        "search: " + search + "<br />" + "<br />" +
                        //"zip: " + zip + "<br />" +
                        //"til: " + til + "<br />" +
                        //"fra: " + fra + "<br />" +
                        //"kun med fast: " + kun_med_fast + "<br />" + "<br />" +
                        "MSG: " + /*Extensions.Extensions.HtmlEncode(*/err_msg/*)*/;
                Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult NotFound(TYPE type, long id = -1, string a = "", string b = "")
        {
            HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
            string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);

            string subject = "NotFound!";
            string body = "IP: " + ip + "<br />" +
                       "type: " + type.ToString() + "<br />" + "<br />" +
                       "Cat Orig: " + c_orig + "<br />" +
                       "Cat Search: " + c_search + "<br />" +
                       "Cat Url: " + c_url + "<br />" +
                       "id: " + id + "<br />" +
                       ((a != "") ? "sub_a: " + a + "<br />" : "") +
                       ((b != "") ? "sub_b: " + b + "<br />" : "");
            Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);

            return HttpNotFound(HttpStatusCode.NotFound.ToString());
        }

        [AllowAnonymous]
        private JsonResult AjaxErrorReturn(string user_msg)
        {
            string err_msg = TempData["err_msg"] as string;

            string subject = "Der er sket en fejl!";
            string body = Extensions.Extensions.HtmlEncode(err_msg);
            Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);

            return Json(new { success = false, res = user_msg });
        }
    }
}