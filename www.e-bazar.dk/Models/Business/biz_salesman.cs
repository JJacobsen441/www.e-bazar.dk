using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_salesman : biz_person
    {
        public biz_salesman() : base()
        {

        }

        public bool IsSalesman(string id, string discriminator) 
        {
            if (id == "")
                return true;

            if (discriminator != "Salesman")
                return false;

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                person pers = (from per in _db.person
                           where per.Id == id && per.descriminator == discriminator
                           select per).FirstOrDefault();
                if (pers == null)
                    return false;
                return true;
            }
        }        

        public override void SavePerson<T>(T dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto_salesman _s = dto as dto_salesman;

                biz_booth booth_poco = new biz_booth();
                
                person per = new person();
                if (!string.IsNullOrEmpty(_s.person_id))
                    per.Id = _s.person_id;
                else
                    throw new Exception("A-OK, handled.");

                Dictionary<string, string> dirs = SetupHelper.SetupProfileDirs(_s);
                per.sysname = dirs["identity_id"];

                per.firstname = !string.IsNullOrEmpty(_s.firstname) && _s.firstname != TextHelper.GetNopValue(NOP.NO_FIRSTNAME.ToString()) ? _s.firstname : "";
                per.lastname = !string.IsNullOrEmpty(_s.lastname) ? _s.lastname : "********";
                per.created_on = DateTime.Now;
                per.phonenumber = _s.phonenumber != null ? _s.phonenumber : 99999999;
                per.show_phone = _s.show_phone;
                per.email = !string.IsNullOrEmpty(_s.email) && _s.email != TextHelper.GetNopValue(NOP.NO_EMAIL.ToString()) ? _s.email : "";
                per.request_email = _s.request_email;
                per.description = !string.IsNullOrEmpty(_s.description) ? _s.description : "";
                per.profileimage = !string.IsNullOrEmpty(_s.profileimage) ? _s.profileimage : "";

                per.descriminator = typeof(T) == typeof(dto_salesman) ? "Salesman" : "Customer";
                per.booth = typeof(T) == typeof(dto_salesman) ? booth_poco.ToBoothList(_s.booth_dtos, _db) : null;


                _db.person.Add(per);
                _db.SaveChanges();
            }
        }

        public override void UpdatePerson<T>(T dto)
        {
            if (dto.IsNull())
                throw new Exception("A-OK, handled");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto_salesman _s = dto as dto_salesman;

                biz_booth booth_poco = new biz_booth();
            
                var per = _db.person.Where(s => s.Id == _s.person_id).FirstOrDefault();
                if (per == null)
                    throw new Exception("A-OK, handled.");
            
                if (!string.IsNullOrEmpty(_s.sysname))
                    per.sysname = _s.sysname;
                else
                    throw new Exception("A-OK, handled.");

                per.firstname = !string.IsNullOrEmpty(_s.firstname) && _s.firstname != TextHelper.GetNopValue(NOP.NO_FIRSTNAME.ToString()) ? _s.firstname : "";
                per.lastname = !string.IsNullOrEmpty(_s.lastname) ? _s.lastname : "********";
                per.phonenumber = _s.phonenumber;
                per.show_phone = _s.show_phone;
                per.email = !string.IsNullOrEmpty(_s.email) && _s.email != TextHelper.GetNopValue(NOP.NO_EMAIL.ToString()) ? _s.email : "";
                per.request_email = _s.request_email;
                per.description = !string.IsNullOrEmpty(_s.description) ? _s.description : "";
                per.profileimage = !string.IsNullOrEmpty(_s.profileimage) ? _s.profileimage : "";

                if (_s.favorites_product != null)
                {
                    foreach (dto_product _dto in _s.favorites_product)
                    {
                        product pro = new product();
                        biz_product biz = new biz_product();
                        biz.ToProduct(false, _dto, ref pro, _db);
                        per.favorites_product.Add(pro);
                    }
                }
                if (_s.favorites_collection != null)
                {
                    foreach (dto_collection _dto in _s.favorites_collection)
                    {
                        collection pro = new collection();
                        biz_collection biz = new biz_collection();
                        biz.ToCollection(false, _dto, ref pro, _db);
                        per.favorites_collection.Add(pro);
                    }
                }
                if (_s.following != null)
                {
                    biz_booth biz = new biz_booth();
                    foreach (dto_booth _dto in _s.following)
                    {
                        //using (EbazarDB _db2 = new EbazarDB())
                        {

                            booth boo = biz.ToBooth(false, _dto, _db);
                            per.following.Add(boo);
                        }
                    }
                }

                per.descriminator = typeof(T) == typeof(dto_salesman) ? "Salesman" : "Customer";
                per.booth = null;

                _db.SaveChanges();            
            }
        }

        
        public override T ToDTO<T>(person per)
        {
            if (per == null)
                return null;

            dto_salesman dto = new dto_salesman();

            biz_product pro_biz = new biz_product(false);
            biz_collection col_biz = new biz_collection();
            biz_booth booth_biz = new biz_booth();

            dto.iscreated = true;

            dto.person_id = per.Id;
            dto.sysname = per.sysname;
            dto.firstname = per.firstname;
            dto.lastname = per.lastname;
            dto.created_on = per.created_on;
            dto.phonenumber = per.phonenumber;
            dto.show_phone = per.show_phone;
            dto.email = per.email;
            dto.request_email = per.request_email;
            dto.profileimage = per.profileimage;
            if (per.favorites_product != null)
                dto.favorites_product = pro_biz.ToDTOList(NullHelper.PerNull(per.favorites_product.ToList(), true), null, "");
            if (per.favorites_collection != null)
                dto.favorites_collection = col_biz.ToDTOList(NullHelper.PerNull(per.favorites_collection.ToList(), true), null, "");
            if (per.following != null)
                dto.following = booth_biz.ToDTOList(NullHelper.PerNull(per.following.ToList()), null);
            dto.nator = per.descriminator;
            
            if (!string.IsNullOrEmpty(per.description))
                dto.description = per.description;
            else
                dto.description = "";

            if (per.booth != null)
                dto.booth_dtos = booth_biz.ToDTOList(per.booth.ToList(), null);
            
            if (per.boothrating != null)
                dto.boothrating = per.boothrating.ToList();
            else
                dto.boothrating = new List<boothrating>();

            return dto as T;
        }
    }
}