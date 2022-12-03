using Microsoft.AspNet.Identity;
using System;
using System.Web.Mvc;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DTOs;

namespace www.e_bazar.dk.SharedClasses
{
    public class CurrentUser
    {
        private static CurrentUser user;
        public string CurrentUserID { get { return ThisSession.CurrentUserId; } set { ThisSession.CurrentUserId = value; } }
        public string CurrentUserName { get { return ThisSession.CurrentUserName; } set { ThisSession.CurrentUserName = value; } }
        public string CurrentType { get { return ThisSession.CurrentType; } set { ThisSession.CurrentType = value; } }
        public bool CurrentIsAuthenticated { get { return ThisSession.CurrentIsAuthenticated; } set { ThisSession.CurrentIsAuthenticated = value; } }

        private CurrentUser()
        {
        }

        public static CurrentUser Inst()
        {
            if (user == null)
                user = new CurrentUser();
            return user;
        }

        public void Setup(Controller con)
        {
            string current_id = "";
            string current_name = "";
            bool current_auth = false;
            string current_type = "none";

            if (con.User != null)
            {
                current_id = con.User.Identity.IsAuthenticated ? con.User.Identity.GetUserId() : "";
                current_name = con.User.Identity.IsAuthenticated ? con.User.Identity.GetUserName() : "";
                current_auth = con.User.Identity.IsAuthenticated;
                current_type =
                    con.User.IsInRole("Administrator") ? "Administrator" :
                    con.User.IsInRole("Salesman") ? "Salesman" :
                    con.User.IsInRole("Customer") ? "Customer" : "none";
            }

            CurrentUser.Inst().Login(current_id, current_name, current_auth, current_type);
        }

        public dto_person GetDTO(SETUP seq)
        {
            if (CurrentUserID == "")
                return null;

            biz_salesman salesman_biz = new biz_salesman();
            biz_customer customer_biz = new biz_customer();
            dto_person person_dto = null;

            switch (seq) 
            {
                case SETUP.FTT:
                    person_dto = salesman_biz.GetPersonDTO<dto_salesman>(CurrentUserID, false, true, true);
                    break;
                case SETUP.FFF:
                    person_dto = salesman_biz.GetPersonDTO<dto_salesman>(CurrentUserID, false, false, false);
                    break;
                default:
                    break;
            }

            if (person_dto.nator != CurrentType)
                throw new Exception("A-OK, Handled.");

            return person_dto;            
        }
        
        public void Login(string ID, string Name, bool Authenticated, string Type)
        {
            CurrentUserID = ID;
            CurrentUserName = Name;
            CurrentIsAuthenticated = Authenticated;
            CurrentType = Type;
        }
        
        public bool OwnsBooth(int booth_id)
        {
            biz_booth booth_poco = new biz_booth();
            booth booth = booth_poco.GetBooth(booth_id, "", "", true, false, false, true, false, false, true);
            if (booth != null)
            {
                if (booth.person.Id == user.CurrentUserID)
                    return true;
            }

            return false;
        }

        public bool OwnsProduct(long product_id)
        {
            biz_product product_poco = new biz_product(false);
            product product = product_poco.GetProduct(product_id, true, false, false, false);
            if (product != null)
            {
                if (product.booth.person.Id == user.CurrentUserID)
                    return true;
            }

            return false;
        }

        public bool OwnsCollection(int collection_id)
        {
            biz_collection collection_poco = new biz_collection();
            collection collection = collection_poco.GetCollection(collection_id, false, true, false, true, false);
            if (collection != null)
            {
                if (collection.booth.person.Id == user.CurrentUserID)
                    return true;
            }

            return false;
        }
    }
}