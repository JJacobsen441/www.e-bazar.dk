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
        public poco_person GetCurrentUser(bool withbooth, bool withfavorites, bool withfollowing)
        {
            poco_salesman salesman_poco = new poco_salesman();
            poco_customer customer_poco = new poco_customer();
            poco_person person_poco = salesman_poco.GetPersonPOCO<poco_salesman>(CurrentUserID, CurrentType, withbooth, withfavorites, withfollowing);
            if (person_poco == null)
                person_poco = customer_poco.GetPersonPOCO<poco_customer>(CurrentUserID, CurrentType, withbooth, withfavorites, withfollowing);

            if (person_poco != null)
                return person_poco;
            return null;
        }
        public bool OwnsBooth(int booth_id)
        {
            poco_booth booth_poco = new poco_booth();
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
            poco_product product_poco = new poco_product(false);
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
            poco_collection collection_poco = new poco_collection();
            collection collection = collection_poco.GetCollection(collection_id, false, true, false, true, false);
            if (collection != null)
            {
                if (collection.booth.person.Id == user.CurrentUserID)
                    return true;
            }

            return false;
        }
        //public poco_person GetCurrentUserScrub() 
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
        public bool IsType<T>()
        {
            poco_person user = GetCurrentUser(false, false, false);
            if(user != null)
                return user.IsType<T>();
            throw new Exception("A-OK, handled.");
        }
    }
}