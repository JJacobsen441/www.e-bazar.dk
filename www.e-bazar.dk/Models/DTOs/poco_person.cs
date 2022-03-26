using Microsoft.AspNet.Identity;
using PostgreSQL.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.Identity;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public abstract class poco_person
    {
        //protected EbazarDB db;
        protected ApplicationDbContext ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> UserManager { get; set; }
        /*protected poco_person()
        {
            this.db = new EbazarDB();
        }*/
        public poco_person()
        {
            //this.db = new EbazarDB();
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
        }
        /*~poco_person()
        {
            db?.Dispose();
        }*/

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

        //public bool is_salesman { get; set; }
        public bool IsType<T>()
        {
            if(typeof(T) == this.GetType())
                return true;
            return false;
        }
        
        public abstract T GetPersonPOCO<T>(string person_id, string descriminator, bool withbooth, bool withfavorites, bool withfollowing) where T : poco_person, new();
        //public abstract List<T> _GetPersonsPOCO<T>(bool withfavorites, bool withfollowing) where T : poco_person, new();
        public abstract void SavePerson<T>() where T : poco_person, new();
        public abstract void UpdatePerson<T>() where T : poco_person, new();
        //public abstract List<T> ToPocoList<T>(ICollection<person> persons) where T : poco_person, new();
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
            //items = favorites_product != null ? items.Concat(favorites_product).ToList() : items;
            //items = favorites_collection != null ? items.Concat(favorites_collection).ToList() : items;

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
            //if (per.favorites_product == null)
            //    return fav;

            foreach (product pro in per.favorites_product)
            {
                poco_category o;
                if (!pro.active)
                    continue;
                //else if (pro.category_main.name == ".ingen" || pro.category_second.name == "..ingen")
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
            //if (per.favorites_collection == null)
            //    return fav;

            foreach (collection col in per.favorites_collection)
            {
                poco_category o;
                if (!col.active)
                    continue;
                //else if (col.category_main.name == ".ingen" || col.category_second.name == "..ingen")
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
            //if (per.following == null)
            //    return fol;

            foreach (booth b in per.following)
            {
                
                //booth boo = new booth();
                //boo = poco.GetBooth(b.Id, "", "", false, true, true, true, false, false, false);
                //if (boo == null)
                //    continue;
                fol.Add(b);
            }
            return fol;
        }

        
        public void AddFavorite(long product_id, int collection_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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
        public void RemoveFavorite(long product_id, int collection_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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

        public void AddFollowing(int booth_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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
        public void RemoveFollowing(int booth_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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

        public void RemoveImage()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            person person = _db.person.Where(b => b.Id == this.person_id).Select(t => t).FirstOrDefault();
            if (person == null)
                throw new Exception("A-OK, Handled.");
            
            person.profileimage = null;
            _db.SaveChanges();
        }

        protected List<product> Null(List<product> p, bool nullbooth)
        {
            List<product> list = new List<product>();
            foreach (product pro in p)
            {
                if (nullbooth)
                    pro.booth = null;
                if (pro.booth != null)
                    pro.booth = Null(pro.booth);
                if (pro.category_main != null)
                    pro.category_main = null;
                if (pro.category_second != null)
                    pro.category_second = null;
                if (pro.collection != null)
                    pro.collection = null;
                if (pro.conversation != null)
                    pro.conversation = null;
                if (pro.favorites != null)
                    pro.favorites = null;
                if (pro.foldera != null)
                    pro.foldera = null;
                if (pro.folderb != null)
                    pro.folderb = null;
                if (pro.image != null)
                    pro.image = null;
                if (pro.product_param != null)
                    pro.product_param = null;
                if (pro.tag != null)
                    pro.tag = null;
                list.Add(pro);
            }
            return list;
        }
        protected List<collection> Null(List<collection> c, bool nullbooth)
        {
            List<collection> list = new List<collection>();
            foreach (collection col in c)
            {
                if (nullbooth)
                    col.booth = null;
                if (col.booth != null)
                    col.booth = Null(col.booth);
                if (col.category_main != null)
                    col.category_main = null;
                if (col.category_second != null)
                    col.category_second = null;
                if (col.product != null)
                    col.product = null;
                if (col.conversation != null)
                    col.conversation = null;
                if (col.favorites != null)
                    col.favorites = null;
                if (col.foldera != null)
                    col.foldera = null;
                if (col.folderb != null)
                    col.folderb = null;
                if (col.image != null)
                    col.image = null;
                if (col.collection_param != null)
                    col.collection_param = null;
                if(col.tag != null)
                    col.tag = null;
                list.Add(col);
            }
            return list;
        }
        protected List<booth> Null(List<booth> b)
        {
            List<booth> list = new List<booth>();
            foreach (booth bth in b)
            {
                list.Add(Null(bth));
            }
            return list;
        }
        protected booth Null(booth bth)
        {
            if (bth.boothrating != null)
                bth.boothrating = null;
            if (bth.category_main != null)
                bth.category_main = null;
            if (bth.collection != null)
                bth.collection = null;
            if (bth.conversation != null)
                bth.conversation = null;
            if (bth.foldera != null)
                bth.foldera = null;
            if (bth.followers != null)
                bth.followers = null;
            if (bth.person != null)
                bth.person = null;// Null(b.person);
            if (bth.product != null)
                bth.product = null;
            if (bth.region != null)
                bth.region = null;

            return bth;
        }
        public List<poco_booth> _GetFollowingPOCOs(string per_id)
        {
            if (string.IsNullOrEmpty(per_id))
                throw new Exception("A-OK, Check.");

            EbazarDB _db = DAL.GetInstance().GetContext();
            
            person per = _db.person.Where(p => p.Id == per_id).FirstOrDefault();
            if (per == null)
                throw new Exception("A-OK, Handled.");
            
            List<booth> fol = GetFollowing(per);
            //if (fol == null)
            //    return new List<poco_booth>();
            List<poco_booth> res = new List<poco_booth>();
            foreach (booth b in fol)
            {
                poco_booth poco = new poco_booth();
                b.person = null;
                poco.ToPoco(Null(b), null);
                res.Add(poco);
            }
            return res;
        }
    }
}