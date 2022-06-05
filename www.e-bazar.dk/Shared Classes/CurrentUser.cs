using System;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;

namespace www.e_bazar.dk.SharedClasses
{
    public class CurrentUser
    {
        private static CurrentUser user;
        public string CurrentUserID
        {
            get 
            {
                return ThisSession.CurrentUserId;
            }
            set 
            {
                ThisSession.CurrentUserId = value;
            }
        }

        public string CurrentUserName
        {
            get
            {
                return ThisSession.CurrentUserName;
            }
            set
            {
                ThisSession.CurrentUserName = value;
            }
        }

        public bool CurrentIsAuthenticated
        {
            get
            {
                return ThisSession.CurrentIsAuthenticated;
            }
            set
            {
                ThisSession.CurrentIsAuthenticated = value;
            }
        }

        public string CurrentType
        {
            get
            {
                return ThisSession.CurrentType;
            }
            set
            {
                ThisSession.CurrentType = value;
            }
        }

        private CurrentUser()
        {
        }

        public static CurrentUser GetInstance()
        {
            //DAL.GetInstance(true).GetDB();
            if (user == null)
                user = new CurrentUser();
            return user;
        }

        public dto_person GetCurrentUser(bool withbooth, bool withfavorites, bool withfollowing)
        {
            if (CurrentUserID == "")
                return null;

            biz_salesman salesman_biz = new biz_salesman();
            biz_customer customer_biz = new biz_customer();
            dto_person person_dto = salesman_biz.GetPersonDTO<dto_salesman>(CurrentUserID, withbooth, withfavorites, withfollowing);
            
            if (person_dto.nator != CurrentType)
                throw new Exception("A-OK, Handled.");

            return person_dto;            
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

        //public biz_person GetCurrentUserScrub() 
        //{
        //    return GetCurrentUser(true, false, false, false);        
        //}

        public void Login(string ID, string Name, bool Authenticated, string Type)
        {
            CurrentUserID = ID;
            CurrentUserName = Name;
            CurrentIsAuthenticated = Authenticated;
            CurrentType = Type;

        }

        //public bool IsType<T>()
        //{
        //    dto_person user = GetCurrentUser(false, false, false);
        //    if(user != null)
        //    {
        //        if(user.GetType() == typeof(dto_salesman))
        //        {
        //            biz_salesman _s = new biz_salesman();
        //            return _s.IsType<T>();
        //        }
        //        else
        //        {
        //            biz_customer _c = new biz_customer();
        //            return _c.IsType<T>();
        //        }
        //    }
        //    throw new Exception("A-OK, handled.");
        //}
    }
}