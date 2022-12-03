using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_customer : biz_person
    {
        public biz_customer() : base()
        {
        }

        public override void SavePerson<T>(T dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto_customer _s = dto as dto_customer;

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
                per.email = !string.IsNullOrEmpty(_s.email) && _s.email != TextHelper.GetNopValue(NOP.NO_EMAIL.ToString()) ? _s.email : "";
                per.request_email = _s.request_email;
                per.profileimage = !string.IsNullOrEmpty(_s.profileimage) ? _s.profileimage : "";

                per.descriminator = typeof(T) == typeof(dto_salesman) ? "Salesman" : "Customer";
                per.booth = null;

                _db.person.Add(per);
                _db.SaveChanges();
            }
        }
        public override void UpdatePerson<T>(T dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto_customer _c = dto as dto_customer;

                biz_booth booth_poco = new biz_booth();
                
                var per = _db.person.Where(s => s.Id == _c.person_id).FirstOrDefault();
                if (per == null)
                    throw new Exception("A-OK, handled.");
            
                if (!string.IsNullOrEmpty(_c.sysname))
                    per.sysname = _c.sysname;
                else
                    throw new Exception("A-OK, handled.");

                per.firstname = !string.IsNullOrEmpty(_c.firstname) && _c.firstname != TextHelper.GetNopValue(NOP.NO_FIRSTNAME.ToString()) ? _c.firstname : "";
                per.lastname = !string.IsNullOrEmpty(_c.lastname) ? _c.lastname : "****";
                per.email = !string.IsNullOrEmpty(_c.email) && _c.email != TextHelper.GetNopValue(NOP.NO_EMAIL.ToString()) ? _c.email : "";
                per.request_email = _c.request_email;
                per.profileimage = !string.IsNullOrEmpty(_c.profileimage) ? _c.profileimage : "";

                if (_c.favorites_product != null)
                {
                    foreach (dto_product _dto in _c.favorites_product)
                    {                        
                        product pro = new product();
                        biz_product biz = new biz_product();
                        biz.ToProduct(false, _dto, ref pro, _db);
                        per.favorites_product.Add(pro);
                    }
                }
                if (_c.favorites_collection != null)
                {
                    foreach (dto_collection _dto in _c.favorites_collection)
                    {
                        collection pro = new collection();
                        biz_collection biz = new biz_collection();
                        biz.ToCollection(false, _dto, ref pro, _db);
                        per.favorites_collection.Add(pro);
                    }
                }
                if (_c.following != null)
                {
                    biz_booth biz = new biz_booth();
                    foreach (dto_booth _dto in _c.following)
                    {
                        //using (EbazarDB _db2 = new EbazarDB())
                        {                            
                            booth boo = biz.ToBooth(false, _dto, _db);
                            per.following.Add(boo);
                        }
                    }
                }

                per.descriminator = typeof(T) == typeof(dto_salesman) ? "Salesman" : "Customer";
                
                _db.SaveChanges();            
            }
        }

        
        public override T ToDTO<T>(person per)
        {
            if (per == null)
                return null;

            dto_customer dto = new dto_customer();

            biz_product pro_poco = new biz_product(false);
            biz_collection col_poco = new biz_collection();
            biz_booth booth_poco = new biz_booth();

            dto.iscreated = true;

            dto.person_id = per.Id;
            dto.sysname = per.sysname;
            dto.firstname = per.firstname;
            dto.lastname = per.lastname;
            dto.created_on = per.created_on;

            dto.email = per.email;
            dto.request_email = per.request_email;
            dto.profileimage = per.profileimage;
            if (per.favorites_product != null)
                dto.favorites_product = pro_poco.ToDTOList(NullHelper.PerNull(per.favorites_product.ToList(), true), null, ""); //this.GetFavoritesProductPOCOs(per) : null,
            if (per.favorites_collection != null)
                dto.favorites_collection = col_poco.ToDTOList(NullHelper.PerNull(per.favorites_collection.ToList(), true), null, ""); //this.GetFavoritesCollectionPOCOs(per) : null,
            if (per.following != null)
                dto.following = booth_poco.ToDTOList(NullHelper.PerNull(per.following.ToList())/*, false*/, null); //this.GetFollowingPOCOs(per) : null,
            dto.nator = per.descriminator;
            
            if(per.boothrating != null)
                dto.boothrating = per.boothrating.ToList();
            else
                dto.boothrating = new List<boothrating>();

            return dto as T;
        }
    }
}