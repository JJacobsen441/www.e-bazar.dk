using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;

namespace www.e_bazar.dk.SharedClasses
{
    public class Setup
    {
        public static Dictionary<string, string> SetupProfileDirs(poco_person model)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            //dirs["identity_id"] = identity_id;
            dirs["identity_id"] = string.IsNullOrEmpty(model.sysname) ? Paths.GenerateFolderName(PATH.BOOTH_DIRECTORY, dirs, "PROFILE") : model.sysname;

            return dirs;
        }
        public static Dictionary<string, ERROR_MESSAGE> SetupSalesmanProfileFromClient(ref dto_userprofile model)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();
            model.salesman_poco.firstname = Check.ProfileFirstname(model.salesman_poco.firstname, ref err);
            model.salesman_poco.lastname = Check.ProfileLastname(model.salesman_poco.lastname, ref err);
            model.salesman_poco.phonenumber = Check.ProfilePhonenumber(model.salesman_poco.phonenumber, ref err);
            model.salesman_poco.email = Check.ProfileEmail(model.salesman_poco.email, ref err);
            model.salesman_poco.description = Check.ProfileDescription(model.salesman_poco.description);

            return err;
        }
        public static Dictionary<string, ERROR_MESSAGE> SetupCustomerProfileFromClient(ref dto_userprofile model)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();
            model.customer_poco.firstname = Check.ProfileFirstname(model.customer_poco.firstname, ref err);
            model.customer_poco.lastname = Check.ProfileLastname(model.customer_poco.lastname, ref err);
            //model.customer_poco.phonenumber = Check.ProfilePhonenumber(model.customer_poco.phonenumber, ref err);
            model.customer_poco.email = Check.ProfileEmail(model.customer_poco.email, ref err);
            return err;
        }

        public static Dictionary<string, string> SetupBoothDirs(ref poco_booth model, string identity_id)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs["identity_id"] = identity_id;
            dirs["booth_sysname"] = string.IsNullOrEmpty(model.sysname) ? Paths.GenerateFolderName(PATH.BOOTH_DIRECTORY, dirs, "BOOTH") : model.sysname;

            return dirs;
        }
        public static Dictionary<string, ERROR_MESSAGE> SetupBoothFromClient(ref poco_booth model, string userid, DAL dal)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();

            if (model.salesman_poco == null)
                model.salesman_poco = (poco_salesman)dal.GetPersonPOCO<poco_salesman>(userid, false, true, true);
            if (model.salesman_poco == null)
            {
                model = null;
                return err;
            }
            if (string.IsNullOrEmpty(model.salesman_id))
                model.salesman_id = userid;
            
            if (model.product_pocos == null)
                model.product_pocos = dal.GetProductPOCOs(model.booth_id, true, false, false, false, false, false, true);
            //if (model.product_pocos == null)
            //{
            //    model = null;
            //    return err;
            //}

            if (model.collection_pocos == null)
                model.collection_pocos = dal.GetCollectionPOCOs(model.booth_id, true, false, false, false);
            //if (model.collection_pocos == null)
            //{
            //    model = null;
            //    return err;
            //}


            model.name = Check.BoothName(model.name, ref err);
            model.description = Check.BoothDescription(model.description, ref err);
            //Check.FullAddress(model.address_poco, ref err);
            Check.PartAddress(model, ref err);
            Check.FullAddress(model, ref err);
            //Check.Tags(model, ref err);

            return err;
        }

        public static Dictionary<string, string> SetupProductDirs(poco_product model, string identity_id)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs["identity_id"] = identity_id;
            dirs["booth_sysname"] = model.booth_poco.sysname;
            dirs["product_sysname"] = string.IsNullOrEmpty(model.sysname) ? Paths.GenerateFolderName(PATH.PRODUCT_DIRECTORY, dirs, "PRODUCT"/*, FILE_NAME.NONE*/) : model.sysname;
            return dirs;
        }
        public static Dictionary<string, ERROR_MESSAGE> SetupProductFromClient(ref poco_product model, int? booth_id, string status_condition_select, string status_stock_select, DAL dal)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();
            model.booth_poco = dal.GetBoothPOCO(booth_id, "", "", true, false, false, true, false, false, true);
            
            model.status_condition = status_condition_select;
            model.status_stock = status_stock_select;
            using (EbazarDB db = new EbazarDB())
            {
                category main;
                category sec;

                if (model.category_main_id == 0)
                {
                    main = db.category.Where(x => x.name == ".ingen").FirstOrDefault();
                    sec = db.category.Where(x => x.name == "..ingen").FirstOrDefault();
                }
                else
                {
                    int m = model.category_main_id;
                    int s = model.category_second_id;

                    main = db.category.Where(x => x.Id == m).FirstOrDefault();
                    if(s == 0)
                        sec = main.children.OrderBy(x => x.priority).FirstOrDefault();
                    else
                        sec = db.category.Where(x => x.Id == s).FirstOrDefault();
                }
                if (main == null)
                    throw new Exception("A-OK, Handled.");
                if (sec == null)
                    throw new Exception("A-OK, Handled.");
                model.active = main.name == ".ingen" || sec.name == "..ingen" ? false : model.active;
                model.category_main_id = main.Id;
                model.category_second_id = sec.Id;
            }
            
            model.price = Check.ProductPrice(model.price, ref err);
            model.name = Check.ProductName(model.name, ref err);
            //model.note = Check.ProductNote(model.note, ref err);
            //model.description = Check.ProductDescription(model.description, ref err);
            model.no_of_units = Check.ProductNoOfUnits("" + model.no_of_units, ref err);
            return err;
        }

        public static Dictionary<string, string> SetupCollectionDirs(poco_collection model, string identity_id)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs["identity_id"] = identity_id;
            dirs["booth_sysname"] = model.booth_poco.sysname;
            dirs["collection_sysname"] = string.IsNullOrEmpty(model.sysname) ? Paths.GenerateFolderName(PATH.COLLECTION_DIRECTORY, dirs, "COLL"/*, FILE_NAME.NONE*/) : model.sysname;
            return dirs;
        }

        public static Dictionary<string, ERROR_MESSAGE> SetupCollectionFromClient(ref poco_collection model, int? booth_id, string status_condition_select, string status_stock_select, DAL dal)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();
            model.booth_poco = /*model.booth_poco == null ? */dal.GetBoothPOCO(booth_id, "", "", true, false, false, true, false, false, true)/* : model.booth_poco*/;
            
            model.status_condition = status_condition_select;
            model.status_stock = status_stock_select;

            using (EbazarDB db = new EbazarDB())
            {
                category main;
                category sec;

                if (model.category_main_id == 0)
                {
                    main = db.category.Where(x => x.name == ".ingen").FirstOrDefault();
                    sec = db.category.Where(x => x.name == "..ingen").FirstOrDefault();
                }
                else
                {
                    int m = model.category_main_id;
                    int s = model.category_second_id;

                    main = db.category.Where(x => x.Id == m).FirstOrDefault();
                    if (s == 0)
                        sec = main.children.OrderBy(x => x.priority).FirstOrDefault();
                    else
                        sec = db.category.Where(x => x.Id == s).FirstOrDefault();
                }
                if (main == null)
                    throw new Exception("A-OK, Handled.");
                if (sec == null)
                    throw new Exception("A-OK, Handled.");
                model.active = main.name == ".ingen" || sec.name == "..ingen" ? false : model.active;
                model.category_main_id = main.Id;
                model.category_second_id = sec.Id;
            }

            model.price = Check.CollectionPrice(model.price, ref err);
            //model.category = Check.CollectionCategory(model.category, ref err);
            model.name = Check.CollectionName(model.name, ref err);
            //model.note = Check.CollectionNote(model.note, ref err);
            //model.description = Check.CollectionDescription(model.description, ref err);
            return err;
        }
        public static Dictionary<string, SYSTEM_MESSAGE> SetupRegisterFromClient(bool success)
        {
            Dictionary<string, SYSTEM_MESSAGE> err = new Dictionary<string, SYSTEM_MESSAGE>();
            Check.Register(success, ref err);
            return err;
        }
        public static Dictionary<string, SYSTEM_MESSAGE> CheckCookieLogin(bool success)
        {
            Dictionary<string, SYSTEM_MESSAGE> err = new Dictionary<string, SYSTEM_MESSAGE>();
            Check.CookieLogin(success, ref err);
            return err;
        }
        public static Dictionary<string, SYSTEM_MESSAGE> SetupCreateLevel(bool success)
        {
            Dictionary<string, SYSTEM_MESSAGE> err = new Dictionary<string, SYSTEM_MESSAGE>();
            Check.CreateLevel(success, ref err);
            return err;
        }
        public static Dictionary<string, SYSTEM_MESSAGE> Feedback(dto_email mail)
        {
            Dictionary<string, SYSTEM_MESSAGE> err = new Dictionary<string, SYSTEM_MESSAGE>();
            Check.CheckFeedback(mail, ref err);
            return err;
        }

    }
}