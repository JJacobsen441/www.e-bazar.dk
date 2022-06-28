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
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.Models.Identity;
using www.e_bazar.dk.SharedClasses;
using static www.e_bazar.dk.Models.ViewModels.ViewModels;

namespace www.e_bazar.dk.Controllers
{
    public class MarketplaceController : Controller
    {
        List<biz_params> _params = null;
        string _c_orig, _c_search, _c_url, _s;
        int _z, _t, _f;
        bool _g = false;

        private Access access;
        private ErrorHandler err = new ErrorHandler();
        private Paginator pag;
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
            dto_person current_user = user.GetCurrentUser(false, true, true);
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
                if (CheckHelper.Generel.IsAdmin(ip))
                    return View("AdminGet");// StatusCodes.Status200OK;
            }

            return RedirectToRoute("Marketplace");

        }

        [HttpPost]
        public ActionResult AdminPost(AdminPostViewModel model)
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
                dto_person current_user = user.GetCurrentUser(false, true, true);
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
                    if (CheckHelper.Generel.IsAdmin(ip))
                    {
                        if (model.pwd == "asDf1234")
                        {
                            using (EbazarDB _db = new EbazarDB())
                            {

                                int status;
                                switch (model.cmd)
                                {
                                    case "categorys":
                                        Admin.Commands.Categorys(bool.Parse(model.bool1), bool.Parse(model.bool2), _db);
                                        status = (int)HttpStatusCode.OK;
                                        break;
                                    case "deleteuser":
                                        status = (int)HttpStatusCode.NotFound;
                                        if (Admin.Commands.DeleteUser("test@e-bazar.dk", UserManager, _db))
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
            if (CheckHelper.Generel.IsAdmin(stats.ip))
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

        private col_marketplace Default() 
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
            List<dto_booth> booth_list = new List<dto_booth>();
            List<dto_booth> booth_newest = DAL.GetInstance().GetNewestBoothDTOs(0, 5);
            
            SetupCurrentUser();
            CurrentUser user = CurrentUser.GetInstance();
            biz_person current_user = null;
            ViewBag.CurrentUser = current_user;

            biz_category cat_poco = new biz_category();
            List<dto_category> cats = cat_poco._GetAll(true);

            Dictionary<string, Dictionary<string, List<dto_params>>> param_a = Categorys.s_Params();
            ViewBag.Subs = param_a;
            ViewBag.Param = ""; 

            pag = new Paginator(count, num_per_page);
            pag.GotoPage(page);

            return new col_marketplace(booth_list, count, num_per_page, booth_newest, Areas.selected, cats, c, zip, fra, til, kun_med_fast, stats_res, s != "");

        }

        [AllowAnonymous]
        public ActionResult Marketplace(MarketplaceViewModel model)
        {
            //return View("Marketplace", Default());
            try
            {
                if (!access.Queue())
                    throw new Exception();

                if (Statics.Maintenance)
                    return View("Maintenance");
                
                SetupCurrentUser();

                CheckHelper.Generel.CheckMarketPlace(model, out _s, out _c_orig, out _z, out _t, out _f, out _g);

                if (!Security.First(model.area_selected, model.a, model.c, model.p, out _c_url, out _c_search, out _params))
                    return View("Marketplace", Default());
                
                if (!Security.Second())
                    return View("Marketplace", Default());
                                
                if (ThisSession.Cookie)
                {
                    ThisSession.Catalog = "";

                    ThisSession.Search = _s;
                    ThisSession.Category = _c_search;
                    ThisSession.Params = _params;
                    ThisSession.Zip = _z;
                    ThisSession.Fra = _f;
                    ThisSession.Til = _t;
                    ThisSession.FastPris = _g;
                }

                if (ThisSession.Json_Messages != null)
                {
                    ViewBag.JSON_SYSTEM_MESSAGE = "";
                    string s2 = JsonConvert.SerializeObject(ThisSession.Json_Messages);
                    if (!string.IsNullOrEmpty(s2) && s2 != "{}")
                        ViewBag.JSON_SYSTEM_MESSAGE = s2;
                    ThisSession.Json_Messages = null;
                }

                Statistics stats = new Statistics();
                Stats stats_res = stats.GetStatistics();
                RelevantHelper.Create(true);
                SendNotifications(stats_res, _z, _f, _t, model.gra, model.s, _c_url, _c_orig);
                
                int num_per_page = 18;
                int count;
                List<dto_booth> booth_list = DAL.GetInstance().GetBoothDTOs(num_per_page * (model.page - 1), num_per_page, out count);
                List<dto_booth> booth_newest = DAL.GetInstance().GetNewestBoothDTOs(0, 5);
                
                biz_category cat_poco = new biz_category();
                List<dto_category> cats = cat_poco._GetAll(true);
                Dictionary<string, Dictionary<string, List<dto_params>>> param_a = Categorys.s_Params();
                ViewBag.Subs = param_a;
                ViewBag.Param = model.p; 

                pag = new Paginator(count, num_per_page);
                pag.GotoPage(model.page);
                
                ViewBag.CurrentUser = CurrentUser.GetInstance().GetCurrentUser(false, true, true);
                ViewBag.AreasChecked = Security.ListToString(Areas.selected, '-');
                ViewBag.CatA = _c_search.IsNullOrEmpty() || _c_search == "alle" || _c_search.Split('-')[0].IsNullOrEmpty() ? "" : _c_search.Split('-')[0];
                ViewBag.CatB = _c_search.IsNullOrEmpty() || _c_search == "alle" || _c_search.Split('-')[1].IsNullOrEmpty() ? "" : _c_search.Split('-')[1];

                col_marketplace marketplace = new col_marketplace(booth_list, count, num_per_page, booth_newest, Areas.selected, cats, _c_search, _z, _f, _t, _g, stats_res, model.s != "");

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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
        public JsonResult GetCats(GetCatsViewModel model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                List<dto_category> cats = Categorys.CatsYesNo;

                string res = "{\"list\":[";
                int c = 0;
                foreach (dto_category entry in cats)
                {
                    string name1 = Security.Format(entry.name, "_", false);
                    string routes_check = Security.EncodeCats_MD5(true, entry.name);

                    if (c != 0)
                        res += ",";
                    string obj = "{\"top\":\"" + name1 + "\",\"sub\":\"empty\",\"value\":\"" + routes_check + "\"}";
                    res += obj;
                    foreach (dto_category entry2 in entry.children)
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
        public ActionResult Booth(BoothViewModel model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                if (Statics.Maintenance)
                    return View("Maintenance");
                
                SetupCurrentUser();

                CheckHelper.Generel.CheckBooth(model);

                dto_person current_user = CurrentUser.GetInstance().GetCurrentUser(false, true, true);

                dto_booth booth_dto = DAL.GetInstance().GetBoothDTO(model.id, model.a_sub, model.b_sub, false, true, true, false, false, false, false);
                
                ViewBag.Booth = booth_dto;
                ViewBag.Address = booth_dto.fulladdress ? booth_dto.street_address.ToLower() + ", " +
                                                            booth_dto.region_dto.zip + " " +
                                                            booth_dto.region_dto.town.ToLower() + ", " +
                                                            booth_dto.country.ToLower() :
                                                            booth_dto.region_dto.zip + " " +
                                                            booth_dto.region_dto.town.ToLower() + ", " +
                                                            booth_dto.country.ToLower(); 
                ViewBag.CurrentUser = current_user;
                ViewBag.Full = booth_dto.fulladdress ? "true" : "false";

                List<dto_booth> other_booths = DAL.GetInstance().GetBoothsByPersonDTOs(booth_dto.salesman_id);
                other_booths = other_booths.Randomize<dto_booth>().Skip(0).Take(3).ToList();

                int count;
                List<dto_booth> chance = DAL.GetInstance().GetBoothDTOs(-1, -1, out count);
                chance = chance.Randomize<dto_booth>().Skip(0).Take(5).ToList();
                
                List<dto_folder> folder_dtos = DAL.GetInstance().GetFolderTree(booth_dto.booth_id, false);
                string folders_selected = ThisSession.Catalog as string;
                col_folders folders_dto = new col_folders(folder_dtos, folders_selected != null ? folders_selected : "");
                
                biz_category cat_biz = new biz_category();
                List<dto_category> cats = cat_biz._GetAll(false);
                
                boothrating br = null;
                if(current_user != null)
                    br = current_user.boothrating.Where(x => x.booth_id == booth_dto.booth_id).FirstOrDefault();
                short rating = br != null ? (short)br.rating : (short)-1;
                bool is_owner = current_user != null ? booth_dto.salesman_dto.person_id == current_user.person_id : false;

                col_booth dto = new col_booth(booth_dto, folders_dto, cats, other_booths, chance, null, is_owner, rating);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
        public ActionResult SelectCatelog(SelectCatelogViewModel model)
        {
            if (string.IsNullOrEmpty(model.a_sub))
                model.a_sub = "";
            if (string.IsNullOrEmpty(model.b_sub))
                model.b_sub = "";
            if (string.IsNullOrEmpty(model.catelog))
                model.catelog = "";

            ThisSession.Catalog = model.catelog;

            return RedirectToRoute("Booth", new { id = model.id, a_sub = model.a_sub, b_sub = model.b_sub });
        }

        [AllowAnonymous]
        public ActionResult Product(ProductViewModel model)
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

                dto_product product_dto = DAL.GetInstance().GetProductDTO(model.id, true, true, true, false, true);
                
                List<dto_booth_item> other = DAL.GetInstance().GetItemDTOs((int)product_dto.booth_id);
                other = other.Randomize<dto_booth_item>().Skip(0).Take(4).ToList();

                CurrentUser user = CurrentUser.GetInstance();
                dto_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;

                col_product dto;
                if (current_user != null)
                    dto = new col_product(product_dto, other, product_dto.booth_dto.salesman_dto.person_id == current_user.person_id);
                else
                    dto = new col_product(product_dto, other);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
        public ActionResult Collection(CollectionViewModel model)
        {

            try
            {
                if (!access.Queue())
                    throw new Exception();

                if (Statics.Maintenance)
                    return View("Maintenance");

                SetupCurrentUser();

                dto_collection collection_dto = DAL.GetInstance().GetCollectionDTO(model.id, true, true, false, true, true);
                
                List<dto_booth_item> other = DAL.GetInstance().GetItemDTOs((int)collection_dto.booth_id);
                other = other.Randomize<dto_booth_item>().Skip(0).Take(4).ToList();

                CurrentUser user = CurrentUser.GetInstance();
                dto_person current_user = user.GetCurrentUser(false, true, true);
                ViewBag.CurrentUser = current_user;

                col_collection dto;
                if (current_user != null)
                    dto = new col_collection(collection_dto, other, collection_dto.booth_dto.salesman_dto.person_id == current_user.person_id);
                else
                    dto = new col_collection(collection_dto, other);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }
        
        private col_message MessageView(long? id, string conn_owner_id, TYPE type)
        {
            TYPE typeEnum = type;

            CurrentUser user = CurrentUser.GetInstance();
            dto_person current_user = CurrentUser.GetInstance().GetCurrentUser(false, true, true);
            if (current_user == null)
                throw new Exception("A-OK, Check.");
            dto_salesman salesman_dto = new dto_salesman();
            dto_booth shop_dto = new dto_booth();
            dto_product product_dto = new dto_product();
            dto_collection collection_dto = new dto_collection();
            if (typeEnum == TYPE.PRODUCT)
            {
                product_dto = DAL.GetInstance().GetProductDTO((long)id, true, false, false, false, false);
                collection_dto = null;
                shop_dto = null;

                salesman_dto = product_dto.booth_dto.salesman_dto;
            }
            else if (typeEnum == TYPE.COLLECTION)
            {
                product_dto = null;
                collection_dto = DAL.GetInstance().GetCollectionDTO((int)id, false, true, false, true, false);
                shop_dto = null;

                salesman_dto = collection_dto.booth_dto.salesman_dto;
            }
            else if (typeEnum == TYPE.BOOTH)
            {
                product_dto = null;
                collection_dto = null;
                shop_dto = DAL.GetInstance().GetBoothDTO((int)id, "", "", true, false, false, false, false, false, false);

                salesman_dto = shop_dto.salesman_dto;
            }

            col_message message_dto = new col_message();
            message_dto.type = typeEnum;
            message_dto.id = (long)id;
            message_dto.conversation = DAL.GetInstance().GetConversation((long)id, conn_owner_id != salesman_dto.person_id ? conn_owner_id : "", typeEnum);

            message_dto.conversation.product_dto = type == TYPE.PRODUCT ? product_dto : null;
            message_dto.conversation.collection_dto = type == TYPE.COLLECTION ? collection_dto : null;
            message_dto.conversation.booth_dto = type == TYPE.BOOTH ? shop_dto : null;
            message_dto.conversation_id = message_dto.conversation.conversation_id;

            message_dto.product_owner = salesman_dto;

            biz_conversation biz = new biz_conversation();
            dto_person other = biz.GetPersonOther(message_dto.conversation);
            message_dto.other = other == null ? salesman_dto : other;//vil altid være den første

            message_dto.product_owner_id = salesman_dto.person_id;
            message_dto.other_id = message_dto.other.person_id;

            message_dto.product_owner_email = salesman_dto.email;
            message_dto.other_email = message_dto.other.email;

            message_dto.product_owner_firstname = salesman_dto.firstname;
            message_dto.other_firstname = message_dto.other.firstname;
            return message_dto;

        }

        /* MessageGet er indgang fra front */
        [HttpGet]
        [AllowAnonymous]
        public ActionResult MessageA(MessageAViewModel model)//der skal være både GET og POST, da der GET(booth) ikke kender userid
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                dto_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    return RedirectToRoute("Login1");
                ViewBag.CurrentUser = current_user;
                
                TYPE e_type;
                bool ok = Enum.TryParse(model.type.ToUpper(), out e_type);

                if (e_type == TYPE.PRODUCT)
                {
                    if (user.OwnsProduct((long)model.id))
                        return RedirectToRoute("UserProfile");
                }
                if (e_type == TYPE.COLLECTION)
                {
                    if (user.OwnsCollection((int)model.id))
                        return RedirectToRoute("UserProfile");
                }
                if (e_type == TYPE.BOOTH)
                {
                    if (user.OwnsBooth((int)model.id))
                        return RedirectToRoute("UserProfile");
                }

                string conn_owner_id = user.CurrentUserID;//conn_owner_id vil altid være CurrentUser, da der logges ind fra booth
                col_message dto = MessageView(model.id, conn_owner_id, e_type);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
        public ActionResult MessageB(MessageBViewModel model)//der skal være både GET og POST, da der GET(booth) ikke kender userid
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                dto_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    return RedirectToRoute("Login1");
                
                ViewBag.CurrentUser = current_user;
                
                TYPE e_type;
                bool ok = Enum.TryParse(model.type.ToUpper(), out e_type);

                
                string conn_owner_id = model.owner;//conn_owner_id vil altid være CurrentUser, da der logges ind fra booth
                col_message dto = MessageView(model.id, conn_owner_id, e_type);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
        public ActionResult Message(MessageViewModel model)//Save
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                dto_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, Check.");
                
                ViewBag.CurrentUser = current_user;//ikke brugt
                ViewBag.User = user;
                
                if (!user.CurrentIsAuthenticated)
                    return RedirectToRoute("Login", new { returnUrl = "/front/markedsplads" });

                col_message col;
                bool ok;
                model.message = StringHelper.OnlyAlphanumeric(model.message, true, true, "notag", CharacterHelper.All(true), out ok);

                string conn_owner_id = "";
                if (!string.IsNullOrEmpty(model.message))
                    conn_owner_id = DAL.GetInstance().SaveMessage(model.conversation_id, model.id, user.CurrentUserID, model.message, model.type);

                col = MessageView(model.id, conn_owner_id, model.type);
                if (col == null)
                    return new HttpNotFoundResult();
                col.message = "";

                string selfmailaddress = current_user.email;
                string othermailaddress = model.other_email;
                string selfname = current_user.firstname;
                string othername = model.other_firstname;

                string subject = "Du har sendt en besked";
                string body = "Kære " + @selfname + ", <br /><br />" +
                                "Du har sendt en besked.<br />" +
                                "besked: " + model.message + "<br /><br />" +
                                "Med venlig hilsen<br />" +
                                Settings.Basic.SITENAME_SHORT_CAP();
                Admin.Notification.Run(Settings.Basic.EMAIL_NO_REPLY(), selfmailaddress, Settings.Basic.EMAIL_NO_REPLY(), subject, body);

                subject = "Besked fra " + Settings.Basic.SITENAME_SHORT() + "";
                body = "Kære " + @othername + ", <br /><br />" +
                                    "Du har modtaget en besked på " + Settings.Basic.SITENAME_SHORT() + ".<br />" +
                                    "besked: " + model.message + "<br />" +
                                    "Login på din på din profil for at svare beskeden: <a href='https://www.e-bazar.dk/konto/login?returnUrl=%2Fadministration%2Fredigerprofil'>Klik her</a><br /><br />" +
                                    "Med venlig hilsen<br />" +
                                    Settings.Basic.SITENAME_SHORT();
                Admin.Notification.Run(Settings.Basic.EMAIL_NO_REPLY(), othermailaddress, Settings.Basic.EMAIL_NO_REPLY(), subject, body);

                return View("MessageView", col);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
                dto_person current_user = user.GetCurrentUser(false, true, true);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
                dto_person current_user = user.GetCurrentUser(false, true, true);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
                dto_person current_user = user.GetCurrentUser(false, true, true);
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
                catch (Exception e)
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult AddFavorite(AddFavoriteViewModel model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                DAL.GetInstance().AddFavorite(model.product_id, model.collection_id);
                if (model.product_id != -1)
                    return RedirectToRoute("Product", new { id = model.product_id });
                else
                    return RedirectToRoute("Collection", new { id = model.collection_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDFAVORITE, e);
                TempData["err_msg"] = err_msg;
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
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult RemoveFavorite(RemoveFavoriteViewModel model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                DAL.GetInstance().RemoveFavorite(model.product_id, model.collection_id);
                if (model.product_id != -1)
                    return RedirectToRoute("Product", new { id = model.product_id });
                else
                    return RedirectToRoute("Collection", new { id = model.collection_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDFAVORITE, e);
                TempData["err_msg"] = err_msg;
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
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult AddFollowing(AddFollowingViewModel model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                DAL.GetInstance().AddFollowing(model.booth_id);
                return RedirectToRoute("Booth", new { id = model.booth_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDFAVORITE, e);
                TempData["err_msg"] = err_msg;
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
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        public ActionResult RemoveFollowing(RemoveFollowingViewModel model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                DAL.GetInstance().RemoveFollowing(model.booth_id);
                return RedirectToRoute("Booth", new { id = model.booth_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDFAVORITE, e);
                TempData["err_msg"] = err_msg;
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
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i finally!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
        }

        [HttpPost]
        public JsonResult AddRating(AddRatingViewModel model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                short r;
                int b;
                bool ok = short.TryParse(model.rating, out r);
                if (ok)
                    ok = int.TryParse(model.booth_id, out b);
                if (ok)
                {
                    ok = DAL.GetInstance().AddRating(int.Parse(model.booth_id), model.person_id, short.Parse(model.rating));
                    return Json(new { success = ok, rating = model.rating });
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
                try 
                { 
                    //DAL.GetInstance().Dispose();
                    access.UnQueue(); 
                }
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
                        "Cat Orig: " + _c_orig + "<br />" +
                        "Cat Search: " + _c_search + "<br />" +
                        "Cat Url: " + _c_url + "<br />" +
                        "search: " + _s + "<br />" + "<br />" +
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
        public ActionResult NotFound(NotFoundViewModel model)
        {
            HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
            string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);

            string subject = "NotFound!";
            string body = "IP: " + ip + "<br />" +
                       "type: " + model.type.ToString() + "<br />" + "<br />" +
                       "Cat Orig: " + _c_orig + "<br />" +
                       "Cat Search: " + _c_search + "<br />" +
                       "Cat Url: " + _c_url + "<br />" +
                       "id: " + model.id + "<br />" +
                       ((model.a != "") ? "sub_a: " + model.a + "<br />" : "") +
                       ((model.b != "") ? "sub_b: " + model.b + "<br />" : "");
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