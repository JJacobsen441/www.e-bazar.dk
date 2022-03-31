using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_customer : poco_person
    {
        public poco_customer() : base()
        {

        }

        public person GetPerson(string person_id, string descriminator, bool withfavorites, bool withfollowing)
        {
            if (person_id == null)// ikke IsNullOrEmpty da user id fra identity kan være tom streng ved UnAuthorized
                throw new Exception("POCO_CUSTOMER > GetPerson > person_id NULL [Check DB]");
            if (person_id == "")
                return null;
            if (descriminator != "Customer")
                return null;

            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * jeg ved at virtual burde fjenes fra model entiteterne
                 * */

                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<person> _p = _db.person
                    .Include("favorites_product")
                    .Include("favorites_collection")
                    .Include("following")
                    .Include("booth")
                    .Include("boothrating")
                    .Where(x => x.Id == person_id && x.descriminator == descriminator);

                person p = _p.AsEnumerable().FirstOrDefault();
                
                if (p == null)
                    throw new Exception("A-OK, Handled.");
                else
                {
                    NullHelper.PerNull(p, false, withfavorites, withfollowing);
                    if(withfollowing) NullHelper.PerNull(p.following.ToList());
                    if(withfavorites) NullHelper.PerNull(p.favorites_product.ToList(), true);
                    if(withfavorites) NullHelper.PerNull(p.favorites_collection.ToList(), true);
                }
                return p;
            }            
        }
        public override T GetPersonPOCO<T>(string person_id, string descriminator, bool withbooth, bool withfavorites, bool withfollowing)
        {
            if (person_id == "")
                return null;

            T person = new T();
            
            person pers = GetPerson(person_id, descriminator, withfavorites, withfollowing);
            
            person.ToPoco<T>(pers);
            return person;
            
        }

        public override void SavePerson<T>()
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


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
        }
        public override void UpdatePerson<T>()
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                poco_booth booth_poco = new poco_booth();
                
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
                        product pro = new product();
                        poco.ToProduct(false, ref pro, _db);
                        per.favorites_product.Add(pro);
                    }
                }
                if (this.favorites_collection != null)
                {
                    foreach (poco_collection poco in this.favorites_collection)
                    {
                        collection pro = new collection();
                        poco.ToCollection(false, ref pro, _db);
                        per.favorites_collection.Add(pro);
                    }
                }
                if (this.following != null)
                {
                    foreach (poco_booth poco in this.following)
                    {
                        //using (EbazarDB _db2 = new EbazarDB())
                        {                            
                            booth boo = poco.ToBooth(false, _db);
                            per.following.Add(boo);
                        }
                    }
                }

                per.descriminator = typeof(T) == typeof(poco_salesman) ? "Salesman" : "Customer";
                
                _db.SaveChanges();            
            }
        }

        
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
                this.favorites_product = pro_poco.ToPocoList(NullHelper.PerNull(per.favorites_product.ToList(), true), null, ""); //this.GetFavoritesProductPOCOs(per) : null,
            if (per.favorites_collection != null)
                this.favorites_collection = col_poco.ToPocoList(NullHelper.PerNull(per.favorites_collection.ToList(), true), null, ""); //this.GetFavoritesCollectionPOCOs(per) : null,
            if (per.following != null)
                this.following = booth_poco.ToPocoList(NullHelper.PerNull(per.following.ToList())/*, false*/, null); //this.GetFollowingPOCOs(per) : null,
            this.nator = per.descriminator;
            
            if(per.boothrating != null)
                this.boothrating = per.boothrating.ToList();
            else
                this.boothrating = new List<boothrating>();
        }
    }
}