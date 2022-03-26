using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_salesman : poco_person
    {
        [DisplayName("Tlf. nr.")]
        public int? phonenumber { get; set; }
        public bool show_phone { get; set; }

        [DisplayName("Tlf. Nr.")]
        public string phonenumber_nop { get; set; }

        [DisplayName("Beskrivelse")]
        public virtual string description { get; set; }
        public virtual List<poco_booth> booth_pocos { get; set; }

        public poco_salesman() : base()
        {

        }
        /*public poco_salesman(EbazarDB db) : base(db)
        {
            
        }*/
        /*~poco_salesman()
        {
            db?.Dispose();
        }*/

        public person GetPerson(string person_id, string descriminator, bool withbooth, bool withfavorites, bool withfollowing) 
        {
            if (person_id == null)// ikke IsNullOrEmpty da user id fra identity kan være tom streng ved UnAuthorized
                throw new Exception("POCO_SALESMAN > GetPerson > person_id NULL [Check DB]");
            if (person_id == "")
                return null;
            if (descriminator != "Salesman")
                return null;

            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_booth booth_poco = new poco_booth();
            //string descriminator = "Salesman";

            person pers = (from per in _db.person
                           where per.Id == person_id && per.descriminator == descriminator
                           select new
                           {
                               Id = per.Id,
                               sysname = per.sysname,
                               firstname = per.firstname,
                               lastname = per.lastname,
                               created_on = per.created_on,
                               phonenumber = per.phonenumber,
                               show_phone = per.show_phone,
                               email = per.email,
                               request_email = per.request_email,
                               profileimage = per.profileimage,
                               favorites_product = per.favorites_product,
                               favorites_collection = per.favorites_collection,
                               following = per.following,
                               //following = withfollowing ? this.GetFollowing() : null,
                               descriminator = per.descriminator,
                               description = descriminator == "Salesman" ? per.description : "",
                               
                               boothrating = per.boothrating.ToList()                               
                           }).AsEnumerable()
                      .Select(per => new person
                      {
                          Id = per.Id,
                          sysname = per.sysname,
                          firstname = per.firstname,
                          lastname = per.lastname,
                          created_on = per.created_on,
                          phonenumber = per.phonenumber,
                          show_phone = per.show_phone,
                          email = per.email,
                          request_email = per.request_email,
                          profileimage = per.profileimage,
                          favorites_product = withfavorites ? this.GetFavoritesProducts(new person() { favorites_product = per.favorites_product }) : null,
                          favorites_collection = withfavorites ? this.GetFavoritesCollections(new person() { favorites_collection = per.favorites_collection }) : null,
                          following = withfollowing ? this.GetFollowing(new person() { following = per.following }) : null,
                          descriminator = per.descriminator,
                          description = descriminator == "Salesman" ? per.description : "",
                          booth = withbooth && descriminator == "Salesman" ? booth_poco.GetBooths(per.Id, false) : null,
                          boothrating = per.boothrating.ToList()
                          
                      }).FirstOrDefault();
            if(pers == null)
                return null;
            return pers;
        }
        public override T GetPersonPOCO<T>(string person_id, string descriminator, bool withbooth, bool withfavorites, bool withfollowing)
        {
            if (person_id == "")
                return null;
            //poco_booth booth_poco = new poco_booth(new EbazarDB());
            //string descriminator = "Salesman";
            T person = new T();
            person pers = GetPerson(person_id, descriminator, withbooth, withfavorites, withfollowing);
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

        public bool IsSalesman(string id, string discriminator) 
        {
            if (discriminator != "Salesman")
                return false;

            EbazarDB _db = DAL.GetInstance().GetContext();

            person pers = (from per in _db.person
                           where per.Id == person_id && per.descriminator == discriminator
                           select per).FirstOrDefault();
            if (pers == null)
                return false;
            return true;
        }

        //public virtual List<person> GetPersons(bool withfavorites, bool withfollowing)
        //{
        //    poco_booth booth_poco = new poco_booth(new EbazarDB());
        //    string descriminator = "Salesman";
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
        //                                phonenumber = per.phonenumber,
        //                                show_phone = per.show_phone,
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
        //                                description = descriminator == "Salesman" ? per.description : "",
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
        //                           phonenumber = per.phonenumber,
        //                           show_phone = per.show_phone,
        //                           email = per.email,
        //                           request_email = per.request_email,
        //                           profileimage = per.profileimage,
        //                           favorites_product = withfavorites ? this._GetFavoritesProducts(new person() { favorites_product = per.favorites_product }) : null,
        //                           favorites_collection = withfavorites ? this._GetFavoritesCollections(new person() { favorites_collection = per.favorites_collection }) : null,
        //                           following = withfollowing ? this.GetFollowing(new person() { following = per.following }) : null,
        //                           descriminator = per.descriminator,
        //                           description = descriminator == "Salesman" ? per.description : "",
        //                           booth = descriminator == "Salesman" ? booth_poco.GetBooths(per.Id, false) : null,//.ToPocoList(per.booth.ToList()) : null,
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

            poco_booth booth_poco = new poco_booth();
            //is_salesman = typeof(T) == typeof(poco_salesman);
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
            per.phonenumber = this.phonenumber != null ? this.phonenumber : 99999999;
            per.show_phone = this.show_phone;
            per.email = !string.IsNullOrEmpty(this.email) && this.email != Texts.GetNopValue(NOP.NO_EMAIL.ToString()) ? this.email : "";
            per.request_email = this.request_email;
            per.description = !string.IsNullOrEmpty(this.description) ? this.description : "";
            per.profileimage = !string.IsNullOrEmpty(this.profileimage) ? this.profileimage : "";

            per.descriminator = typeof(T) == typeof(poco_salesman) ? "Salesman" : "Customer";
            per.booth = typeof(T) == typeof(poco_salesman) ? booth_poco._ToBoothList(this.booth_pocos) : null;


            _db.person.Add(per);
            _db.SaveChanges();
        }

        public override void UpdatePerson<T>()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_booth booth_poco = new poco_booth();
            
            var per = _db.person.Where(s => s.Id == this.person_id).FirstOrDefault();
            if (per == null)
                throw new Exception("A-OK, handled.");
            
                if (!string.IsNullOrEmpty(this.sysname))
                    per.sysname = this.sysname;
                else
                    throw new Exception("A-OK, handled.");

                per.firstname = !string.IsNullOrEmpty(this.firstname) && this.firstname != Texts.GetNopValue(NOP.NO_FIRSTNAME.ToString()) ? this.firstname : "";
                per.lastname = !string.IsNullOrEmpty(this.lastname) ? this.lastname : "********";
                per.phonenumber = this.phonenumber;
                per.show_phone = this.show_phone;
                per.email = !string.IsNullOrEmpty(this.email) && this.email != Texts.GetNopValue(NOP.NO_EMAIL.ToString()) ? this.email : "";
                per.request_email = this.request_email;
                per.description = !string.IsNullOrEmpty(this.description) ? this.description : "";
                per.profileimage = !string.IsNullOrEmpty(this.profileimage) ? this.profileimage : "";

                if (this.favorites_product != null)
                {
                    foreach (poco_product poco in this.favorites_product)
                    {
                        product pro = new product();
                        poco.ToProduct(false, ref pro);
                        per.favorites_product.Add(pro);
                    }
                }
                if (this.favorites_collection != null)
                {
                    foreach (poco_collection poco in this.favorites_collection)
                    {
                        collection pro = new collection();
                        poco.ToCollection(false, ref pro);
                        per.favorites_collection.Add(pro);
                    }
                }
                if (this.following != null)
                {
                    foreach (poco_booth poco in this.following)
                    {
                        booth boo = poco.ToBooth(false);
                        per.following.Add(boo);
                    }
                }

                per.descriminator = typeof(T) == typeof(poco_salesman) ? "Salesman" : "Customer";
                per.booth = null;

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
            this.phonenumber = per.phonenumber;
            this.show_phone = per.show_phone;
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
            
            if (!string.IsNullOrEmpty(per.description))
                this.description = per.description;
            else
                this.description = "";

            if (per.booth != null)
                this.booth_pocos = booth_poco.ToPocoList(per.booth.ToList()/*, false*/, null);
            
            if (per.boothrating != null)
                this.boothrating = per.boothrating.ToList();
            else
                this.boothrating = new List<boothrating>();
        }
    }
}