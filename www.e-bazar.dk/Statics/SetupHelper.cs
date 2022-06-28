using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Statics
{
    public class SetupHelper
    {
        public static Dictionary<string, string> SetupProfileDirs(dto_person model)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            //dirs["identity_id"] = identity_id;
            dirs["identity_id"] = string.IsNullOrEmpty(model.sysname) ? PathHelper.GenerateFolderName(PATH.BOOTH_DIRECTORY, dirs, "PROFILE") : model.sysname;

            return dirs;
        }

        public static Dictionary<string, ERROR_MESSAGE> SetupSalesmanProfileFromClient(ref col_userprofile model)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();
            model.salesman_dto.firstname = CheckHelper.ProfileFirstname(model.salesman_dto.firstname, ref err);
            model.salesman_dto.lastname = CheckHelper.ProfileLastname(model.salesman_dto.lastname, ref err);
            model.salesman_dto.phonenumber = CheckHelper.ProfilePhonenumber(model.salesman_dto.phonenumber, ref err);
            model.salesman_dto.email = CheckHelper.ProfileEmail(model.salesman_dto.email, ref err);
            model.salesman_dto.description = CheckHelper.ProfileDescription(model.salesman_dto.description);

            return err;
        }

        public static Dictionary<string, ERROR_MESSAGE> SetupCustomerProfileFromClient(ref col_userprofile model)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();
            model.customer_dto.firstname = CheckHelper.ProfileFirstname(model.customer_dto.firstname, ref err);
            model.customer_dto.lastname = CheckHelper.ProfileLastname(model.customer_dto.lastname, ref err);
            //model.customer_poco.phonenumber = Check.ProfilePhonenumber(model.customer_poco.phonenumber, ref err);
            model.customer_dto.email = CheckHelper.ProfileEmail(model.customer_dto.email, ref err);
            return err;
        }

        public static Dictionary<string, string> SetupBoothDirs(ref dto_booth model, string identity_id)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs["identity_id"] = identity_id;
            dirs["booth_sysname"] = string.IsNullOrEmpty(model.sysname) ? PathHelper.GenerateFolderName(PATH.BOOTH_DIRECTORY, dirs, "BOOTH") : model.sysname;

            return dirs;
        }

        public static Dictionary<string, ERROR_MESSAGE> SetupBoothFromClient(ref dto_booth model, string userid, DAL dal)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();

            if (model.salesman_dto == null)
                model.salesman_dto = (dto_salesman)dal.GetPersonDTO<dto_salesman>(userid, false, true, true);
            if (model.salesman_dto == null)
            {
                model = null;
                return err;
            }

            if (string.IsNullOrEmpty(model.salesman_id))
                model.salesman_id = userid;
            
            if (model.product_dtos == null)
                model.product_dtos = dal.GetProductDTOs(model.booth_id, true, false, false, false, false, false, true);
            
            if (model.collection_dtos == null)
                model.collection_dtos = dal.GetCollectionDTOs(model.booth_id, true, false, false, false);
            
            model.name = CheckHelper.BoothName(model.name, ref err);
            model.description = CheckHelper.BoothDescription(model.description, ref err);
            //Check.FullAddress(model.address_poco, ref err);
            CheckHelper.PartAddress(model, ref err);
            CheckHelper.FullAddress(model, ref err);
            //Check.Tags(model, ref err);

            return err;
        }

        public static Dictionary<string, string> SetupProductDirs(dto_product model, string identity_id)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs["identity_id"] = identity_id;
            dirs["booth_sysname"] = model.booth_dto.sysname;
            dirs["product_sysname"] = string.IsNullOrEmpty(model.sysname) ? PathHelper.GenerateFolderName(PATH.PRODUCT_DIRECTORY, dirs, "PRODUCT"/*, FILE_NAME.NONE*/) : model.sysname;
            return dirs;
        }

        public static Dictionary<string, ERROR_MESSAGE> SetupProductFromClient(ref dto_product model, int? booth_id, string status_condition_select, string status_stock_select, DAL dal)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();
            model.booth_dto = dal.GetBoothDTO(booth_id, "", "", true, false, false, true, false, false, true);
            
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
            
            model.price = CheckHelper.ProductPrice(model.price, ref err);
            model.name = CheckHelper.ProductName(model.name, ref err);
            //model.note = Check.ProductNote(model.note, ref err);
            //model.description = Check.ProductDescription(model.description, ref err);
            model.no_of_units = CheckHelper.ProductNoOfUnits("" + model.no_of_units, ref err);
            return err;
        }

        public static Dictionary<string, string> SetupCollectionDirs(dto_collection model, string identity_id)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs["identity_id"] = identity_id;
            dirs["booth_sysname"] = model.booth_dto.sysname;
            dirs["collection_sysname"] = string.IsNullOrEmpty(model.sysname) ? PathHelper.GenerateFolderName(PATH.COLLECTION_DIRECTORY, dirs, "COLL"/*, FILE_NAME.NONE*/) : model.sysname;
            return dirs;
        }

        public static Dictionary<string, ERROR_MESSAGE> SetupCollectionFromClient(ref dto_collection model, int? booth_id, string status_condition_select, string status_stock_select, DAL dal)
        {
            Dictionary<string, ERROR_MESSAGE> err = new Dictionary<string, ERROR_MESSAGE>();
            model.booth_dto = dal.GetBoothDTO(booth_id, "", "", true, false, false, true, false, false, true)/* : model.booth_poco*/;
            
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

            model.price = CheckHelper.CollectionPrice(model.price, ref err);
            //model.category = Check.CollectionCategory(model.category, ref err);
            model.name = CheckHelper.CollectionName(model.name, ref err);
            //model.note = Check.CollectionNote(model.note, ref err);
            //model.description = Check.CollectionDescription(model.description, ref err);
            return err;
        }

        public static Dictionary<string, SYSTEM_MESSAGE> SetupRegisterFromClient(bool success)
        {
            Dictionary<string, SYSTEM_MESSAGE> err = new Dictionary<string, SYSTEM_MESSAGE>();
            CheckHelper.Register(success, ref err);
            return err;
        }

        public static Dictionary<string, SYSTEM_MESSAGE> CheckCookieLogin(bool success)
        {
            Dictionary<string, SYSTEM_MESSAGE> err = new Dictionary<string, SYSTEM_MESSAGE>();
            CheckHelper.CookieLogin(success, ref err);
            return err;
        }

        public static Dictionary<string, SYSTEM_MESSAGE> SetupCreateLevel(bool success)
        {
            Dictionary<string, SYSTEM_MESSAGE> err = new Dictionary<string, SYSTEM_MESSAGE>();
            CheckHelper.CreateLevel(success, ref err);
            return err;
        }

        public static Dictionary<string, SYSTEM_MESSAGE> Feedback(col_email mail)
        {
            Dictionary<string, SYSTEM_MESSAGE> err = new Dictionary<string, SYSTEM_MESSAGE>();
            CheckHelper.CheckFeedback(mail, ref err);
            return err;
        }
    }
}