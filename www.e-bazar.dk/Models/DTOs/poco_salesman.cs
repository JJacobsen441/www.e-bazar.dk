using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using www.e_bazar.dk.Extensions;
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

        public person GetPerson(string person_id, string descriminator, bool withbooth, bool withfavorites, bool withfollowing) 
        {
            if (person_id == null)// ikke IsNullOrEmpty da user id fra identity kan være tom streng ved UnAuthorized
                throw new Exception("POCO_SALESMAN > GetPerson > person_id NULL [Check DB]");
            if (person_id == "")
                return null;
            if (descriminator != "Salesman")
                return null;

            //EbazarDB _db = DAL.GetInstance().GetContext();
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
                    .Where(x=> x.Id == person_id && x.descriminator == descriminator);

                person p = _p.AsEnumerable().FirstOrDefault();
                
                if(p == null)
                    return null;
                else
                {
                    NullHelper.PerNull(p, withbooth && descriminator == "Salesman", withfavorites, withfollowing);
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
            person pers = GetPerson(person_id, descriminator, withbooth, withfavorites, withfollowing);
            
            person.ToPoco<T>(pers);
            return person;
        }

        public bool IsSalesman(string id, string discriminator) 
        {
            if (discriminator != "Salesman")
                return false;

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                person pers = (from per in _db.person
                           where per.Id == person_id && per.descriminator == discriminator
                           select per).FirstOrDefault();
                if (pers == null)
                    return false;
                return true;
            }
        }        

        public override void SavePerson<T>()
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                poco_booth booth_poco = new poco_booth();
                
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
                per.booth = typeof(T) == typeof(poco_salesman) ? booth_poco.ToBoothList(this.booth_pocos, _db) : null;


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
                per.booth = null;

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
            this.phonenumber = per.phonenumber;
            this.show_phone = per.show_phone;
            this.email = per.email;
            this.request_email = per.request_email;
            this.profileimage = per.profileimage;
            if (per.favorites_product != null)
                this.favorites_product = pro_poco.ToPocoList(NullHelper.PerNull(per.favorites_product.ToList(), true), null, "");
            if (per.favorites_collection != null)
                this.favorites_collection = col_poco.ToPocoList(NullHelper.PerNull(per.favorites_collection.ToList(), true), null, "");
            if (per.following != null)
                this.following = booth_poco.ToPocoList(NullHelper.PerNull(per.following.ToList()), null);
            this.nator = per.descriminator;
            
            if (!string.IsNullOrEmpty(per.description))
                this.description = per.description;
            else
                this.description = "";

            if (per.booth != null)
                this.booth_pocos = booth_poco.ToPocoList(per.booth.ToList(), null);
            
            if (per.boothrating != null)
                this.boothrating = per.boothrating.ToList();
            else
                this.boothrating = new List<boothrating>();
        }
    }
}