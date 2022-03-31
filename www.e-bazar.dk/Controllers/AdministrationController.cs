using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using PostgreSQL.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace www.e_bazar.dk.Controllers
{
    public class AdministrationController : Controller
    {
        private Access access;
        private ErrorHandler err = new ErrorHandler();

        /// <summary>
        /// Application DB context
        /// </summary>
        protected ApplicationDbContext ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> UserManager { get; set; }

        public AdministrationController()
        {
            string guid = Guid.NewGuid().ToString();
            access = new Access("entry", guid);
            
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
        }
        
        public AdministrationController(UserManager<ApplicationUser> userManager)
        {
            string guid = Guid.NewGuid().ToString();
            access = new Access("entry", guid);

            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = userManager;
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
        
        private T SetupUserProfile<T>(string userid, ref dto_userprofile userprofile, string role_name) where T: poco_person, new()
        {
            T person_poco = (T)DAL.GetInstance().GetPersonPOCO<T>(userid, true, true, true);
            if (person_poco == null)
                throw new Exception("A-OK, handled.");
            
            bool is_salesman = typeof(poco_salesman) == typeof(T);
            List<poco_conversation> lista = DAL.GetInstance().GetConversationsPerson(userid, is_salesman, true);
            if (lista == null)
                return null;
            
            List<poco_conversation> own = lista.Where(c => c.person_id == userid).ToList();
            own = own.Where(c=>c.comment_pocos.Count() > 0).OrderBy(c => c.comment_pocos.OrderBy(com => com.created_on).FirstOrDefault().created_on).ToList();
            
            List<poco_conversation> items = lista.Where(c => c.product_poco != null && c.person_id != userid).ToList();
            items = items.Concat(lista.Where(c => c.collection_poco != null && c.person_id != userid)).ToList();
            items = items.OrderBy(c => c.comment_pocos.OrderBy(com => com.created_on).FirstOrDefault().created_on).ToList();
            
            List<poco_conversation> booths = lista.Where(c => c.booth_poco != null && c.person_id != userid).ToList();
            booths = booths.OrderBy(c => c.comment_pocos.OrderBy(com => com.created_on).FirstOrDefault().created_on).ToList();
            dto_conversations conversations = new dto_conversations(own, booths, items, is_salesman);

            List<IBoothItem> follower_news = new List<IBoothItem>();
            foreach (poco_booth boo in person_poco.following)
                follower_news = follower_news.Concat(boo.GetRelevantItems(0, 5)).ToList();
            follower_news = follower_news.OrderByDescending(i => i.created_on).Take(5).ToList();
            
            if (person_poco.GetType() == typeof(poco_salesman))
            {
                userprofile.customer_poco = null;
                userprofile.salesmen = null;
                userprofile.customers = null;
                userprofile.booth_pocos = DAL.GetInstance().GetBoothPOCOs(userid, false);
                userprofile.follower_news = follower_news;
                userprofile.conversations_dto = conversations;
            }
            else
            {
                userprofile.salesman_poco = null;
                userprofile.salesmen = null;
                userprofile.customers = null;
                userprofile.booth_pocos = new List<poco_booth>();
                userprofile.follower_news = follower_news;
                userprofile.conversations_dto = conversations;
            }
            
            return person_poco;
        }
        
        public ActionResult UserProfile()
        {

            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (user.CurrentUserName != "admin@e-bazar.dk")
                {
                    if(current_user == null)
                        throw new Exception("A-OK, handled.");
                }

                ViewBag.CurrentUser = current_user;

                if (ThisSession.Json_Errors != null)
                {
                    ViewBag.JSON_ERRORS = "";
                    string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                    if (!string.IsNullOrEmpty(s) && s != "{}")
                        ViewBag.JSON_ERRORS = s;
                    ThisSession.Json_Errors = null;
                    ViewBag.JSON_SYSTEM_MESSAGE = "";
                    string s2 = JsonConvert.SerializeObject(ThisSession.Json_Messages);
                    if (!string.IsNullOrEmpty(s2) && s2 != "{}")
                        ViewBag.JSON_SYSTEM_MESSAGE = s2;
                    ThisSession.Json_Messages = null;
                    
                }
                                
                string id = user.CurrentUserID;
                List<string> role_names = UserManager.GetRoles(id).ToList();
                string role_name = role_names.Contains("Administrator") ? "Administrator" : role_names.Contains("Salesman") ? "Salesman" : "Customer";

                dto_userprofile profile = new dto_userprofile();
                poco_salesman salesman_poco;
                poco_customer customer_poco;
                List<poco_booth> booth_pocos = new List<poco_booth>();
                if (role_name == "Administrator")
                {
                    return RedirectToRoute("AdminGet");
                }
                else if (role_name == "Salesman")
                {
                    salesman_poco = SetupUserProfile<poco_salesman>(id, ref profile, role_name);
                    profile.salesman_poco = salesman_poco;
                    return View("SalesmanProfile", profile);
                }
                else
                {
                    customer_poco = SetupUserProfile<poco_customer>(id, ref profile, role_name);
                    profile.customer_poco = customer_poco;
                    return View("CustomerProfile", profile);
                }
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.USERPROFILE, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                try 
                { //DAL.GetInstance().Dispose();
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
        [ValidateAntiForgeryToken]
        public ActionResult SalesmanProfile(dto_userprofile model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                Dictionary<string, ERROR_MESSAGE> err = Setup.SetupSalesmanProfileFromClient(ref model);
                Dictionary<string, string> dirs = Setup.SetupProfileDirs(model.salesman_poco);

                if (CheckHelper.ErrorSalesmanProfile.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, Texts.GetErrorMessageValue(err.ToList()[0].Key));
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageValue(err.ToList()[1].Key));
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, Texts.GetErrorMessageValue(err.ToList()[2].Key));
                    if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageValue(err.ToList()[3].Key));

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;                    
                }
                else
                {
                    List<string> uploaded_profileimage = Paths.GetFileNames(PATH.PROFILE_DIRECTORY_TMP, dirs, false);
                    if (uploaded_profileimage.Count > 0)
                        model.salesman_poco.profileimage = uploaded_profileimage.Where(f => f.Substring(0, 2) != "t_").ToList().FirstOrDefault();
                    DAL.GetInstance(/*true*/).UpdatePerson(model.salesman_poco);

                    string tmp_path = Paths.GetPath(PATH.PROFILE_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = Paths.GetFileNames(PATH.PROFILE_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true), file, true, true, false, true);
                            file = uploaded_files[1];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                }
                return RedirectToRoute("UserProfile");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.SALESMANPROFILE, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                try 
                { //DAL.GetInstance().Dispose();
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
        [ValidateAntiForgeryToken]
        public ActionResult CustomerProfile(dto_userprofile model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                Dictionary<string, ERROR_MESSAGE> err = Setup.SetupCustomerProfileFromClient(ref model);
                Dictionary<string, string> dirs = Setup.SetupProfileDirs(model.customer_poco);

                if (CheckHelper.ErrorCustomerProfile.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if(err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, Texts.GetErrorMessageValue(err.ToList()[0].Key));
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageValue(err.ToList()[1].Key));
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, Texts.GetErrorMessageValue(err.ToList()[2].Key));

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                }
                else
                {
                    List<string> uploaded_profileimage = Paths.GetFileNames(PATH.PROFILE_DIRECTORY_TMP, dirs, false);
                    if (uploaded_profileimage.Count > 0)
                        model.customer_poco.profileimage = uploaded_profileimage.Where(f => f.Substring(0, 2) != "t_").ToList().FirstOrDefault();
                    DAL.GetInstance(/*true*/).UpdatePerson(model.customer_poco);

                    string tmp_path = Paths.GetPath(PATH.PROFILE_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = Paths.GetFileNames(PATH.PROFILE_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true), file, true, true, false, true);
                            file = uploaded_files[1];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                }
                return RedirectToRoute("UserProfile");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.CUSTOMERPROFILE, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult CreateBooth()
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                ViewBag.CurrentUser = current_user;

                poco_booth model = TempData["datacontainer"] as poco_booth;

                if (model == null)//ny
                {
                    poco_salesman salesman_poco = new poco_salesman();
                    poco_booth poco = new poco_booth();
                    booth booth = new booth();
                    booth.person = salesman_poco.GetPerson(CurrentUser.GetInstance().CurrentUserID, "Salesman", false, false, false);
                    if (booth.person == null)
                        throw new Exception("A-OK, handled."); 
                    booth.person_id = user.CurrentUserID;
                    poco.ToPoco(booth, null);

                    if (ThisSession.Json_Errors != null)
                    {
                        ViewBag.JSON_ERRORS = "";
                        string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                        if (!string.IsNullOrEmpty(s) && s != "{}")
                            ViewBag.JSON_ERRORS = s;
                        ThisSession.Json_Errors = null;
                    }
                    return View("CreateBooth", poco);
                }
                else//der er errors
                {
                    poco_salesman salesman_poco = new poco_salesman();
                    model.salesman_poco = salesman_poco.GetPersonPOCO<poco_salesman>(CurrentUser.GetInstance().CurrentUserID, "Salesman", true, false, false);
                    if (model.salesman_poco == null)
                        throw new Exception("A-OK, handled.");
                    model.salesman_id = user.CurrentUserID;

                    if (ThisSession.Json_Errors != null)
                    {
                        ViewBag.JSON_ERRORS = "";
                        string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                        if (!string.IsNullOrEmpty(s) && s != "{}")
                            ViewBag.JSON_ERRORS = s;
                        ThisSession.Json_Errors = null;
                    }

                    return View("CreateBooth", model);
                }
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.EDITBOOTH, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        public ActionResult CreateBooth(poco_booth model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                //ViewBag.CurrentUser = current_user;

                poco_person salesman_poco = current_user;
                Dictionary<string, ERROR_MESSAGE> err = Setup.SetupBoothFromClient(ref model, CurrentUser.GetInstance().CurrentUserID, DAL.GetInstance(/*true*/));
                //if (model == null)
                //    return _NotFound();
                Dictionary<string, string> dirs = Setup.SetupBoothDirs(ref model, salesman_poco.sysname);

                if (CheckHelper.ErrorBooth.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if(err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, Texts.GetErrorMessageValue(err.ToList()[0].Key));
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageValue(err.ToList()[1].Key));
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, Texts.GetErrorMessageValue(err.ToList()[2].Key));
                    //errors.Add(err.ToList()[2].Key, Check.ErrorMessageBooth.GetErrorMessage(err.ToList()[2].Value));
                    //errors.Add(err.ToList()[3].Key, Check.ErrorMessageBooth.GetErrorMessage(err.ToList()[3].Value));

                    if(ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("CreateBooth");
                }
                else
                {
                    List<string> uploaded_fnames = Paths.GetFileNames(PATH.BOOTH_DIRECTORY_TMP, dirs, false);
                    model.frontimage = uploaded_fnames.Count > 0 ? uploaded_fnames.Where(f => f.Substring(0, 2) != "t_").ToList().FirstOrDefault() : model.frontimage;
                    model.sysname = dirs["booth_sysname"];
                    int booth_id = DAL.GetInstance(/*true*/).SaveBooth(model);
                    model.booth_id = booth_id;


                    string tmp_path = Paths.GetPath(PATH.BOOTH_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = Paths.GetFileNames(PATH.BOOTH_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true), file, true, true, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                    return RedirectToRoute("EditBooth1", new { booth_id = booth_id });
                }                
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.CREATEBOOTH, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult EditBooth(int booth_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();


                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                ViewBag.CurrentUser = current_user;

                if (booth_id == -1)
                    throw new Exception("A-OK, Check");

                if (!user.OwnsBooth(booth_id))
                    throw new Exception("A-OK, handled.");

                poco_booth model = TempData["datacontainer"] as poco_booth;
                ViewBag.Tab = ThisSession.Tab as string == "categorys" ? "categorys" : "home";
                TempData["datacontainer"] = null;

                Dictionary<string, string> errors = ThisSession.Json_Errors as Dictionary<string, string>;
                if (ThisSession.Json_Errors != null)
                {
                    ViewBag.JSON_ERRORS = "";
                    string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                    if (!string.IsNullOrEmpty(s) && s != "{}")
                        ViewBag.JSON_ERRORS = s;
                    ThisSession.Json_Errors = null;
                }
                if(ThisSession.Json_Messages != null)
                {
                    ViewBag.JSON_SYSTEM_MESSAGE = "";
                    string s2 = JsonConvert.SerializeObject(ThisSession.Json_Messages);
                    if (!string.IsNullOrEmpty(s2) && s2 != "{}")
                        ViewBag.JSON_SYSTEM_MESSAGE = s2;
                    ThisSession.Json_Messages = null;
                }

                poco_booth booth_poco = new poco_booth();
                booth_poco = DAL.GetInstance().GetBoothPOCO(booth_id, "", "", true, true, true, true, false, false, true);
                
                List<poco_folder> folder_pocos = DAL.GetInstance().GetFolderTree(booth_id, false);
                dto_folders folders_dto = new dto_folders(folder_pocos, "");
                poco_category cat_poco = new poco_category();
                List<poco_category> cats = cat_poco._GetAll(true);
                List<poco_booth> booth_pocos = DAL.GetInstance().GetBoothPOCOs(CurrentUser.GetInstance().CurrentUserID, false);
                
                return View("EditBooth", new dto_booth(booth_poco, folders_dto, cats, null, null, booth_pocos, false, -1/*dont care*/));

            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.EDITBOOTH, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        public ActionResult EditBooth(dto_booth model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                
                poco_booth booth_poco = model.booth_poco;
                poco_person salesman_poco = current_user;
                Dictionary<string, ERROR_MESSAGE> err = Setup.SetupBoothFromClient(ref booth_poco, CurrentUser.GetInstance().CurrentUserID, DAL.GetInstance());
                
                Dictionary<string, string> dirs = Setup.SetupBoothDirs(ref booth_poco, salesman_poco.sysname);

                if (CheckHelper.ErrorBooth.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, Texts.GetErrorMessageValue(err.ToList()[0].Key));//booth name
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageValue(err.ToList()[1].Key));//description
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, Texts.GetErrorMessageValue(err.ToList()[2].Key));//part address
                    if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageValue(err.ToList()[3].Key));//full address
                    //if (err.ToList()[4].Value != ERROR_MESSAGE.OK)
                        //errors.Add(err.ToList()[4].Key, Texts.GetErrorMessageValue(err.ToList()[4].Key));//tags

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    //TempData["datacontainer"] = model.booth_poco;
                    return RedirectToRoute("EditBooth1", new { booth_id = model.booth_poco.booth_id });
                }
                else
                {
                    List<string> uploaded_fnames = Paths.GetFileNames(PATH.BOOTH_DIRECTORY_TMP, dirs, false);
                    string current_path = Paths.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true);
                    List<string> current_fnames = Paths.GetFileNames(PATH.BOOTH_DIRECTORY_NAME, dirs, false);
                    booth_poco.frontimage = uploaded_fnames.Count > 0 ? uploaded_fnames.Where(f => f.Substring(0, 2) != "t_").ToList().FirstOrDefault() : current_fnames.Count > 0 ? booth_poco.frontimage : "";
                    DAL.GetInstance(/*true*/).UpdateBooth(booth_poco);

                    string tmp_path = Paths.GetPath(PATH.BOOTH_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = Paths.GetFileNames(PATH.BOOTH_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true), file, true, true, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                    return RedirectToRoute("EditBooth1", new { booth_id = booth_poco.booth_id });
                }
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.EDITBOOTH, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult DeleteBooth(int booth_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                
                if (!user.OwnsBooth(booth_id))
                    throw new Exception("A-OK, handled.");

                Dictionary<string, string> dirs;
                string path;
                poco_booth booth_poco = DAL.GetInstance(/*true*/).GetBoothPOCO(booth_id, "", "", true, true, true, false, false, true, true);//////////////husk conversations og categories
                
                foreach (poco_product product_poco in booth_poco.product_pocos)
                {
                    DAL.GetInstance().DeleteProduct(product_poco.id);
                }

                foreach (poco_collection collection_poco in booth_poco.collection_pocos)
                {
                    DAL.GetInstance().DeleteCollection((int)collection_poco.id);
                }
                dirs = new Dictionary<string, string>();
                dirs["identity_id"] = current_user.sysname;
                dirs["booth_sysname"] = booth_poco.sysname;
                path = Paths.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true);
                Paths.ClearFolder(path, true, true);

                DAL.GetInstance().DeleteBooth(booth_id);

                return RedirectToRoute("UserProfile");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.DELETEBOOTH, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        
        public ActionResult CreateProduct(int booth_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                ViewBag.CurrentUser = current_user;

                if (!user.OwnsBooth(booth_id))
                    throw new Exception("A-OK, handled.");

                //EbazarDB _db = DAL.GetInstance().GetContext();
                using (EbazarDB _db = new EbazarDB())
                {

                    poco_product model = TempData["datacontainer"] as poco_product;
                    if (model == null)//ny
                    {
                        poco_product product_poco = new poco_product(false);
                        poco_booth booth_poco = new poco_booth();
                        booth b = booth_poco.GetBooth(booth_id, "", "", true, true, true, true, false, false, true);
                    
                        booth_poco.ToPoco(b, null);

                        product_poco.booth_poco = booth_poco;
                        product_poco.category_main_id = _db.category.Where(x => x.name != ".ingen" && x.is_parent && x.priority == 1).FirstOrDefault().Id;

                        product_poco.ToPoco(new product(), new List<poco_booth.Hit>(), "");
                        product_poco.SetupToClient<poco_product>();
                    
                        ViewBag.ProductCategoryMain = product_poco.category_main_selectlist;
                        ViewBag.ProductCategorySecond = product_poco.category_second_selectlist;
                        ViewBag.StatusStock = product_poco.status_stock_selectlist;
                        ViewBag.StatusCondition = product_poco.status_condition_selectlist;

                        if (ThisSession.Json_Errors != null)
                        {
                            ViewBag.JSON_ERRORS = "";
                            string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                            if (!string.IsNullOrEmpty(s) && s != "{}")
                                ViewBag.JSON_ERRORS = s;
                            ThisSession.Json_Errors = null;
                        }

                        return View("CreateProduct", product_poco);
                    }
                    else//der er errors
                    {
                        model.booth_poco = DAL.GetInstance(/*true*/).GetBoothPOCO(booth_id, "", "", true, false, false, false, false, false, true);
                        //if (model.booth_poco == null)
                        //    return _NotFound();
                        model.SetupToClient<poco_product>();
                        ViewBag.StatusCondition = model.status_condition_selectlist;
                        ViewBag.StatusStock = model.status_stock_selectlist;
                        ViewBag.ProductCategoryMain = model.category_main_selectlist;

                        if (ThisSession.Json_Errors != null)
                        {
                            ViewBag.JSON_ERRORS = "";
                            string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                            if (!string.IsNullOrEmpty(s) && s != "{}")
                                ViewBag.JSON_ERRORS = s;
                            ThisSession.Json_Errors = null;
                        }
                        return View("CreateProduct", model);
                    }
                }
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.CREATEPRODUCT, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        public ActionResult CreateProduct(poco_product model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                
                poco_person salesman_poco = current_user;
                string status_condition_select = Request != null ? Request.Form["status_condition_select"].ToString() : "TEST_PRODUCT";
                string status_stock_select = "PÅ_LAGER";
                
                Dictionary<string, ERROR_MESSAGE> err = Setup.SetupProductFromClient(ref model, model.booth_id, status_condition_select, status_stock_select, DAL.GetInstance());
                
                Dictionary<string, string> dirs = Setup.SetupProductDirs(model, salesman_poco.sysname);

                if (CheckHelper.ErrorProduct.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, Texts.GetErrorMessageValue(err.ToList()[0].Key));//price
                    //if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageProduct(err.ToList()[1].Value));//category
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageValue(err.ToList()[1].Key));//name
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageProduct(err.ToList()[3].Value));//note
                    //if (err.ToList()[4].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[4].Key, Texts.GetErrorMessageProduct(err.ToList()[4].Value));//description
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, Texts.GetErrorMessageValue(err.ToList()[2].Key));//no_of_units

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("CreateProduct", new { booth_id = model.booth_id });
                }
                else
                {
                    model.sysname = dirs["product_sysname"];
                    List<string> uploaded_fnames = Paths.GetFileNames(PATH.PRODUCT_DIRECTORY_TMP, dirs, false);
                    long product_id = DAL.GetInstance().SaveProduct(model, uploaded_fnames.Where(f => f.Substring(0, 2) != "t_").ToList());
                    model.id = product_id;//for test

                    string tmp_path = Paths.GetPath(PATH.PRODUCT_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = Paths.GetFileNames(PATH.PRODUCT_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true), file, true, false, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }                        
                    }
                    
                    return RedirectToRoute("EditProduct1", new { product_id = product_id });
                }
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.CREATEPRODUCT, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult EditProduct(long product_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                ViewBag.CurrentUser = current_user;

                if (!user.OwnsProduct(product_id))
                    throw new Exception("A-OK, handled.");

                poco_product product_poco = DAL.GetInstance(/*true*/).GetProductPOCO(product_id, true, false, true, true, true);
                
                List<poco_params> list = DAL.GetInstance(/*true*/).GetParams((int)product_poco.category_main_id, (int)product_poco.category_second_id);
                ViewBag.Params = list;
                ViewBag.ProductCategoryMain = product_poco.category_main_selectlist;
                ViewBag.ProductCategorySecond = product_poco.category_second_selectlist;
                ViewBag.LevelA = product_poco.foldera_selectlist;
                ViewBag.LevelB = product_poco.folderb_selectlist;
                ViewBag.StatusStock = product_poco.status_stock_selectlist;
                ViewBag.StatusCondition = product_poco.status_condition_selectlist;
                
                if (ThisSession.Json_Errors != null)
                {
                    ViewBag.JSON_ERRORS = "";
                    string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                    if (!string.IsNullOrEmpty(s) && s != "{}")
                        ViewBag.JSON_ERRORS = s;
                    ThisSession.Json_Errors = null;
                }
                
                return View("EditProduct", product_poco);
                
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.EDITPRODUCT, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        public ActionResult EditProduct(poco_product model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                
                poco_person salesman_poco = current_user;
                string status_condition_select = Request != null ? Request.Form["status_condition_select"].ToString() : "TEST_PRODUCT";
                string status_stock_select = "PÅ_LAGER";
                
                Dictionary<string, ERROR_MESSAGE> err = Setup.SetupProductFromClient(ref model, model.booth_id, status_condition_select, status_stock_select, DAL.GetInstance(/*true*/));
                
                Dictionary<string, string> dirs = Setup.SetupProductDirs(model, salesman_poco.sysname);
                               
                if (CheckHelper.ErrorProduct.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, Texts.GetErrorMessageValue(err.ToList()[0].Key));//price
                    //if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageProduct(err.ToList()[1].Value));//category
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageValue(err.ToList()[1].Key));//name
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageProduct(err.ToList()[3].Value));//note
                    //if (err.ToList()[4].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[4].Key, Texts.GetErrorMessageProduct(err.ToList()[4].Value));//description
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, Texts.GetErrorMessageValue(err.ToList()[2].Key));//no_of_units

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("EditProduct1", new { product_id = model.id });
                }
                else
                {
                    List<string> uploaded_fnames = Paths.GetFileNames(PATH.PRODUCT_DIRECTORY_TMP, dirs, false);
                    DAL.GetInstance().UpdateProduct(model, uploaded_fnames.Where(f => f.Substring(0, 2) != "t_" && f.Substring(0, 2) != "s_").ToList());

                    string tmp_path = Paths.GetPath(PATH.PRODUCT_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = Paths.GetFileNames(PATH.PRODUCT_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true), file, true, false, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                    return RedirectToRoute("EditProduct1", new { product_id = model.id });
                }
                
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.EDITPRODUCT, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult DeleteProduct(long product_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                
                if (!user.OwnsProduct(product_id))
                    throw new Exception("A-OK, handled.");

                poco_product product_poco = DAL.GetInstance().GetProductPOCO(product_id, true, false, false, false, false);
                
                Dictionary<string, string> dirs = new Dictionary<string, string>();
                dirs["identity_id"] = current_user.sysname;
                dirs["booth_sysname"] = product_poco.booth_poco.sysname;
                dirs["product_sysname"] = product_poco.sysname;
                string path = Paths.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true);
                Paths.ClearFolder(path, true, true);

                DAL.GetInstance().DeleteProduct(product_id);

                return RedirectToRoute("EditBooth1", new { booth_id = product_poco.booth_poco.booth_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.DELETEPRODUCT, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult CreateCollection(int booth_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                ViewBag.CurrentUser = current_user;

                if (!user.OwnsBooth(booth_id))
                    throw new Exception("A-OK, handled.");

                //EbazarDB _db = DAL.GetInstance().GetContext();
                using (EbazarDB _db = new EbazarDB())
                {

                    poco_collection model = TempData["datacontainer"] as poco_collection;
                    if (model == null)//ny
                    {
                        poco_collection collection_poco = new poco_collection();
                        poco_booth booth_poco = new poco_booth();
                        booth b = booth_poco.GetBooth(booth_id, "", "", true, true, true, true, false, false, true);

                        booth_poco.ToPoco(b, null);

                        collection_poco.booth_poco = booth_poco;
                        collection_poco.category_main_id = _db.category.Where(x => x.name != ".ingen" && x.is_parent && x.priority == 1).FirstOrDefault().Id;

                        collection_poco.ToPoco(new collection(), new List<poco_booth.Hit>(), "");
                        collection_poco.SetupToClient<poco_product>();

                        ViewBag.StatusCondition = collection_poco.status_condition_selectlist;
                        ViewBag.StatusStock = collection_poco.status_stock_selectlist;
                        ViewBag.CollectionCategoryMain = collection_poco.category_main_selectlist;
                        ViewBag.CollectionCategorySecond = collection_poco.category_second_selectlist;

                        if (ThisSession.Json_Errors != null)
                        {
                            ViewBag.JSON_ERRORS = "";
                            string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                            if (!string.IsNullOrEmpty(s) && s != "{}")
                                ViewBag.JSON_ERRORS = s;
                            ThisSession.Json_Errors = null;
                        }

                        return View("CreateCollection", collection_poco);
                    }
                    else//der er errors
                    {
                        poco_product product_poco = new poco_product(false);
                        model.booth_poco = DAL.GetInstance(/*true*/).GetBoothPOCO(booth_id, "", "", true, false, false, true, false, false, true);
                    
                        model.product_pocos = product_poco.GetProductPOCOsByCollectionId((int)model.id, false, false);
                    
                        model.SetupToClient<poco_collection>();
                        ViewBag.StatusCondition = model.status_condition_selectlist;
                        ViewBag.StatusStock = model.status_stock_selectlist;
                        ViewBag.CollectionCategoryMain = model.category_main_selectlist;
                        ViewBag.CollectionCategorySecond = model.category_second_selectlist;

                        if (ThisSession.Json_Errors != null)
                        {
                            ViewBag.JSON_ERRORS = "";
                            string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                            if (!string.IsNullOrEmpty(s) && s != "{}")
                                ViewBag.JSON_ERRORS = s;
                            ThisSession.Json_Errors = null;
                        }

                        return View("CreateCollection", model);
                    }
                }
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.CREATECOLLECTION, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        public ActionResult CreateCollection(poco_collection model)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                
                poco_person salesman_poco = current_user;
                string status_condition_select = Request != null ? Request.Form["status_condition_select"].ToString() : "TEST_COLLECTION";
                string status_stock_select = "PÅ_LAGER";
                
                Dictionary<string, ERROR_MESSAGE> err = Setup.SetupCollectionFromClient(ref model, model.booth_id, status_condition_select, status_stock_select, DAL.GetInstance());
                
                Dictionary<string, string> dirs = Setup.SetupCollectionDirs(model, salesman_poco.sysname);

                if (CheckHelper.ErrorCollection.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if(err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, Texts.GetErrorMessageValue(err.ToList()[0].Key));//price
                    //if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageCollection(err.ToList()[1].Value));//category
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageValue(err.ToList()[1].Key));//name
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageCollection(err.ToList()[3].Value));//note
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageCollection(err.ToList()[3].Value));//description

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("CreateCollection", new { booth_id = model.booth_poco.booth_id });
                }
                else
                {
                    model.sysname = dirs["collection_sysname"];
                    List<string> uploaded_fnames = Paths.GetFileNames(PATH.COLLECTION_DIRECTORY_TMP, dirs, false);
                    int collection_id = DAL.GetInstance().SaveCollection(model, uploaded_fnames.Where(f => f.Substring(0, 2) != "t_").ToList());
                    model.id = collection_id;//for test

                    string tmp_path = Paths.GetPath(PATH.COLLECTION_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = Paths.GetFileNames(PATH.COLLECTION_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true), file, true, false, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }                        
                    }

                    return RedirectToRoute("EditCollection1", new { collection_id = collection_id });
                }
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.CREATECOLLECTION, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult EditCollection(int collection_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                ViewBag.CurrentUser = current_user;

                if (!user.OwnsCollection(collection_id))
                    throw new Exception("A-OK, handled.");

                poco_collection collection_poco = DAL.GetInstance(/*true*/).GetCollectionPOCO(collection_id, true, true, true, true, true);
                    
                List<poco_params> list = DAL.GetInstance(/*true*/).GetParams((int)collection_poco.category_main_id, (int)collection_poco.category_second_id);
                ViewBag.Params = list;
                ViewBag.StatusCondition = collection_poco.status_condition_selectlist;
                ViewBag.StatusStock = collection_poco.status_stock_selectlist;
                ViewBag.CollectionCategoryMain = collection_poco.category_main_selectlist;
                ViewBag.CollectionCategorySecond = collection_poco.category_second_selectlist;
                ViewBag.LevelA = collection_poco.foldera_selectlist;
                ViewBag.LevelB = collection_poco.folderb_selectlist;
                    
                if (ThisSession.Json_Errors != null)
                {
                    ViewBag.JSON_ERRORS = "";
                    string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                    if (!string.IsNullOrEmpty(s) && s != "{}")
                        ViewBag.JSON_ERRORS = s;
                    ThisSession.Json_Errors = null;
                }

                return View("EditCollection", collection_poco);

            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.EDITCOLLECTION, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        public ActionResult EditCollection(poco_collection model)
        {
            try 
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");

                poco_person salesman_poco = current_user;
                string status_condition_select = Request != null ? Request.Form["status_condition_select"].ToString() : "TEST_COLLECTION";
                string status_stock_select = "PÅ_LAGER";
                
                Dictionary<string, ERROR_MESSAGE> err = Setup.SetupCollectionFromClient(ref model, model.booth_id, status_condition_select, status_stock_select, DAL.GetInstance(/*true*/));
                
                Dictionary<string, string> dirs = Setup.SetupCollectionDirs(model, salesman_poco.sysname);
                
                if (CheckHelper.ErrorCollection.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, Texts.GetErrorMessageValue(err.ToList()[0].Key));//price
                    //if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageCollection(err.ToList()[1].Value));//category
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageValue(err.ToList()[1].Key));//name
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageCollection(err.ToList()[3].Value));//note
                    //if (err.ToList()[4].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[4].Key, Texts.GetErrorMessageCollection(err.ToList()[4].Value));//description

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("EditCollection1", new { collection_id = model.id });
                }
                else
                {
                    List<string> uploaded_fnames = Paths.GetFileNames(PATH.COLLECTION_DIRECTORY_TMP, dirs, false);
                    DAL.GetInstance().UpdateCollection(model, uploaded_fnames.Where(f => f.Substring(0, 2) != "t_" && f.Substring(0, 2) != "s_").ToList());

                    string tmp_path = Paths.GetPath(PATH.COLLECTION_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = Paths.GetFileNames(PATH.COLLECTION_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true), file, true, false, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                Paths.MoveFile(tmp_path, file, Paths.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                        //Paths.MoveFile("t_" + file, Paths.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true));
                    }

                    return RedirectToRoute("EditCollection1", new { collection_id = model.id });
                }
                
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.EDITCOLLECTION, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult DeleteCollection(int collection_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();
                                
                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                
                if (!user.OwnsCollection(collection_id))
                    throw new Exception("A-OK, handled.");

                poco_collection collection_poco = DAL.GetInstance().GetCollectionPOCO(collection_id, false, true, false, false, false);
                
                Dictionary<string, string> dirs = new Dictionary<string, string>();
                dirs["identity_id"] = current_user.sysname;
                dirs["booth_sysname"] = collection_poco.booth_poco.sysname;
                dirs["collection_sysname"] = collection_poco.sysname;
                string path = Paths.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true);
                Paths.ClearFolder(path, true, true);

                DAL.GetInstance().DeleteCollection(collection_id);

                return RedirectToAction("EditBooth", new { booth_id = collection_poco.booth_poco.booth_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.DELETECOLLECTION, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult AddProductToCollection(int collection_id, long product_id)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                DAL.GetInstance(/*true*/).AddProductToCollection(collection_id, product_id);

                return RedirectToRoute("EditCollection1", new { collection_id = collection_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.ADDPRODUCTTOCOLLECTION, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult RemoveProductFromCollection(int collection_id, long product_id)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                DAL.GetInstance(/*true*/).RemoveProductFromCollection(collection_id, product_id);

                return RedirectToRoute("EditCollection1", new { collection_id = collection_id });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVEPRODUCTFROMCOLLECTION, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        public void UploadImage()
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                TYPE type;
                string typeFile = System.Web.HttpContext.Current.Request.Form["TypeFile"];
                Enum.TryParse(typeFile, out type);
                //TYPE typeFile = typeFile == "Booth" ? TYPE.BOOTH : typeFile == "Product" ? TYPE.PRODUCT : typeFile == "Collection" ? TYPE.COLLECTION : TYPE.PROFILE;
                if (System.Web.HttpContext.Current.Request.Files.Count <= 0)
                {
                    System.Web.HttpContext.Current.Response.Write("No file uploaded");
                    return;
                }
                else if (System.Web.HttpContext.Current.Request.Form != null)
                {
                    HttpPostedFile file = System.Web.HttpContext.Current.Request.Files["images"];
                    if (file != null)
                        UploadImage(file, type);
                    else
                        System.Web.HttpContext.Current.Response.Write("No file uploaded");
                    return;
                }
                throw new Exception("A-OK, handled.");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.UPLOADIMAGE, e);
                TempData["err_msg"] = err_msg;
                AjaxError("Der skete en fejl!");
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

        public void UploadImage(HttpPostedFile file, TYPE type)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            poco_person currentuser = CurrentUser.GetInstance().GetCurrentUser(false, false, false);
            if (currentuser == null)
                throw new Exception("A-OK, handled.");
            dirs["identity_id"] = currentuser.sysname;

            string ext = file.FileName.Substring(file.FileName.LastIndexOf('.'));
            string name = Paths.GenerateFileName(file.FileName.Substring(0, file.FileName.IndexOf('.')), FILE_NAME.NONE);
            if (/*ext.ToLower().Contains("gif") || */ext.ToLower().Contains("jpg") || ext.ToLower().Contains("jpeg") || ext.ToLower().Contains("png"))
            {
                using (Stream inputStream = file.InputStream)
                {
                    PATH path_tmp = ImageHelper.GetPath(type);
                    string path = Paths.GetPath(path_tmp, dirs, true);
                    Paths.CreatePath(path);
                    string[] files = System.IO.Directory.GetFiles(path);

                    foreach (string s in files)
                        System.IO.File.Delete(s);

                    MemoryStream memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }
                    System.Drawing.Image fullsizeImage = ImageHelper.FixOrientation(memoryStream);
                    if (type == TYPE.BOOTH || type == TYPE.PROFILE)
                    {
                        int w = type == TYPE.BOOTH ? 300 : 200;
                        int h = type == TYPE.BOOTH ? 200 : 250;
                        fullsizeImage = ImageHelper.CropImage(fullsizeImage, type);                        
                        fullsizeImage = ImageHelper.ResizeImage(fullsizeImage, w, h);
                    }
                    else
                    {
                        fullsizeImage = ImageHelper.ResizeImageKeepRatio(fullsizeImage, 600, 600);
                    }

                    double ratio = (double)fullsizeImage.Height / (double)fullsizeImage.Width;
                    double scale = (double)128 / (double)fullsizeImage.Width;
                    
                    int newWidth = (int)Math.Round((((double)fullsizeImage.Width) * scale));
                    int newHeight = (int)Math.Round((((double)fullsizeImage.Height) * scale));

                    System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);

                    System.IO.MemoryStream fullsize = new System.IO.MemoryStream();
                    System.IO.MemoryStream thumbnail = new System.IO.MemoryStream();
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    newImage.Save(thumbnail, format);  //Or whatever format you want.
                    fullsizeImage.Save(fullsize, format);
                    
                    System.IO.File.WriteAllBytes(path + name + ".jpg", fullsize.ToArray());
                    System.IO.File.WriteAllBytes(path + "t_" + name + ".jpg", thumbnail.ToArray());
                    System.Web.HttpContext.Current.Response.Write("File uploaded");
                    return;
                }
            }
            throw new Exception("A-OK, handled.");
        }
                
        [HttpPost]
        public JsonResult RemoveImage(string ImageName, string BoothId, string ItemId, string type)
        {

            try
            {
                if (!access.Queue())
                    throw new Exception();

                SetupCurrentUser();

                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                
                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                poco_person person_poco = current_user;
                PATH path_tmp;
                poco_booth booth_poco = null;
                IBoothItem item_poco = null;
                if (typeEnum == TYPE.PROFILE)
                {
                    path_tmp = PATH.PROFILE_DIRECTORY_NAME;

                    if (person_poco != null)
                        person_poco.RemoveImage();
                    else
                        throw new Exception("A-OK, handled.");
                }
                else
                {
                    booth_poco = new poco_booth();
                    booth_poco = DAL.GetInstance(/*true*/).GetBoothPOCO(int.Parse(BoothId), "", "", true, true, true, true, false, false, true);
                    
                    if (typeEnum == TYPE.BOOTH)
                    {
                        path_tmp = PATH.BOOTH_DIRECTORY_NAME;
                        
                        booth_poco.RemoveImage();
                    }
                    else
                    {
                        if (typeEnum == TYPE.PRODUCT)
                        {
                            path_tmp = PATH.PRODUCT_DIRECTORY_NAME;
                            item_poco = booth_poco.product_pocos.Where(p => p.id == long.Parse(ItemId)).FirstOrDefault();
                        }
                        else
                        {
                            path_tmp = PATH.COLLECTION_DIRECTORY_NAME;
                            item_poco = booth_poco.collection_pocos.Where(c => c.id == long.Parse(ItemId)).FirstOrDefault();
                        }

                        if (item_poco != null)
                        {
                            //item_poco.db = new EbazarDB();
                            item_poco.RemoveImage(ImageName);
                        }
                        else
                            throw new Exception("A-OK, handled.");
                    }
                }
                Dictionary<string, string> dirs = new Dictionary<string, string>();
                dirs["identity_id"] = person_poco.sysname;
                if (typeEnum != TYPE.PROFILE)
                    dirs["booth_sysname"] = booth_poco.sysname;
                if (typeEnum == TYPE.PRODUCT)
                    dirs["product_sysname"] = item_poco.sysname;
                if (typeEnum == TYPE.COLLECTION)
                    dirs["collection_sysname"] = item_poco.sysname;
                string path = Paths.GetPath(path_tmp, dirs, true);

                bool clearpath = typeEnum == TYPE.PROFILE;// || typeEnum == TYPE.COLLECTION || typeEnum == TYPE.PRODUCT;
                Paths.DeleteFile(path, ImageName, false);
                Paths.DeleteFile(path, "t_" + ImageName, clearpath);

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVEPRODUCTIMAGE, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("Der skete en fejl!");
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
        public JsonResult GetTags(string TagName)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //string contains = System.Web.HttpContext.Current.Request.Form["TagName"];
                bool ok;
                TagName = StringHelper.OnlyAlphanumeric(TagName.ToLower().Trim(), false, false, "notag", CharacterHelper.Space(), out ok);
                List<poco_tag> tag_pocos = DAL.GetInstance(/*true*/).Get5TagPOCOs(TagName);

                if (tag_pocos != null)
                    return Json(new { success = true, tags = tag_pocos });
                else
                    return Json(new { success = false });
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
                
        [HttpPost]
        public JsonResult SaveTag(string tag_name, string id, string type)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //string tag_name = System.Web.HttpContext.Current.Request.Form["TagName"];
                //string product_id = System.Web.HttpContext.Current.Request.Form["ProductId"];
                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                string msg = "ok";
                long tag_id = 0;
                bool success = true;
                bool ok;
                tag_name = StringHelper.OnlyAlphanumeric(tag_name.ToLower().Trim(), false, false, "notag", CharacterHelper.Space(), out ok);

                if (tag_name.Split(' ').Length > 3)
                    msg = "Max 3 søgeord";
                else
                {
                    MESSAGE_TAG result = DAL.GetInstance(/*true*/).SaveTag(tag_name, id, typeEnum);

                    if (result == MESSAGE_TAG.OK)
                        tag_id = DAL.GetInstance(/*true*/).GetTag(tag_name).Id;
                    else if (result == MESSAGE_TAG.MAXLIMIT)
                        msg = "Max grænse nået!";
                    else
                    {
                        msg = "Der skete en fejl, beklager.";
                        success = false;
                    }
                }
                
                return Json(new { tag_id = tag_id, tag_name = tag_name, msg = msg, success = success });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.SAVETAG, e);
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
        public JsonResult RemoveTag(string TagId, string Id, string type)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //string tag_name = System.Web.HttpContext.Current.Request.Form["TagName"];
                //string product_id = System.Web.HttpContext.Current.Request.Form["ProductId"];
                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                long tag_id = 0;
                string msg = "";
                bool ok;
                ok = DAL.GetInstance(/*true*/).RemoveTag(long.Parse(TagId), typeEnum, Id, false);

                if (ok)
                    tag_id = long.Parse(TagId);
                else
                    msg = "err_msg";
                return Json(new { success = ok, tag_id = tag_id, msg = msg });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVETAG, e);
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
        public JsonResult _SaveParam(long id, int param_id, int val_id, string type)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();
                //access.Queue();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, Check.");
                //if (current_user.test == "TEST")
                //    return Json(new { msg = "slået fra til test.", success = false });

                //string tag_name = System.Web.HttpContext.Current.Request.Form["TagName"];
                //string product_id = System.Web.HttpContext.Current.Request.Form["ProductId"];
                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                bool result = DAL.GetInstance(/*true*/).SaveParam(id, param_id, val_id, typeEnum);

                string msg = "ok";
                bool success = true;

                if (result)
                {
                    //if (typeEnum == TYPE.PRODUCT)
                    //    Cache.products_ok = false;
                    //if (typeEnum == TYPE.COLLECTION)
                    //    Cache.collections_ok = false;
                    //if (typeEnum == TYPE.SHOP)
                    //    Cache.shops_ok = false;
                }
                else
                {
                    msg = "Der skete en fejl, beklager.";
                    success = false;
                }

                return Json(new { msg = msg, success = success });
            }
            catch (Exception e)
            {
                Statics.Log(err.HandleError(ERROR.SAVEPARAM, e));
                TempData["err_msg"] = err.HandleError(ERROR.SAVEPARAM, e);
                TempData["ErrorMessage"] = "";
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
        public JsonResult _RemoveParam(int param_id, long id, string type)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //access.Queue();

                SetupCurrentUser();
                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, Check.");
                //if (current_user.test == "TEST")
                //    return Json(new { msg = "slået fra til test.", success = false });

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                string msg = "";
                bool ok;
                ok = DAL.GetInstance(/*true*/).RemoveParam(param_id, typeEnum, id);

                if (ok)
                {
                    //if (typeEnum == TYPE.PRODUCT)
                    //    Cache.products_ok = false;
                    //if (typeEnum == TYPE.COLLECTION)
                    //    Cache.collections_ok = false;
                    //if (typeEnum == TYPE.SHOP)
                    //    Cache.shops_ok = false;
                }
                else
                    msg = "err_msg";
                return Json(new { success = ok, msg = msg });
            }
            catch (Exception e)
            {
                Statics.Log(err.HandleError(ERROR.REMOVEPARAM, e));
                TempData["err_msg"] = err.HandleError(ERROR.REMOVEPARAM, e);
                TempData["ErrorMessage"] = "";
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
        public JsonResult AddCategory(int CatId, int BoothId)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //string tag_name = System.Web.HttpContext.Current.Request.Form["TagName"];
                //string product_id = System.Web.HttpContext.Current.Request.Form["ProductId"];

                long cat_id = 0;
                string msg = "";
                string cat_name = DAL.GetInstance(/*true*/).AddCategory(CatId, BoothId);

                if (!string.IsNullOrEmpty(cat_name))
                    cat_id = CatId;
                else
                    msg = "err_msg";
                
                return Json(new { cat_id = cat_id, cat_name = cat_name, msg = msg, success = !string.IsNullOrEmpty(cat_name) });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.SAVETAG, e);
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
        public JsonResult RemoveCategory(int CatId, int BoothId)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();
                //string tag_name = System.Web.HttpContext.Current.Request.Form["TagName"];
                //string product_id = System.Web.HttpContext.Current.Request.Form["ProductId"];
                long cat_id = 0;
                string msg = "";
                bool ok = DAL.GetInstance(/*true*/).RemoveCategory(CatId, BoothId);

                if (ok)
                    cat_id = CatId;
                else
                    msg = "err_msg";
                return Json(new { success = ok, cat_id = cat_id, msg = msg });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVETAG, e);
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
        public JsonResult DeleteConversation(long conversation_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                DAL.GetInstance(/*true*/).DeleteConversation(conversation_id);
                return Json( new { success = true });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.DELETECONVERSATION, e);
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

        public ActionResult RemoveFavorite(long product_id, int collection_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                SetupCurrentUser();

                DAL.GetInstance(/*true*/).RemoveFavorite(product_id, collection_id);
                return RedirectToAction("Userprofile", new { coming_from = "RemoveFavorite" });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVEFAVORITE, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult RemoveFollowing(int booth_id)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                SetupCurrentUser();

                DAL.GetInstance(/*true*/).RemoveFollowing(booth_id);
                return RedirectToRoute("Userprofile");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVEFAVORITE, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult CreateFolder(string fld_name, string id, string type, string booth_id)
        {
            //SetupCurrentUser();
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                ThisSession.Tab = "categorys";

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                bool ok;
                fld_name = StringHelper.OnlyAlphanumeric(fld_name, false, true, "notag", CharacterHelper.Limited(false), out ok);
                if (ok)
                    DAL.GetInstance(/*true*/).CreateFolder(fld_name, int.Parse(id), typeEnum);
                else
                {
                    Dictionary<string, SYSTEM_MESSAGE> err = Setup.SetupCreateLevel(true);
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != SYSTEM_MESSAGE.NO_MESSAGE)
                        errors.Add(err.ToList()[0].Key, Texts.GetSystemMessageValue(err.ToList()[0].Key.ToString()));//price
                    if (ThisSession.Json_Messages != null)
                        ThisSession.Json_Messages = errors;
                }
                return RedirectToRoute("EditBooth1", new { booth_id = int.Parse(booth_id) });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.CREATELEVEL, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult MoveFolder(string fld_id, string direction, string id, string type, string booth_id)
        {
            //SetupCurrentUser();
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                DAL.GetInstance(/*true*/).MoveFolder(int.Parse(fld_id), direction, int.Parse(id), typeEnum);
                ThisSession.Tab = "categorys";
                return RedirectToRoute("EditBooth1", new { booth_id = int.Parse(booth_id) });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.MOVELEVEL, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult DeleteFolder(string fld_id, string id, string type, string booth_id)
        {
            //SetupCurrentUser();
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                DAL.GetInstance(/*true*/).DeleteFolder(int.Parse(fld_id), int.Parse(id), typeEnum);
                ThisSession.Tab = "categorys";
                return RedirectToRoute("EditBooth1", new { booth_id = int.Parse(booth_id) });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.DELETELEVEL, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult SetFolder(string fld_id, string id, string type, string is_product)
        {
            //SetupCurrentUser();
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                DAL.GetInstance(/*true*/).SetFolder(int.Parse(fld_id), id, typeEnum, is_product == "true");
                ThisSession.Tab = "categorys";
                if (is_product == "true")
                    return RedirectToRoute("EditProduct1", new { product_id = long.Parse(id) });
                else
                    return RedirectToRoute("EditCollection1", new { collection_id = int.Parse(id) });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.SETLEVEL, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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
        public JsonResult GetEmail(string booth_id)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                int _id;
                if(int.TryParse(booth_id, out _id))
                {
                    poco_booth booth_poco = DAL.GetInstance(/*true*/).GetBoothPOCO(_id, "", "", true, false, false, false, false, false, true);
                
                    //if (booth_poco != null)
                    //{
                        if(booth_poco.salesman_poco.request_email)
                            return Json(new { success = true, ok = true, value = booth_poco.salesman_poco.email });
                        return Json(new { success = true, ok = false, value = "" });
                    //}
                    //else
                    //    return Json(new { success = false });
                }
                return Json(new { success = false });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.GETADDRESSEMAIL, e);
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
        public JsonResult GetAddressTown(string Zip)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //string zip = System.Web.HttpContext.Current.Request.Form["Zip"];

                string town = DAL.GetInstance(/*true*/).GetAddressTown(Zip);
                //List<string> res = new List<string>();
                //res.Add(town);
                //return Json(res);
                return Json(new { success=true, town=town });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.GETADDRESSTOWN, e);
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
        public JsonResult Maintenance(bool run)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                if (run)
                    Statics.Maintenance = !Statics.Maintenance;
                return Json(new { success = Statics.Maintenance });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVETAG, e);
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
        public JsonResult SetActive(int Id, string Value, string Type)
        {
            //SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();

                //string tag_name = System.Web.HttpContext.Current.Request.Form["TagName"];
                //string product_id = System.Web.HttpContext.Current.Request.Form["ProductId"];
                bool value = Value.ToLower() == "true" ? true : false;
                string res = DAL.GetInstance(/*true*/).SetActive(Id, value, Type);
                string msg = res == "true" ? "[aktiv]" : "[ikke aktiv]";
                return Json(new { success = res == "err" ? false : true, msg = msg});
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVETAG, e);
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
        public JsonResult ChangeBoothId(int BoothId, long ProductId)
        {
            SetupCurrentUser();

            try
            {
                if (!access.Queue())
                    throw new Exception();
                string a = "";
                string b = a;
                //string tag_name = System.Web.HttpContext.Current.Request.Form["TagName"];
                //string product_id = System.Web.HttpContext.Current.Request.Form["ProductId"];
                if (DAL.GetInstance(/*true*/).ChangeBoothId(BoothId, ProductId))
                    return Json(new { success = true });
                else
                    return Json(new { success = false, msg = "Varen er del af Sæt." });
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVETAG, e);
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
        public ActionResult Feedback(dto_email mail)
        {
            try
            {
                if (!access.Queue())
                    throw new Exception();

                //if (ThisSession.IsMobile == "none")
                //    return View("IsMobile");

                SetupCurrentUser();

                CurrentUser user = CurrentUser.GetInstance();
                poco_person current_user = user.GetCurrentUser(false, true, true);
                if (current_user == null)
                    throw new Exception("A-OK, handled.");
                //ViewBag.CurrentUser = current_user;
                poco_person per = current_user;
                
                Dictionary<string, SYSTEM_MESSAGE> err = Setup.Feedback(mail);
                Dictionary<string, string> errors = new Dictionary<string, string>();
                if (err.ToList()[0].Value != SYSTEM_MESSAGE.NO_MESSAGE)
                    errors.Add(err.ToList()[0].Key, Texts.GetSystemMessageValue(err.ToList()[0].Key.ToString()));//price
                else
                {
                    errors.Add(err.ToList()[0].Key, "Besked sendt, hold øje med spam folder for svar.");//price
                    
                    string subject = "Feedback: " + mail.Subject;
                    string body = mail.Message.Replace("\r\n", "<br />");
                    Admin.Notification.Run(per.email, "admin@e-bazar.dk", "admin@e-bazar.dk", subject, body);
                }

                
                if (HttpContext != null)
                    ThisSession.Json_Messages = errors;

                return RedirectToRoute("UserProfile");
            }
            catch (Exception e)
            {
                string err_msg = err.HandleError(ERROR.REMOVETAG, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
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

        public ActionResult ErrorPage()
        {
            string err_msg = TempData["err_msg"] as string;
            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;

            //if (string.IsNullOrEmpty(ViewBag.ErrorMessage) && string.IsNullOrEmpty(err_msg))
            //    return HttpNotFound(HttpStatusCode.NotFound.ToString());

            //if (err_msg.Contains("A-OK, Handled."))
            //    return HttpNotFound(HttpStatusCode.NotFound.ToString());

            string subject = "Der er sket en fejl!";
            string body = "ViewBag: " + ViewBag.ErrorMessage + "<br />" +
                      "MSG: " + /*Extensions.Extensions.HtmlEncode(*/err_msg/*)*/;
            Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);
            
            return View("ErrorPage");
        }

        [AllowAnonymous]
        public ActionResult NotFound()
        {
            //if (ThisSession.IsMobile == "none")
            //    return View("IsMobile");

            string subject = "NotFound!";
            string body = Extensions.Extensions.HtmlEncode("Not Found.");
            Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);

            return HttpNotFound(HttpStatusCode.NotFound.ToString());
        }

        private void AjaxError(string user_msg)
        {
            string err_msg = TempData["err_msg"] as string;

            string subject = "Der er sket en fejl!";
            string body = Extensions.Extensions.HtmlEncode(err_msg);
            Admin.Notification.Run("mail@e-bazar.dk", "mail@e-bazar.dk", "mail@e-bazar.dk", subject, body);

            System.Web.HttpContext.Current.Response.Write(user_msg);//bliver nok ikke fanget på klienten
        }

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