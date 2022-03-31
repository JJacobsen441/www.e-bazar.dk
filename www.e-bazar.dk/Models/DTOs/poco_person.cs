using Microsoft.AspNet.Identity;
using PostgreSQL.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models.Identity;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public abstract class poco_person
    {
        protected ApplicationDbContext ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> UserManager { get; set; }
        
        public poco_person()
        {
            //this.db = new EbazarDB();
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
        }        

        public string person_id { get; set; }
        
        public bool iscreated = true;

        [DisplayName("Fornavn")]
        public string firstname { get; set; }
        
        [DisplayName("Efternavn")]
        public string lastname { get; set; }
        
        [DisplayName("Oprettet")]
        public DateTime created_on { get; set; }

        

        [DisplayName("Kontakt email")]
        public string email { get; set; }
        public bool request_email { get; set; }

        [StringLength(128)]
        public string sysname { get; set; }

        [DisplayName("Profil Billed")]
        public string profileimage { get; set; }
        
        public string nator { get; set; }
        
        
        public virtual List<poco_product> favorites_product { get; set; }
        public virtual List<poco_collection> favorites_collection { get; set; }
        public virtual List<poco_booth> following { get; set; }
        
        public virtual List<boothrating> boothrating { get; set; }
                
        public bool IsType<T>()
        {
            if(typeof(T) == this.GetType())
                return true;
            return false;
        }
        
        public abstract T GetPersonPOCO<T>(string person_id, string descriminator, bool withbooth, bool withfavorites, bool withfollowing) where T : poco_person, new();
        public abstract void SavePerson<T>() where T : poco_person, new();
        public abstract void UpdatePerson<T>() where T : poco_person, new();
        public abstract void ToPoco<T>(person per) where T : poco_person, new();

        public class SampleObjectComparer : IComparer<IBoothItem>
        {
            public int Compare(IBoothItem x, IBoothItem y)
            {
                return GetHashCode(x).CompareTo(GetHashCode(x));
            }

            public bool Equals(IBoothItem x, IBoothItem y)
            {
                return x.GetType() == typeof(poco_product) && y.GetType() == typeof(poco_product);
            }

            public int GetHashCode(IBoothItem x)
            {
                return x.GetType().GetHashCode();
            }
        }

        public List<IBoothItem> _GetFavorites()
        {
            var comparer = new SampleObjectComparer();

            List<IBoothItem> items = new List<IBoothItem>();
            
            foreach (poco_product poco in favorites_product)
            {
                poco_booth b = poco.booth_poco;

                if (b == null)
                    b = poco.GetBoothPOCO();
                IBoothItem i = poco;
                i.booth_poco = b;
                items.Add(i);
            }

            foreach (poco_collection poco in favorites_collection)
            {
                poco_booth b = poco.booth_poco;

                if (b == null)
                    b = poco.GetBoothPOCO();
                IBoothItem i = poco;
                i.booth_poco = b;
                items.Add(i);
            }
            items = items.OrderBy(i => i.name).OrderBy(i => i, comparer).ToList();

            return items;
        }
        
        public List<product> GetFavoritesProducts(person per)
        {
            if (per == null)
                throw new Exception("A-OK, Check.");
            if (per.favorites_product == null)
                throw new Exception("A-OK, Check.");

            List<product> fav = new List<product>();
            
            foreach (product pro in per.favorites_product)
            {
                poco_category o;
                if (!pro.active)
                    continue;
                else if
                    (Categorys.ListContains(Categorys.CatsNoYes, pro.category_main_id, out o) && o.name == ".ingen" ||
                     Categorys.ListContains(Categorys.CatsNoYes, pro.category_second_id, out o) && o.name == "..ingen")
                    continue;

                fav.Add(pro);
            }
            return fav;
        }
        
        public List<collection> GetFavoritesCollections(person per)
        {
            if (per == null)
                throw new Exception("A-OK, Check.");
            if (per.favorites_collection == null)
                throw new Exception("A-OK, Check.");

            List<collection> fav = new List<collection>();
            
            foreach (collection col in per.favorites_collection)
            {
                poco_category o;
                if (!col.active)
                    continue;
                else if
                    (Categorys.ListContains(Categorys.CatsNoYes, col.category_main_id, out o) && o.name == ".ingen" ||
                     Categorys.ListContains(Categorys.CatsNoYes, col.category_second_id, out o) && o.name == "..ingen")
                    continue;

                fav.Add(col);
            }
            return fav;
        }

        protected List<booth> GetFollowing(person per)
        {
            if (per == null)
                throw new Exception("A-OK, Check.");
            if (per.following == null)
                throw new Exception("A-OK, Check.");

            List<booth> fol = new List<booth>();
            
            foreach (booth b in per.following)
                fol.Add(b);
            
            return fol;
        }
        
        public void AddFavorite(long product_id, int collection_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person per = _db.person.Where(p => p.Id == this.person_id).FirstOrDefault();
                if(per == null)
                    throw new Exception("A-OK, Handled.");
            
                if (product_id != -1)
                {
                    product pro = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
                    if(pro == null)
                        throw new Exception("A-OK, Handled.");
                    per.favorites_product.Add(pro);
                }
                else
                {
                    collection col = _db.collection.Where(c => c.Id == (int)collection_id).FirstOrDefault();
                    if(col == null)
                        throw new Exception("A-OK, Handled.");
                    per.favorites_collection.Add(col);
                }
                _db.SaveChanges();
            }            
        }

        public void RemoveFavorite(long product_id, int collection_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person per = _db.person.Where(p => p.Id == this.person_id).FirstOrDefault();
                if(per == null)
                    throw new Exception("A-OK, Handled.");
        
                if (product_id != -1)
                {
                    product pro = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
                    if(pro == null)
                        throw new Exception("A-OK, Handled.");
                    per.favorites_product.Remove(pro);
                }
                else
                {
                    collection col = _db.collection.Where(c => c.Id == (int)collection_id).FirstOrDefault();
                    if(col == null)
                        throw new Exception("A-OK, Handled.");
                    per.favorites_collection.Remove(col);
                }
                _db.SaveChanges();        
            }
        }

        public void AddFollowing(int booth_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person per = _db.person.Where(p => p.Id == this.person_id).FirstOrDefault();
                if(per == null)
                    throw new Exception("A-OK, Handled.");
        
                if (booth_id != -1)
                {
                    booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
                    if(booth != null)
                        per.following.Add(booth);
                    _db.SaveChanges();
                }
            }        
        }

        public void RemoveFollowing(int booth_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person per = _db.person.Where(p => p.Id == this.person_id).FirstOrDefault();
                if (per == null)
                    throw new Exception("A-OK, Handled.");
        
                if (booth_id != -1)
                {
                    booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
                    if(booth != null)
                        per.following.Remove(booth);
                    _db.SaveChanges();
                }
            }        
        }

        public void RemoveImage()
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person person = _db.person.Where(b => b.Id == this.person_id).Select(t => t).FirstOrDefault();
                if (person == null)
                    throw new Exception("A-OK, Handled.");
            
                person.profileimage = null;
                _db.SaveChanges();
            }
        }

        public List<poco_booth> _GetFollowingPOCOs(string per_id)
        {
            if (string.IsNullOrEmpty(per_id))
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                person per = _db.person.Where(p => p.Id == per_id).FirstOrDefault();
                if (per == null)
                    throw new Exception("A-OK, Handled.");
            
                List<booth> fol = GetFollowing(per);
                
                List<poco_booth> res = new List<poco_booth>();
                foreach (booth b in fol)
                {
                    poco_booth poco = new poco_booth();
                    b.person = null;
                    poco.ToPoco(NullHelper.PerNull(b), null);
                    res.Add(poco);
                }
                return res;
            }
        }
    }
}