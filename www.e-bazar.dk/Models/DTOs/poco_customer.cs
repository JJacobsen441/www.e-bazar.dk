using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_customer : poco_person
    {
        public poco_customer() : base()
        {

        }
        /*public poco_customer(EbazarDB db) : base(db)
        {

        }*/
        /*~poco_customer()
        {
            db?.Dispose();
        }*/

        public person GetPerson(string person_id, string descriminator, bool withfavorites, bool withfollowing)
        {
            if (person_id == null)// ikke IsNullOrEmpty da user id fra identity kan være tom streng ved UnAuthorized
                throw new Exception("POCO_CUSTOMER > GetPerson > person_id NULL [Check DB]");
            if (person_id == "")
                return null;
            if (descriminator != "Customer")
                return null;

            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_booth booth_poco = new poco_booth();
            //string descriminator = "Customer";
            List<person> list = _db.person.ToList();
            person pers = (from per in list
                           where per.Id == person_id && per.descriminator == descriminator
                           select new
                           {
                               Id = per.Id,
                               sysname = per.sysname,
                               firstname = per.firstname,
                               lastname = per.lastname,
                               created_on = per.created_on,
                               
                               email = per.email,
                               request_email = per.request_email,
                               profileimage = per.profileimage,
                               favorites_product = per.favorites_product,
                               favorites_collection = per.favorites_collection,
                               following = per.following,
                               //following = withfollowing ? per.following.ToList() : null, //this.GetFollowingPOCOs(per) : null,
                               descriminator = per.descriminator,
                               
                               boothrating = per.boothrating.ToList()
                           }).AsEnumerable()
                      .Select(per => new person
                      {
                          Id = per.Id,
                          sysname = per.sysname,
                          firstname = per.firstname,
                          lastname = per.lastname,
                          created_on = per.created_on,
                          email = per.email,
                          request_email = per.request_email,
                          profileimage = per.profileimage,
                          favorites_product = withfavorites ? this.GetFavoritesProducts(new person() { favorites_product = per.favorites_product }) : null,
                          favorites_collection = withfavorites ? this.GetFavoritesCollections(new person() { favorites_collection = per.favorites_collection }) : null,
                          following = withfollowing ? this.GetFollowing(new person() { following = per.following }) : null,
                          descriminator = per.descriminator,
                          
                          boothrating = per.boothrating.ToList()

                      }).FirstOrDefault();
            if(pers == null)
                throw new Exception("A-OK, Handled.");
            return pers;
        }
        public override T GetPersonPOCO<T>(string person_id, string descriminator, bool withbooth, bool withfavorites, bool withfollowing)
        {
            if (person_id == "")
                return null;
            //poco_booth booth_poco = new poco_booth(new EbazarDB());
            //string descriminator = "Salesman";
            T person = new T();
            
            person pers = GetPerson(person_id, descriminator, withfavorites, withfollowing);
            //if (pers == null)
            //    return null;

            //if (pers.following == null && withfollowing)
            //    return null;
            //if (pers.favorites_product == null && withfavorites)
            //    return null;
            //if (pers.favorites_collection == null && withfavorites)
            //    return null;
            person.ToPoco<T>(pers);
            return person;
            
        }

        //public virtual List<person> GetPersons(bool withfavorites, bool withfollowing)
        //{
        //    poco_booth booth_poco = new poco_booth(new EbazarDB());
        //    string descriminator = "Customer";
        //    List<person> list = db.person.ToList();
        //    List<person> persons = (from per in list
        //                            where per.descriminator == descriminator
        //                            select new
        //                            {
        //                                Id = per.Id,
        //                                sysname = per.sysname,
        //                                firstname = per.firstname,
        //                                lastname = per.lastname,
        //                                created_on = per.created_on,
                                        
        //                                email = per.email,
        //                                request_email = per.request_email,
        //                                profileimage = per.profileimage,
        //                                favorites_product = per.favorites_product,
        //                                favorites_collection = per.favorites_collection,
        //                                following = per.following,
        //                                //favorites_product = withfavorites ? this.GetFavoritesProducts(per) : null,
        //                                //favorites_collection = withfavorites ? this.GetFavoritesCollections(per) : null,
        //                                //following = withfollowing ? per.following.ToList() : null,// this.GetFollowingPOCOs(per) : null,
        //                                descriminator = per.descriminator,
                                        
        //                                //booth = descriminator == "Salesman" ? per.booth.ToList() : null,//booth_poco.ToPocoList(per.booth.ToList()) : null,
        //                                boothrating = per.boothrating.ToList()
        //                                //identity_id = withidentityid ? UserManager.FindById(person_id).Id : null,
        //                                //is_salesman = is_salesman
        //                            }).AsEnumerable()
        //                       .Select(per => new person
        //                       {
        //                           Id = per.Id,
        //                           sysname = per.sysname,
        //                           firstname = per.firstname,
        //                           lastname = per.lastname,
        //                           created_on = per.created_on,
                                   
        //                           email = per.email,
        //                           request_email = per.request_email,
        //                           profileimage = per.profileimage,
        //                           favorites_product = withfavorites ? this._GetFavoritesProducts(new person() { favorites_product = per.favorites_product }) : null,
        //                           favorites_collection = withfavorites ? this._GetFavoritesCollections(new person() { favorites_collection = per.favorites_collection }) : null,
        //                           following = withfollowing ? this.GetFollowing(new person() { following = per.following }) : null,
        //                           descriminator = per.descriminator,

        //                           boothrating = per.boothrating.ToList()
        //                           //identity_id = withidentityid ? UserManager.FindById(person_id).Id : null,
        //                           //is_salesman = is_salesman
        //                       }).ToList();
        //    return persons;
        //}
        //public override List<T> _GetPersonsPOCO<T>(bool withfavorites, bool withfollowing)
        //{
        //    //string descriminator = "Salesman";
        //    List<T> list = new List<T>();
        //    List<person> persons = GetPersons(withfavorites, withfollowing);
        //    if (persons != null)
        //    {
        //        list = this.ToPocoList<T>(persons);
        //        return list;
        //    }
        //    return null;
        //}

        public override void SavePerson<T>()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            person per = new person();
            if (!string.IsNullOrEmpty(this.person_id))
                per.Id = this.person_id;
            else
                throw new Exception("A-OK, handled.");

            Dictionary<string, string> dirs = Setup.SetupProfileDirs(this);
            per.sysname = dirs["identity_id"];

            per.firstname = !string.IsNullOrEmpty(this.firstname) && this.firstname != Texts.GetNopValue(NOP.NO_FIRSTNAME.ToString()) ? this.firstname : "";
            per.lastname = !string.IsNullOrEmpty(this.lastname) ? this.lastname : "********";
            per.created_on = DateTime.Now;
            per.email = !string.IsNullOrEmpty(this.email) && this.email != Texts.GetNopValue(NOP.NO_EMAIL.ToString()) ? this.email : "";
            per.request_email = this.request_email;
            per.profileimage = !string.IsNullOrEmpty(this.profileimage) ? this.profileimage : "";

            per.descriminator = typeof(T) == typeof(poco_salesman) ? "Salesman" : "Customer";
            per.booth = null;

            _db.person.Add(per);
            _db.SaveChanges();
        }
        public override void UpdatePerson<T>()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_booth booth_poco = new poco_booth();
            //is_salesman = typeof(T) == typeof(poco_salesman);
            var per = _db.person.Where(s => s.Id == this.person_id).FirstOrDefault();
            if (per == null)
                throw new Exception("A-OK, handled.");
            
                if (!string.IsNullOrEmpty(this.sysname))
                    per.sysname = this.sysname;
                else
                    throw new Exception("A-OK, handled.");

                per.firstname = !string.IsNullOrEmpty(this.firstname) && this.firstname != Texts.GetNopValue(NOP.NO_FIRSTNAME.ToString()) ? this.firstname : "";
                per.lastname = !string.IsNullOrEmpty(this.lastname) ? this.lastname : "****";
                per.email = !string.IsNullOrEmpty(this.email) && this.email != Texts.GetNopValue(NOP.NO_EMAIL.ToString()) ? this.email : "";
                per.request_email = this.request_email;
                per.profileimage = !string.IsNullOrEmpty(this.profileimage) ? this.profileimage : "";

                if (this.favorites_product != null)
                {
                    foreach (poco_product poco in this.favorites_product)
                    {
                        //poco.db = this.db;
                        product pro = new product();
                        poco.ToProduct(false, ref pro, _db);
                        per.favorites_product.Add(pro);
                    }
                }
                if (this.favorites_collection != null)
                {
                    foreach (poco_collection poco in this.favorites_collection)
                    {
                        //poco.db = this.db;
                        collection pro = new collection();
                        poco.ToCollection(false, ref pro, _db);
                        per.favorites_collection.Add(pro);
                    }
                }
                if (this.following != null)
                {
                    foreach (poco_booth poco in this.following)
                    {
                        //poco.db = this.db;
                        booth boo = poco.ToBooth(false);
                        per.following.Add(boo);
                    }
                }

                per.descriminator = typeof(T) == typeof(poco_salesman) ? "Salesman" : "Customer";
            //per.booth = null;// typeof(T) == typeof(poco_salesman) ? booth_poco.ToBoothList(person_poco.booth_pocos) : null;

            _db.SaveChanges();
            
        }

        //public override List<T> ToPocoList<T>(ICollection<person> persons)
        //{
        //    List<T> list = new List<T>();
        //    if (persons == null)
        //        return list;
        //    foreach (person per in persons)
        //    {
        //        T p = new T();
        //        p.ToPoco<T>(per);
        //        list.Add(p);
        //    }
        //    return list;
        //}
        public override void ToPoco<T>(person per)
        {
            if (per == null)
                return;

            poco_product pro_poco = new poco_product(false);
            poco_collection col_poco = new poco_collection();
            poco_booth booth_poco = new poco_booth();

            this.iscreated = true;

            this.person_id = per.Id;
            this.sysname = per.sysname;
            this.firstname = per.firstname;
            this.lastname = per.lastname;
            this.created_on = per.created_on;
            
            this.email = per.email;
            this.request_email = per.request_email;
            this.profileimage = per.profileimage;
            if (per.favorites_product != null)
                this.favorites_product = pro_poco.ToPocoList(Null(per.favorites_product.ToList(), true), null, ""); //this.GetFavoritesProductPOCOs(per) : null,
            if (per.favorites_collection != null)
                this.favorites_collection = col_poco.ToPocoList(Null(per.favorites_collection.ToList(), true), null, ""); //this.GetFavoritesCollectionPOCOs(per) : null,
            if (per.following != null)
                this.following = booth_poco.ToPocoList(Null(per.following.ToList())/*, false*/, null); //this.GetFollowingPOCOs(per) : null,
            this.nator = per.descriminator;
            
            if(per.boothrating != null)
                this.boothrating = per.boothrating.ToList();
            else
                this.boothrating = new List<boothrating>();
        }
    }
}