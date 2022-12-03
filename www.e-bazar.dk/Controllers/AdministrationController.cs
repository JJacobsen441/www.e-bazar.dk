using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using PostgreSQL.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.Models.Identity;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Controllers
{
    public class AdministrationController : Controller, IControllerSetup
    {
        public dto_person current_user { get; set; }
        public Access access { get; set; }
                
        protected UserManager<ApplicationUser> UserManager { get; set; }
        protected ApplicationDbContext ApplicationDbContext { get; set; }

        public AdministrationController() : base()
        {
            string guid = Guid.NewGuid().ToString();
            access = new Access("entry", guid);
            
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
        }
        
        public AdministrationController(UserManager<ApplicationUser> userManager) : base()
        {
            string guid = Guid.NewGuid().ToString();
            access = new Access("entry", guid);

            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = userManager;
        }

        //public Params GetSetup()
        //{
        //    Params _p = new Params()
        //    {
        //        bag = ViewBag,
        //        access = this.access,
        //        current_user = this.current_user
        //    };

        //    return _p;
        //}

        private T SetupUserProfile<T>(string userid, ref col_userprofile userprofile, string role_name) where T: dto_person, new()
        {
            T person_poco = (T)DAL.GetInstance().GetPersonDTO<T>(userid, true, true, true);
                        
            bool is_salesman = typeof(dto_salesman) == typeof(T);
            List<dto_conversation> lista = DAL.GetInstance().GetConversationsPerson(userid, is_salesman, true);
            if (lista == null)
                return null;
            
            List<dto_conversation> own = lista.Where(c => c.person_id == userid).ToList();
            own = own.Where(c=>c.comment_dtos.Count() > 0).OrderBy(c => c.comment_dtos.OrderBy(com => com.created_on).FirstOrDefault().created_on).ToList();
            
            List<dto_conversation> items = lista.Where(c => c.product_dto != null && c.person_id != userid).ToList();
            items = items.Concat(lista.Where(c => c.collection_dto != null && c.person_id != userid)).ToList();
            items = items.OrderBy(c => c.comment_dtos.OrderBy(com => com.created_on).FirstOrDefault().created_on).ToList();
            
            List<dto_conversation> booths = lista.Where(c => c.booth_dto != null && c.person_id != userid).ToList();
            booths = booths.OrderBy(c => c.comment_dtos.OrderBy(com => com.created_on).FirstOrDefault().created_on).ToList();
            col_conversations conversations = new col_conversations(own, booths, items, is_salesman);

            List<dto_booth_item> follower_news = new List<dto_booth_item>();
            biz_booth biz = new biz_booth();
            foreach (dto_booth dto in person_poco.following)
                follower_news = follower_news.Concat(biz.GetRelevantItems(dto, 0, 5)).ToList();
            follower_news = follower_news.OrderByDescending(i => i.created_on).Take(5).ToList();
            
            if (person_poco.GetType() == typeof(dto_salesman))
            {
                userprofile.customer_dto = null;
                userprofile.salesmen = null;
                userprofile.customers = null;
                userprofile.booth_dtos = DAL.GetInstance().GetBoothDTOs(userid, false);
                userprofile.follower_news = follower_news;
                userprofile.conversations_dto = conversations;
            }
            else
            {
                userprofile.salesman_dto = null;
                userprofile.salesmen = null;
                userprofile.customers = null;
                userprofile.booth_dtos = new List<dto_booth>();
                userprofile.follower_news = follower_news;
                userprofile.conversations_dto = conversations;
            }
            
            return person_poco;
        }
        
        [HttpGet]///
        [Route("administration/redigerprofil", Name = "UserProfile")]
        public ActionResult UserProfile()
        {

            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, false, true);

                ControllerHelper.JsonError(this);
                ControllerHelper.JsonMessage(this);
                
                string id = CurrentUser.Inst().CurrentUserID;
                List<string> role_names = UserManager.GetRoles(id).ToList();
                string role_name = role_names.Contains("Administrator") ? "Administrator" : role_names.Contains("Salesman") ? "Salesman" : "Customer";

                col_userprofile profile = new col_userprofile();
                dto_salesman salesman_dto;
                dto_customer customer_dto;
                List<biz_booth> booth_pocos = new List<biz_booth>();
                if (role_name == "Administrator")
                {
                    return RedirectToRoute("AdminGet");
                }
                else if (role_name == "Salesman")
                {
                    salesman_dto = SetupUserProfile<dto_salesman>(id, ref profile, role_name);
                    profile.salesman_dto = salesman_dto;
                    return View("SalesmanProfile", profile);
                }
                else
                {
                    customer_dto = SetupUserProfile<dto_customer>(id, ref profile, role_name);
                    profile.customer_dto = customer_dto;
                    return View("CustomerProfile", profile);
                }
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.USERPROFILE, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("administration/redigerprofil/sales", Name = "SalesmanProfile")]
        public ActionResult SalesmanProfile(col_userprofile model)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);
                
                Dictionary<string, ERROR_MESSAGE> err = SetupHelper.SetupSalesmanProfileFromClient(ref model);
                Dictionary<string, string> dirs = SetupHelper.SetupProfileDirs(model.salesman_dto);

                if (CheckHelper.ErrorSalesmanProfile.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetErrorMessageValue(err.ToList()[0].Key));
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, TextHelper.GetErrorMessageValue(err.ToList()[1].Key));
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, TextHelper.GetErrorMessageValue(err.ToList()[2].Key));
                    if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[3].Key, TextHelper.GetErrorMessageValue(err.ToList()[3].Key));

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;                    
                }
                else
                {
                    List<string> uploaded_profileimage = PathHelper.GetFileNames(PATH.PROFILE_DIRECTORY_TMP, dirs, false);
                    if (uploaded_profileimage.Count > 0)
                        model.salesman_dto.profileimage = uploaded_profileimage.Where(f => f.Substring(0, 2) != "t_").ToList().FirstOrDefault();
                    DAL.GetInstance().UpdatePerson(model.salesman_dto);

                    string tmp_path = PathHelper.GetPath(PATH.PROFILE_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = PathHelper.GetFileNames(PATH.PROFILE_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true), file, true, true, false, true);
                            file = uploaded_files[1];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                }
                return RedirectToRoute("UserProfile");
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.SALESMANPROFILE, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("administration/redigerprofil/cust", Name = "CustomerProfile")]
        public ActionResult CustomerProfile(col_userprofile model)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);
                
                Dictionary<string, ERROR_MESSAGE> err = SetupHelper.SetupCustomerProfileFromClient(ref model);
                Dictionary<string, string> dirs = SetupHelper.SetupProfileDirs(model.customer_dto);

                if (CheckHelper.ErrorCustomerProfile.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if(err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetErrorMessageValue(err.ToList()[0].Key));
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, TextHelper.GetErrorMessageValue(err.ToList()[1].Key));
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, TextHelper.GetErrorMessageValue(err.ToList()[2].Key));

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                }
                else
                {
                    List<string> uploaded_profileimage = PathHelper.GetFileNames(PATH.PROFILE_DIRECTORY_TMP, dirs, false);
                    if (uploaded_profileimage.Count > 0)
                        model.customer_dto.profileimage = uploaded_profileimage.Where(f => f.Substring(0, 2) != "t_").ToList().FirstOrDefault();
                    DAL.GetInstance().UpdatePerson(model.customer_dto);

                    string tmp_path = PathHelper.GetPath(PATH.PROFILE_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = PathHelper.GetFileNames(PATH.PROFILE_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true), file, true, true, false, true);
                            file = uploaded_files[1];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                }
                return RedirectToRoute("UserProfile");
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.CUSTOMERPROFILE, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]///
        [Route("administration/opretbod", Name = "CreateBooth")]
        public ActionResult CreateBooth()
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                
                dto_booth model = TempData["datacontainer"] as dto_booth;

                if (model == null)//ny
                {
                    biz_salesman salesman_poco = new biz_salesman();
                    biz_booth biz = new biz_booth();
                    dto_booth dto = new dto_booth();
                    booth booth = new booth();
                    person per = salesman_poco.GetPerson(CurrentUser.Inst().CurrentUserID, false, false, false);
                    if (per == null || per.descriminator != "Salesman")
                        throw new Exception("A-OK, Handled.");
                    booth.person = per;
                    booth.person_id = CurrentUser.Inst().CurrentUserID;
                    dto = biz.ToDTO(booth, null);

                    ControllerHelper.JsonError(this);
                    ControllerHelper.JsonMessage(this);

                    string subject = "Ny bod oprettet.";
                    string body = "ny bod oprettet.<br />" +
                                "navn: " + booth.name + "<br /><br />" +
                                "med venlig hilsen<br />" +
                                SettingsHelper.Basic.SITENAME();
                    AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), per.email, SettingsHelper.Basic.EMAIL_MAIL(), subject, body);

                    return View("CreateBooth", dto);
                }
                else//der er errors
                {
                    biz_salesman salesman_poco = new biz_salesman();
                    model.salesman_dto = salesman_poco.GetPersonDTO<dto_salesman>(CurrentUser.Inst().CurrentUserID, true, false, false);
                    if (model.salesman_dto.nator != "Salesman")
                        throw new Exception("A-OK, handled.");
                    model.salesman_id = CurrentUser.Inst().CurrentUserID;

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
                string err_msg = ErrorHelper.HandleError(ERROR.EDITBOOTH, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }
        
        [HttpPost]
        public ActionResult CreateBooth(dto_booth model)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                
                dto_person salesman_poco = current_user;
                Dictionary<string, ERROR_MESSAGE> err = SetupHelper.SetupBoothFromClient(ref model, CurrentUser.Inst().CurrentUserID, DAL.GetInstance(/*true*/));
                Dictionary<string, string> dirs = SetupHelper.SetupBoothDirs(ref model, salesman_poco.sysname);

                if (CheckHelper.ErrorBooth.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if(err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetErrorMessageValue(err.ToList()[0].Key));
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, TextHelper.GetErrorMessageValue(err.ToList()[1].Key));
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, TextHelper.GetErrorMessageValue(err.ToList()[2].Key));
                    //errors.Add(err.ToList()[2].Key, Check.ErrorMessageBooth.GetErrorMessage(err.ToList()[2].Value));
                    //errors.Add(err.ToList()[3].Key, Check.ErrorMessageBooth.GetErrorMessage(err.ToList()[3].Value));

                    if(ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("CreateBooth");
                }
                else
                {
                    List<string> uploaded_fnames = PathHelper.GetFileNames(PATH.BOOTH_DIRECTORY_TMP, dirs, false);
                    model.frontimage = uploaded_fnames.Count > 0 ? uploaded_fnames.Where(f => f.Substring(0, 2) != "t_").ToList().FirstOrDefault() : model.frontimage;
                    model.sysname = dirs["booth_sysname"];
                    int booth_id = DAL.GetInstance().SaveBooth(model);
                    model.booth_id = booth_id;


                    string tmp_path = PathHelper.GetPath(PATH.BOOTH_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = PathHelper.GetFileNames(PATH.BOOTH_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true), file, true, true, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                    return RedirectToRoute("EditBooth1", new { booth_id = booth_id });
                }                
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.CREATEBOOTH, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]///
        [Route("administration/redigerbod/{booth_id}", Name = "EditBooth1")]
        public ActionResult EditBooth(int booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                
                if (booth_id == -1)
                    throw new Exception("A-OK, Check");

                if (!CurrentUser.Inst().OwnsBooth(booth_id))
                    throw new Exception("A-OK, handled.");

                biz_booth model = TempData["datacontainer"] as biz_booth;
                ViewBag.Tab = ThisSession.Tab as string == "categorys" ? "categorys" : "home";
                TempData["datacontainer"] = null;

                ControllerHelper.JsonError(this);
                ControllerHelper.JsonMessage(this);

                dto_booth dto = DAL.GetInstance().GetBoothDTO(booth_id, "", "", true, true, true, true, false, false, true);
                
                List<dto_folder> folder_pocos = DAL.GetInstance().GetFolderTree(booth_id, false);
                col_folders folders_dto = new col_folders(folder_pocos, "");
                biz_category cat_poco = new biz_category();
                List<dto_category> cats = cat_poco._GetAll(true);
                List<dto_booth> booth_dtos = DAL.GetInstance().GetBoothDTOs(CurrentUser.Inst().CurrentUserID, false);
                
                return View("EditBooth", new col_booth(dto, folders_dto, cats, null, null, booth_dtos, false, -1/*dont care*/));

            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.EDITBOOTH, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("administration/redigerbod/post", Name = "EditBooth2")]
        public ActionResult EditBooth(col_booth model)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                               
                dto_booth booth_dto = model.booth_dto;
                dto_person salesman_poco = current_user;
                Dictionary<string, ERROR_MESSAGE> err = SetupHelper.SetupBoothFromClient(ref booth_dto, CurrentUser.Inst().CurrentUserID, DAL.GetInstance());
                
                Dictionary<string, string> dirs = SetupHelper.SetupBoothDirs(ref booth_dto, salesman_poco.sysname);

                if (CheckHelper.ErrorBooth.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetErrorMessageValue(err.ToList()[0].Key));//booth name
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, TextHelper.GetErrorMessageValue(err.ToList()[1].Key));//description
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, TextHelper.GetErrorMessageValue(err.ToList()[2].Key));//part address
                    if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[3].Key, TextHelper.GetErrorMessageValue(err.ToList()[3].Key));//full address
                    //if (err.ToList()[4].Value != ERROR_MESSAGE.OK)
                        //errors.Add(err.ToList()[4].Key, Texts.GetErrorMessageValue(err.ToList()[4].Key));//tags

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    //TempData["datacontainer"] = model.booth_poco;
                    return RedirectToRoute("EditBooth1", new { booth_id = model.booth_dto.booth_id });
                }
                else
                {
                    List<string> uploaded_fnames = PathHelper.GetFileNames(PATH.BOOTH_DIRECTORY_TMP, dirs, false);
                    string current_path = PathHelper.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true);
                    List<string> current_fnames = PathHelper.GetFileNames(PATH.BOOTH_DIRECTORY_NAME, dirs, false);
                    booth_dto.frontimage = uploaded_fnames.Count > 0 ? uploaded_fnames.Where(f => f.Substring(0, 2) != "t_").ToList().FirstOrDefault() : current_fnames.Count > 0 ? booth_dto.frontimage : "";
                    DAL.GetInstance().UpdateBooth(booth_dto);

                    string tmp_path = PathHelper.GetPath(PATH.BOOTH_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = PathHelper.GetFileNames(PATH.BOOTH_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true), file, true, true, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                    return RedirectToRoute("EditBooth1", new { booth_id = booth_dto.booth_id });
                }
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.EDITBOOTH, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]///
        [Route("deletebooth", Name = "DeleteBooth")]
        public ActionResult DeleteBooth(int booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                                
                if (!CurrentUser.Inst().OwnsBooth(booth_id))
                    throw new Exception("A-OK, handled.");

                Dictionary<string, string> dirs;
                string path;
                dto_booth booth_poco = DAL.GetInstance().GetBoothDTO(booth_id, "", "", true, true, true, false, false, true, true);//////////////husk conversations og categories
                
                foreach (dto_product product_poco in booth_poco.product_dtos)
                {
                    DAL.GetInstance().DeleteProduct(product_poco.id);
                }

                foreach (dto_collection collection_poco in booth_poco.collection_dtos)
                {
                    DAL.GetInstance().DeleteCollection((int)collection_poco.id);
                }
                dirs = new Dictionary<string, string>();
                dirs["identity_id"] = current_user.sysname;
                dirs["booth_sysname"] = booth_poco.sysname;
                path = PathHelper.GetPath(PATH.BOOTH_DIRECTORY_NAME, dirs, true);
                PathHelper.ClearFolder(path, true, true);

                DAL.GetInstance().DeleteBooth(booth_id);

                return RedirectToRoute("UserProfile");
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.DELETEBOOTH, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]///
        [Route("administration/opretprodukt", Name = "CreateProduct")]
        public ActionResult CreateProduct(int booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                
                if (!CurrentUser.Inst().OwnsBooth(booth_id))
                    throw new Exception("A-OK, handled.");

                //EbazarDB _db = DAL.GetInstance().GetContext();
                using (EbazarDB _db = new EbazarDB())
                {

                    dto_product model = TempData["datacontainer"] as dto_product;
                    if (model == null)//ny
                    {
                        biz_product product_biz = new biz_product();
                        dto_product product_dto = new dto_product();
                        biz_booth booth_biz = new biz_booth();
                        dto_booth booth_dto = new dto_booth();
                                                
                        product_dto.category_main_id = _db.category.Where(x => x.name != ".ingen" && x.is_parent && x.priority == 1).FirstOrDefault().Id;

                        product_dto = product_biz.ToDTO(new product(), new List<dto_booth.Hit>(), "");
                        product_dto = product_biz.SetupToClient<dto_product>(product_dto);

                        booth b = booth_biz.GetBooth(booth_id, "", "", true, true, true, true, false, false, true);
                        booth_dto = booth_biz.ToDTO(b, null);
                        product_dto.booth_dto = booth_dto;
                    
                        ViewBag.ProductCategoryMain = product_dto.category_main_selectlist;
                        ViewBag.ProductCategorySecond = product_dto.category_second_selectlist;
                        ViewBag.StatusStock = product_dto.status_stock_selectlist;
                        ViewBag.StatusCondition = product_dto.status_condition_selectlist;

                        ControllerHelper.JsonError(this);
                        ControllerHelper.JsonMessage(this);

                        return View("CreateProduct", product_dto);
                    }
                    else//der er errors
                    {
                        model.booth_dto = DAL.GetInstance().GetBoothDTO(booth_id, "", "", true, false, false, false, false, false, true);
                        
                        biz_product product_biz = new biz_product();
                        model = product_biz.SetupToClient<dto_product>(model);
                        ViewBag.StatusCondition = model.status_condition_selectlist;
                        ViewBag.StatusStock = model.status_stock_selectlist;
                        ViewBag.ProductCategoryMain = model.category_main_selectlist;

                        ControllerHelper.JsonError(ViewBag);
                        ControllerHelper.JsonMessage(ViewBag);

                        return View("CreateProduct", model);
                    }
                }
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.CREATEPRODUCT, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }
        
        [HttpPost]
        [Route("administration/opretprodukt", Name = "CreateProduct2")]
        public ActionResult CreateProduct(dto_product model)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                               
                dto_person salesman_poco = current_user;
                string status_condition_select = Request != null ? Request.Form["status_condition_select"].ToString() : "TEST_PRODUCT";
                string status_stock_select = "PÅ_LAGER";
                
                Dictionary<string, ERROR_MESSAGE> err = SetupHelper.SetupProductFromClient(ref model, model.booth_id, status_condition_select, status_stock_select, DAL.GetInstance());
                
                Dictionary<string, string> dirs = SetupHelper.SetupProductDirs(model, salesman_poco.sysname);

                if (CheckHelper.ErrorProduct.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetErrorMessageValue(err.ToList()[0].Key));//price
                    //if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageProduct(err.ToList()[1].Value));//category
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, TextHelper.GetErrorMessageValue(err.ToList()[1].Key));//name
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageProduct(err.ToList()[3].Value));//note
                    //if (err.ToList()[4].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[4].Key, Texts.GetErrorMessageProduct(err.ToList()[4].Value));//description
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, TextHelper.GetErrorMessageValue(err.ToList()[2].Key));//no_of_units

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("CreateProduct", new { booth_id = model.booth_id });
                }
                else
                {
                    model.sysname = dirs["product_sysname"];
                    List<string> uploaded_fnames = PathHelper.GetFileNames(PATH.PRODUCT_DIRECTORY_TMP, dirs, false);
                    long product_id = DAL.GetInstance().SaveProduct(model, uploaded_fnames.Where(f => f.Substring(0, 2) != "t_").ToList());
                    model.id = product_id;//for test

                    string tmp_path = PathHelper.GetPath(PATH.PRODUCT_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = PathHelper.GetFileNames(PATH.PRODUCT_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true), file, true, false, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }                        
                    }
                    
                    return RedirectToRoute("EditProduct1", new { product_id = product_id });
                }
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.CREATEPRODUCT, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("administration/redigerprodukt/{product_id}", Name = "EditProduct1")]
        public ActionResult EditProduct(long product_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                
                if (!CurrentUser.Inst().OwnsProduct(product_id))
                    throw new Exception("A-OK, handled.");

                dto_product product_dto = DAL.GetInstance().GetProductDTO(product_id, true, false, true, true, true);
                
                List<dto_params> list = DAL.GetInstance().GetParams((int)product_dto.category_main_id, (int)product_dto.category_second_id);
                ViewBag.Params = list;
                ViewBag.ProductCategoryMain = product_dto.category_main_selectlist;
                ViewBag.ProductCategorySecond = product_dto.category_second_selectlist;
                ViewBag.LevelA = product_dto.foldera_selectlist;
                ViewBag.LevelB = product_dto.folderb_selectlist;
                ViewBag.StatusStock = product_dto.status_stock_selectlist;
                ViewBag.StatusCondition = product_dto.status_condition_selectlist;

                ControllerHelper.JsonError(this);
                ControllerHelper.JsonMessage(this);

                return View("EditProduct", product_dto);
                
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.EDITPRODUCT, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("administration/redigerprodukt/post", Name = "EditProduct2")]
        public ActionResult EditProduct(dto_product model)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                                
                dto_person salesman_poco = current_user;
                string status_condition_select = Request != null ? Request.Form["status_condition_select"].ToString() : "TEST_PRODUCT";
                string status_stock_select = "PÅ_LAGER";
                
                Dictionary<string, ERROR_MESSAGE> err = SetupHelper.SetupProductFromClient(ref model, model.booth_id, status_condition_select, status_stock_select, DAL.GetInstance(/*true*/));
                
                Dictionary<string, string> dirs = SetupHelper.SetupProductDirs(model, salesman_poco.sysname);
                               
                if (CheckHelper.ErrorProduct.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetErrorMessageValue(err.ToList()[0].Key));//price
                    //if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageProduct(err.ToList()[1].Value));//category
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, TextHelper.GetErrorMessageValue(err.ToList()[1].Key));//name
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageProduct(err.ToList()[3].Value));//note
                    //if (err.ToList()[4].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[4].Key, Texts.GetErrorMessageProduct(err.ToList()[4].Value));//description
                    if (err.ToList()[2].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[2].Key, TextHelper.GetErrorMessageValue(err.ToList()[2].Key));//no_of_units

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("EditProduct1", new { product_id = model.id });
                }
                else
                {
                    List<string> uploaded_fnames = PathHelper.GetFileNames(PATH.PRODUCT_DIRECTORY_TMP, dirs, false);
                    DAL.GetInstance().UpdateProduct(model, uploaded_fnames.Where(f => f.Substring(0, 2) != "t_" && f.Substring(0, 2) != "s_").ToList());

                    string tmp_path = PathHelper.GetPath(PATH.PRODUCT_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = PathHelper.GetFileNames(PATH.PRODUCT_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true), file, true, false, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                    }
                    return RedirectToRoute("EditProduct1", new { product_id = model.id });
                }
                
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.EDITPRODUCT, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]///
        [Route("deleteproduct", Name = "DeleteProduct")]
        public ActionResult DeleteProduct(long product_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                                
                if (!CurrentUser.Inst().OwnsProduct(product_id))
                    throw new Exception("A-OK, handled.");

                dto_product product_dto = DAL.GetInstance().GetProductDTO(product_id, true, false, false, false, false);
                
                Dictionary<string, string> dirs = new Dictionary<string, string>();
                dirs["identity_id"] = current_user.sysname;
                dirs["booth_sysname"] = product_dto.booth_dto.sysname;
                dirs["product_sysname"] = product_dto.sysname;
                string path = PathHelper.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs, true);
                PathHelper.ClearFolder(path, true, true);

                DAL.GetInstance().DeleteProduct(product_id);

                return RedirectToRoute("EditBooth1", new { booth_id = product_dto.booth_dto.booth_id });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.DELETEPRODUCT, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("administration/opretsamling", Name = "CreateCollection")]
        public ActionResult CreateCollection(int booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                
                if (!CurrentUser.Inst().OwnsBooth(booth_id))
                    throw new Exception("A-OK, handled.");
                                
                using (EbazarDB _db = new EbazarDB())
                {

                    dto_collection model = TempData["datacontainer"] as dto_collection;
                    if (model == null)//ny
                    {
                        dto_collection collection_dto = new dto_collection();
                        biz_collection collection_biz = new biz_collection();
                        biz_booth booth_biz = new biz_booth();
                        
                        collection_dto.category_main_id = _db.category.Where(x => x.name != ".ingen" && x.is_parent && x.priority == 1).FirstOrDefault().Id;

                        collection_dto = collection_biz.ToDTO(new collection(), new List<dto_booth.Hit>(), "");
                        collection_dto = collection_biz.SetupToClient<dto_collection>(collection_dto);

                        booth b = booth_biz.GetBooth(booth_id, "", "", true, true, true, true, false, false, true);
                        dto_booth booth_dto = booth_biz.ToDTO(b, null);
                        collection_dto.booth_dto = booth_dto;

                        ViewBag.StatusCondition = collection_dto.status_condition_selectlist;
                        ViewBag.StatusStock = collection_dto.status_stock_selectlist;
                        ViewBag.CollectionCategoryMain = collection_dto.category_main_selectlist;
                        ViewBag.CollectionCategorySecond = collection_dto.category_second_selectlist;

                        ControllerHelper.JsonError(this);
                        ControllerHelper.JsonMessage(this);

                        return View("CreateCollection", collection_dto);
                    }
                    else//der er errors
                    {
                        biz_product product_biz = new biz_product(false);
                        model.booth_dto = DAL.GetInstance().GetBoothDTO(booth_id, "", "", true, false, false, true, false, false, true);
                    
                        model.product_dtos = product_biz.GetProductDTOsByCollectionId((int)model.id, false, false);

                        biz_product biz = new biz_product();
                        model = biz.SetupToClient<dto_collection>(model);
                        ViewBag.StatusCondition = model.status_condition_selectlist;
                        ViewBag.StatusStock = model.status_stock_selectlist;
                        ViewBag.CollectionCategoryMain = model.category_main_selectlist;
                        ViewBag.CollectionCategorySecond = model.category_second_selectlist;

                        ControllerHelper.JsonError(this);
                        ControllerHelper.JsonMessage(this);

                        return View("CreateCollection", model);
                    }
                }
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.CREATECOLLECTION, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("administration/opretsamling", Name = "CreateCollection2")]
        public ActionResult CreateCollection(dto_collection model)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                                
                dto_person salesman_poco = current_user;
                string status_condition_select = Request != null ? Request.Form["status_condition_select"].ToString() : "TEST_COLLECTION";
                string status_stock_select = "PÅ_LAGER";
                
                Dictionary<string, ERROR_MESSAGE> err = SetupHelper.SetupCollectionFromClient(ref model, model.booth_id, status_condition_select, status_stock_select, DAL.GetInstance());
                
                Dictionary<string, string> dirs = SetupHelper.SetupCollectionDirs(model, salesman_poco.sysname);

                if (CheckHelper.ErrorCollection.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if(err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetErrorMessageValue(err.ToList()[0].Key));//price
                    //if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageCollection(err.ToList()[1].Value));//category
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, TextHelper.GetErrorMessageValue(err.ToList()[1].Key));//name
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageCollection(err.ToList()[3].Value));//note
                    //if (err.ToList()[3].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[3].Key, Texts.GetErrorMessageCollection(err.ToList()[3].Value));//description

                    if (ThisSession.Json_Errors != null)
                        ThisSession.Json_Errors = errors;
                    TempData["datacontainer"] = model;
                    return RedirectToRoute("CreateCollection", new { booth_id = model.booth_dto.booth_id });
                }
                else
                {
                    model.sysname = dirs["collection_sysname"];
                    List<string> uploaded_fnames = PathHelper.GetFileNames(PATH.COLLECTION_DIRECTORY_TMP, dirs, false);
                    int collection_id = DAL.GetInstance().SaveCollection(model, uploaded_fnames.Where(f => f.Substring(0, 2) != "t_").ToList());
                    model.id = collection_id;//for test

                    string tmp_path = PathHelper.GetPath(PATH.COLLECTION_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = PathHelper.GetFileNames(PATH.COLLECTION_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true), file, true, false, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }                        
                    }

                    return RedirectToRoute("EditCollection1", new { collection_id = collection_id });
                }
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.CREATECOLLECTION, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("administration/redigersamling/{collection_id}", Name = "EditCollection1")]
        public ActionResult EditCollection(int collection_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);

                if (!CurrentUser.Inst().OwnsCollection(collection_id))
                    throw new Exception("A-OK, handled.");

                dto_collection collection_dto = DAL.GetInstance().GetCollectionDTO(collection_id, true, false, true, true, true);
                collection_dto.booth_dto = DAL.GetInstance().GetBoothDTO(collection_dto.booth_id, "", "", false, true, true, false, false, false, false);
                    
                List<dto_params> list = DAL.GetInstance().GetParams((int)collection_dto.category_main_id, (int)collection_dto.category_second_id);
                ViewBag.Params = list;
                ViewBag.StatusCondition = collection_dto.status_condition_selectlist;
                ViewBag.StatusStock = collection_dto.status_stock_selectlist;
                ViewBag.CollectionCategoryMain = collection_dto.category_main_selectlist;
                ViewBag.CollectionCategorySecond = collection_dto.category_second_selectlist;
                ViewBag.LevelA = collection_dto.foldera_selectlist;
                ViewBag.LevelB = collection_dto.folderb_selectlist;

                ControllerHelper.JsonError(this);
                ControllerHelper.JsonMessage(this);

                return View("EditCollection", collection_dto);

            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.EDITCOLLECTION, e) + ", GET";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }
        
        [HttpPost]
        [Route("administration/redigersamling/post", Name = "EditCollection2")]
        public ActionResult EditCollection(dto_collection model)
        {
            try 
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);

                dto_person salesman_poco = current_user;
                string status_condition_select = Request != null ? Request.Form["status_condition_select"].ToString() : "TEST_COLLECTION";
                string status_stock_select = "PÅ_LAGER";
                
                Dictionary<string, ERROR_MESSAGE> err = SetupHelper.SetupCollectionFromClient(ref model, model.booth_id, status_condition_select, status_stock_select, DAL.GetInstance(/*true*/));
                
                Dictionary<string, string> dirs = SetupHelper.SetupCollectionDirs(model, salesman_poco.sysname);
                
                if (CheckHelper.ErrorCollection.HasError(err))
                {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetErrorMessageValue(err.ToList()[0].Key));//price
                    //if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                    //    errors.Add(err.ToList()[1].Key, Texts.GetErrorMessageCollection(err.ToList()[1].Value));//category
                    if (err.ToList()[1].Value != ERROR_MESSAGE.OK)
                        errors.Add(err.ToList()[1].Key, TextHelper.GetErrorMessageValue(err.ToList()[1].Key));//name
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
                    List<string> uploaded_fnames = PathHelper.GetFileNames(PATH.COLLECTION_DIRECTORY_TMP, dirs, false);
                    DAL.GetInstance().UpdateCollection(model, uploaded_fnames.Where(f => f.Substring(0, 2) != "t_" && f.Substring(0, 2) != "s_").ToList());

                    string tmp_path = PathHelper.GetPath(PATH.COLLECTION_DIRECTORY_TMP, dirs, true);
                    List<string> uploaded_files = PathHelper.GetFileNames(PATH.COLLECTION_DIRECTORY_TMP, dirs, false);
                    if (uploaded_files != null)
                    {
                        if (uploaded_files.Count() > 0)
                        {
                            string file = uploaded_files[0];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true), file, true, false, false, false);
                            file = uploaded_files[1];
                            if (file != null)
                                PathHelper.MoveFile(tmp_path, file, PathHelper.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true), file, true, false, true, false);
                        }
                        //Paths.MoveFile("t_" + file, Paths.GetPath(PATH.PROFILE_DIRECTORY_NAME, dirs, true));
                    }

                    return RedirectToRoute("EditCollection1", new { collection_id = model.id });
                }
                
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.EDITCOLLECTION, e) + ", POST";
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]///
        [Route("deletecollection", Name = "DeleteCollection")]
        public ActionResult DeleteCollection(int collection_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);

                if (!CurrentUser.Inst().OwnsCollection(collection_id))
                    throw new Exception("A-OK, handled.");

                dto_collection collection_dto = DAL.GetInstance().GetCollectionDTO(collection_id, false, true, false, false, false);
                
                Dictionary<string, string> dirs = new Dictionary<string, string>();
                dirs["identity_id"] = current_user.sysname;
                dirs["booth_sysname"] = collection_dto.booth_dto.sysname;
                dirs["collection_sysname"] = collection_dto.sysname;
                string path = PathHelper.GetPath(PATH.COLLECTION_DIRECTORY_NAME, dirs, true);
                PathHelper.ClearFolder(path, true, true);

                DAL.GetInstance().DeleteCollection(collection_id);

                return RedirectToAction("EditBooth", new { booth_id = collection_dto.booth_dto.booth_id });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.DELETECOLLECTION, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("Administration/AddProductToCollection", Name = "AddProductToCollection")]
        public ActionResult AddProductToCollection(int collection_id, long product_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                DAL.GetInstance().AddProductToCollection(collection_id, product_id);

                return RedirectToRoute("EditCollection1", new { collection_id = collection_id });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.ADDPRODUCTTOCOLLECTION, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("Administration/RemoveProductFromCollection", Name = "RemoveProductFromCollection")]
        public ActionResult RemoveProductFromCollection(int collection_id, long product_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                DAL.GetInstance().RemoveProductFromCollection(collection_id, product_id);

                return RedirectToRoute("EditCollection1", new { collection_id = collection_id });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVEPRODUCTFROMCOLLECTION, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }        

        [HttpPost]
        [Route("Administration/UploadImage", Name = "UploadImage")]
        public void UploadImage()
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

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
                string err_msg = ErrorHelper.HandleError(ERROR.UPLOADIMAGE, e);
                TempData["err_msg"] = err_msg;
                AjaxError("Der skete en fejl!");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        public void UploadImage(HttpPostedFile file, TYPE type)
        {
            ControllerHelper.Setup<AdministrationController>(this, SETUP.FFF, true, false);
            
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs["identity_id"] = current_user.sysname;

            string ext = file.FileName.Substring(file.FileName.LastIndexOf('.'));
            string name = PathHelper.GenerateFileName(file.FileName.Substring(0, file.FileName.IndexOf('.')), FILE_NAME.NONE);
            if (/*ext.ToLower().Contains("gif") || */ext.ToLower().Contains("jpg") || ext.ToLower().Contains("jpeg") || ext.ToLower().Contains("png"))
            {
                using (Stream inputStream = file.InputStream)
                {
                    PATH path_tmp = ImageHelper.GetPath(type);
                    string path = PathHelper.GetPath(path_tmp, dirs, true);
                    PathHelper.CreatePath(path);
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
        [Route("Administration/RemoveImage", Name = "RemoveImage")]
        public JsonResult RemoveImage(string ImageName, string BoothId, string ItemId, string type)
        {

            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                               
                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                dto_person person_dto = current_user;
                dto_booth booth_dto = null;
                PATH path_tmp;
                Dictionary<string, string> dirs = new Dictionary<string, string>();
                dirs["identity_id"] = person_dto.sysname;
                
                if (typeEnum == TYPE.PROFILE)
                {
                    biz_person biz = new biz_salesman();
                    path_tmp = PATH.PROFILE_DIRECTORY_NAME;

                    if (person_dto != null)
                    {
                        if (person_dto.nator == "Salesman")
                            biz.RemoveImage<dto_salesman>((dto_salesman)person_dto);
                        else
                            biz.RemoveImage<dto_customer>((dto_customer)person_dto);
                    }
                    else
                        throw new Exception("A-OK, handled.");
                }
                else
                {
                    booth_dto = new dto_booth();
                    booth_dto = DAL.GetInstance().GetBoothDTO(int.Parse(BoothId), "", "", true, true, true, true, false, false, true);
                    dirs["booth_sysname"] = booth_dto.sysname;
                    
                    if (typeEnum == TYPE.BOOTH)
                    {
                        biz_booth biz = new biz_booth();
                        path_tmp = PATH.BOOTH_DIRECTORY_NAME;
                        
                        biz.RemoveImage(booth_dto);
                    }
                    else
                    {
                        if (typeEnum == TYPE.PRODUCT)
                        {
                            biz_product biz = new biz_product();
                            path_tmp = PATH.PRODUCT_DIRECTORY_NAME;
                            dto_product item_poco = booth_dto.product_dtos.Where(p => p.id == long.Parse(ItemId)).FirstOrDefault();
                            if (item_poco != null)
                                biz.RemoveImage<dto_product>(ImageName, item_poco);
                            dirs["product_sysname"] = item_poco.sysname;
                        }
                        else
                        {
                            biz_collection biz = new biz_collection();
                            path_tmp = PATH.COLLECTION_DIRECTORY_NAME;
                            dto_collection item_poco = booth_dto.collection_dtos.Where(c => c.id == long.Parse(ItemId)).FirstOrDefault();
                            if (item_poco != null)
                                biz.RemoveImage<dto_collection>(ImageName, item_poco);
                            dirs["collection_sysname"] = item_poco.sysname;
                        }                        
                    }
                }
                
                string path = PathHelper.GetPath(path_tmp, dirs, true);

                bool clearpath = typeEnum == TYPE.PROFILE;// || typeEnum == TYPE.COLLECTION || typeEnum == TYPE.PRODUCT;
                PathHelper.DeleteFile(path, ImageName, false);
                PathHelper.DeleteFile(path, "t_" + ImageName, clearpath);

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVEPRODUCTIMAGE, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("Der skete en fejl!");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }
                
        [HttpPost]
        [Route("Administration/_GetTags", Name = "GetTags")]
        public JsonResult GetTags(string TagName)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                bool ok;
                TagName = StringHelper.OnlyAlphanumeric(TagName.ToLower().Trim(), false, false, "notag", CharacterHelper.Space(), out ok);
                List<dto_tag> tag_dtos = DAL.GetInstance().Get5TagDTOs(TagName);

                if (tag_dtos != null)
                    return Json(new { success = true, tags = tag_dtos });
                else
                    return Json(new { success = false });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.GETTAGS, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }
                
        [HttpPost]
        [Route("Administration/_SaveTag", Name = "SaveTag")]
        public JsonResult SaveTag(string tag_name, string id, string type)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

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
                    MESSAGE_TAG result = DAL.GetInstance().SaveTag(tag_name, id, typeEnum);

                    if (result == MESSAGE_TAG.OK)
                        tag_id = DAL.GetInstance().GetTag(tag_name).Id;
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
                string err_msg = ErrorHelper.HandleError(ERROR.SAVETAG, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }
                
        [HttpPost]
        [Route("Administration/_RemoveTag", Name = "RemoveTag")]
        public JsonResult RemoveTag(string TagId, string Id, string type)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                long tag_id = 0;
                string msg = "";
                bool ok;
                ok = DAL.GetInstance().RemoveTag(long.Parse(TagId), typeEnum, Id, false);

                if (ok)
                    tag_id = long.Parse(TagId);
                else
                    msg = "err_msg";
                return Json(new { success = ok, tag_id = tag_id, msg = msg });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVETAG, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/_SaveParam", Name = "SaveParam")]
        public JsonResult _SaveParam(long id, int param_id, int val_id, string type)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                                
                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                bool result = DAL.GetInstance().SaveParam(id, param_id, val_id, typeEnum);

                string msg = "ok";
                bool success = true;

                if (!result)
                {
                    msg = "Der skete en fejl, beklager.";
                    success = false;
                }

                return Json(new { msg = msg, success = success });
            }
            catch (Exception e)
            {
                TempData["err_msg"] = ErrorHelper.HandleError(ERROR.SAVEPARAM, e);
                TempData["ErrorMessage"] = "";
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/_RemoveParam", Name = "RemoveParam")]
        public JsonResult _RemoveParam(int param_id, long id, string type)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                               
                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                string msg = "";
                bool ok;
                ok = DAL.GetInstance().RemoveParam(param_id, typeEnum, id);

                if (!ok)
                    msg = "err_msg";
                return Json(new { success = ok, msg = msg });
            }
            catch (Exception e)
            {
                TempData["err_msg"] = ErrorHelper.HandleError(ERROR.REMOVEPARAM, e);
                TempData["ErrorMessage"] = "";
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/AddCategory", Name = "AddCategory")]
        public JsonResult AddCategory(int CatId, int BoothId)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                long cat_id = 0;
                string msg = "";
                string cat_name = DAL.GetInstance().AddCategory(CatId, BoothId);

                if (!string.IsNullOrEmpty(cat_name))
                    cat_id = CatId;
                else
                    msg = "err_msg";
                
                return Json(new { cat_id = cat_id, cat_name = cat_name, msg = msg, success = !string.IsNullOrEmpty(cat_name) });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.SAVETAG, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/RemoveCategory", Name = "RemoveCategory")]
        public JsonResult RemoveCategory(int CatId, int BoothId)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                long cat_id = 0;
                string msg = "";
                bool ok = DAL.GetInstance().RemoveCategory(CatId, BoothId);

                if (ok)
                    cat_id = CatId;
                else
                    msg = "err_msg";
                return Json(new { success = ok, cat_id = cat_id, msg = msg });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVETAG, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/DeleteConversation", Name = "DeleteConversation")]
        public JsonResult DeleteConversation(long conversation_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                DAL.GetInstance().DeleteConversation(conversation_id);
                return Json( new { success = true });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.DELETECONVERSATION, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]///
        [Route("Administration/RemoveFavorite", Name = "RemoveFavorite2")]
        public ActionResult RemoveFavorite(long product_id, int collection_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                DAL.GetInstance().RemoveFavorite(product_id, collection_id);
                return RedirectToAction("Userprofile", new { coming_from = "RemoveFavorite" });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVEFAVORITE, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]///
        [Route("Administration/RemoveFollowing", Name = "RemoveFollowing2")]
        public ActionResult RemoveFollowing(int booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                DAL.GetInstance().RemoveFollowing(booth_id);
                return RedirectToRoute("Userprofile");
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVEFAVORITE, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("Administration/CreateFolder", Name = "CreateFolder")]
        public ActionResult CreateFolder(string fld_name, string id, string type, string booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                ThisSession.Tab = "categorys";

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                bool ok;
                fld_name = StringHelper.OnlyAlphanumeric(fld_name, false, true, "notag", CharacterHelper.Limited(false), out ok);
                if (ok)
                    DAL.GetInstance().CreateFolder(fld_name, int.Parse(id), typeEnum);
                else
                {
                    Dictionary<string, SYSTEM_MESSAGE> err = SetupHelper.SetupCreateLevel(true);
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    if (err.ToList()[0].Value != SYSTEM_MESSAGE.NO_MESSAGE)
                        errors.Add(err.ToList()[0].Key, TextHelper.GetSystemMessageValue(err.ToList()[0].Key.ToString()));//price
                    if (ThisSession.Json_Messages != null)
                        ThisSession.Json_Messages = errors;
                }
                return RedirectToRoute("EditBooth1", new { booth_id = int.Parse(booth_id) });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.CREATELEVEL, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("Administration/MoveFolder", Name = "MoveFolder")]
        public ActionResult MoveFolder(string fld_id, string direction, string id, string type, string booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                DAL.GetInstance().MoveFolder(int.Parse(fld_id), direction, int.Parse(id), typeEnum);
                ThisSession.Tab = "categorys";
                return RedirectToRoute("EditBooth1", new { booth_id = int.Parse(booth_id) });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.MOVELEVEL, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("Administration/DeleteFolder", Name = "DeleteFolder")]
        public ActionResult DeleteFolder(string fld_id, string id, string type, string booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                DAL.GetInstance().DeleteFolder(int.Parse(fld_id), int.Parse(id), typeEnum);
                ThisSession.Tab = "categorys";
                return RedirectToRoute("EditBooth1", new { booth_id = int.Parse(booth_id) });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.DELETELEVEL, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpGet]
        [Route("Administration/SetFolder", Name = "SetFolder")]
        public ActionResult SetFolder(string fld_id, string id, string type, string is_product)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                TYPE typeEnum;
                Enum.TryParse(type, out typeEnum);

                DAL.GetInstance().SetFolder(int.Parse(fld_id), id, typeEnum, is_product == "true");
                ThisSession.Tab = "categorys";
                if (is_product == "true")
                    return RedirectToRoute("EditProduct1", new { product_id = long.Parse(id) });
                else
                    return RedirectToRoute("EditCollection1", new { collection_id = int.Parse(id) });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.SETLEVEL, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Administration/GetEmail", Name = "GetEmail")]
        public JsonResult GetEmail(string booth_id)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                int _id;
                if(int.TryParse(booth_id, out _id))
                {
                    dto_booth booth_dto = DAL.GetInstance().GetBoothDTO(_id, "", "", true, false, false, false, false, false, true);
                
                    if(booth_dto.salesman_dto.request_email)
                        return Json(new { success = true, ok = true, value = booth_dto.salesman_dto.email });
                    return Json(new { success = true, ok = false, value = "" });
                }
                return Json(new { success = false });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.GETADDRESSEMAIL, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/GetAddressTown", Name = "GetAddressTown")]
        public JsonResult GetAddressTown(string Zip)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                string town = DAL.GetInstance().GetAddressTown(Zip);
                
                return Json(new { success=true, town=town });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.GETADDRESSTOWN, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/Maintenance", Name = "Maintenance")]
        public JsonResult Maintenance(bool run)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                if (run)
                    StaticsHelper.Maintenance = !StaticsHelper.Maintenance;
                return Json(new { success = StaticsHelper.Maintenance });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVETAG, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/SetActive", Name = "SetActive")]
        public JsonResult SetActive(int Id, string Value, string Type)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                bool value = Value.ToLower() == "true" ? true : false;
                string res = DAL.GetInstance().SetActive(Id, value, Type);
                string msg = res == "true" ? "[aktiv]" : "[ikke aktiv]";
                return Json(new { success = res == "err" ? false : true, msg = msg});
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVETAG, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/ChangeBoothId", Name = "ChangeBoothId")]
        public JsonResult ChangeBoothId(int BoothId, long ProductId)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.XXXX, false, false);

                string a = "";
                string b = a;

                if (DAL.GetInstance().ChangeBoothId(BoothId, ProductId))
                    return Json(new { success = true });
                else
                    return Json(new { success = false, msg = "Varen er del af Sæt." });
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVETAG, e);
                TempData["err_msg"] = err_msg;
                return AjaxErrorReturn("err");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        [HttpPost]
        [Route("Administration/Feedback", Name = "Feedback")]
        public ActionResult Feedback(col_email mail)
        {
            try
            {
                ControllerHelper.Setup<AdministrationController>(this, SETUP.FTT, true, false);
                               
                Dictionary<string, SYSTEM_MESSAGE> err = SetupHelper.Feedback(mail);
                Dictionary<string, string> errors = new Dictionary<string, string>();
                if (err.ToList()[0].Value != SYSTEM_MESSAGE.NO_MESSAGE)
                    errors.Add(err.ToList()[0].Key, TextHelper.GetSystemMessageValue(err.ToList()[0].Key.ToString()));//price
                else
                {
                    errors.Add(err.ToList()[0].Key, "Besked sendt, hold øje med spam folder for svar.");//price
                    
                    string subject = "Feedback: " + mail.Subject;
                    string body = mail.Message.Replace("\r\n", "<br />");
                    AdminHelper.Notification.Run(current_user.email, SettingsHelper.Basic.EMAIL_ADMIN(), SettingsHelper.Basic.EMAIL_ADMIN(), subject, body);
                }

                
                if (HttpContext != null)
                    ThisSession.Json_Messages = errors;

                return RedirectToRoute("UserProfile");
            }
            catch (Exception e)
            {
                string err_msg = ErrorHelper.HandleError(ERROR.REMOVETAG, e);
                TempData["err_msg"] = err_msg;
                return RedirectToRoute("ErrorPage2");
            }
            finally
            {
                ControllerHelper.Finally(this);
            }
        }

        //[HttpGet]
        [Route("administration/fejlside", Name = "ErrorPage2")]
        public ActionResult ErrorPage()
        {
            string err_msg = TempData["err_msg"] as string;
            ViewBag.ErrorMessage = TempData["ErrorMessage"] as string;

            string subject = "Der er sket en fejl!";
            string body = "ViewBag: " + ViewBag.ErrorMessage + "<br />" +
                      "MSG: " + /*Extensions.Extensions.HtmlEncode(*/err_msg/*)*/;
            AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), subject, body);
            
            return View("ErrorPage");
        }

        //[AllowAnonymous]
        //[Route("administration/not_found", Name = "NotFound2")]
        //public ActionResult NotFound()
        //{
        //    //if (ThisSession.IsMobile == "none")
        //    //    return View("IsMobile");

        //    string subject = "NotFound!";
        //    string body = Extensions.HtmlEncode("Not Found.");
        //    AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), subject, body);

        //    return HttpNotFound(HttpStatusCode.NotFound.ToString());
        //}

        //[Route("Administration/AjaxError", Name = "AjaxError")]
        private void AjaxError(string user_msg)
        {
            string err_msg = TempData["err_msg"] as string;

            string subject = "Der er sket en fejl!";
            string body = Extensions.HtmlEncode(err_msg);
            AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), subject, body);

            System.Web.HttpContext.Current.Response.Write(user_msg);//bliver nok ikke fanget på klienten
        }

        //[Route("Administration/AjaxErrorReturn", Name = "AjaxErrorReturn2")]
        private JsonResult AjaxErrorReturn(string user_msg)
        {
            string err_msg = TempData["err_msg"] as string;

            string subject = "Der er sket en fejl!";
            string body = Extensions.HtmlEncode(err_msg);
            AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), subject, body);

            return Json(new { success = false, res = user_msg });
        }        
    }
}