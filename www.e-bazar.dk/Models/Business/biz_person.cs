using Microsoft.AspNet.Identity;
using PostgreSQL.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Models.Identity;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public abstract class biz_person
    {
        protected ApplicationDbContext ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> UserManager { get; set; }
        
        public biz_person()
        {
            //this.db = new EbazarDB();
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
        }        

        public bool IsType<T>(Type t)
        {
            if(typeof(T) == t)
                return true;
            return false;
        }
        
        //public abstract T GetPersonDTO<T>(string person_id/*, string descriminator*/, bool withbooth, bool withfavorites, bool withfollowing) where T : dto_person, new();
        public abstract void SavePerson<T>(T dto) where T : dto_person, new();
        public abstract void UpdatePerson<T>(T dto) where T : dto_person, new();
        public abstract T ToDTO<T>(person per) where T : dto_person, new();

        public class SampleObjectComparer : IComparer<dto_booth_item>
        {
            public int Compare(dto_booth_item x, dto_booth_item y)
            {
                return GetHashCode(x).CompareTo(GetHashCode(x));
            }

            public bool Equals(dto_booth_item x, dto_booth_item y)
            {
                return x.GetType() == typeof(biz_product) && y.GetType() == typeof(biz_product);
            }

            public int GetHashCode(dto_booth_item x)
            {
                return x.GetType().GetHashCode();
            }
        }

        public List<dto_booth_item> _GetFavorites(dto_person dto)
        {
            var comparer = new SampleObjectComparer();

            List<dto_booth_item> items = new List<dto_booth_item>();
            
            foreach (dto_product _dto in dto.favorites_product)
            {
                biz_product biz = new biz_product();
                dto_booth b = _dto.booth_dto;

                if (b == null)
                    b = biz.GetBoothDTO<dto_product>(_dto);
                dto_booth_item i = _dto;
                i.booth_dto = b;
                items.Add(i);
            }

            foreach (dto_collection _dto in dto.favorites_collection)
            {
                biz_collection biz = new biz_collection();
                dto_booth b = _dto.booth_dto;

                if (b == null)
                    b = biz.GetBoothDTO<dto_collection>(_dto);
                dto_booth_item i = _dto;
                i.booth_dto = b;
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
                dto_category o;
                if (!pro.active)
                    continue;
                else if
                    (CategorysHelper.ListContains(CategorysHelper.CatsNoYes, pro.category_main_id, out o) && o.name == ".ingen" ||
                     CategorysHelper.ListContains(CategorysHelper.CatsNoYes, pro.category_second_id, out o) && o.name == "..ingen")
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
                dto_category o;
                if (!col.active)
                    continue;
                else if
                    (CategorysHelper.ListContains(CategorysHelper.CatsNoYes, col.category_main_id, out o) && o.name == ".ingen" ||
                     CategorysHelper.ListContains(CategorysHelper.CatsNoYes, col.category_second_id, out o) && o.name == "..ingen")
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
        
        public void AddFavorite(string per_id, long product_id, int collection_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person per = _db.person.Where(p => p.Id == per_id).FirstOrDefault();
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

        public void RemoveFavorite(string per_id, long product_id, int collection_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person per = _db.person.Where(p => p.Id == per_id).FirstOrDefault();
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

        public void AddFollowing(string per_id, int booth_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person per = _db.person.Where(p => p.Id == per_id).FirstOrDefault();
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

        public void RemoveFollowing(string per_id, int booth_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person per = _db.person.Where(p => p.Id == per_id).FirstOrDefault();
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

        public void RemoveImage<T>(T dto) where T : dto_person
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                person person = _db.person.Where(b => b.Id == dto.person_id).Select(t => t).FirstOrDefault();
                if (person == null)
                    throw new Exception("A-OK, Handled.");
            
                person.profileimage = null;
                _db.SaveChanges();
            }
        }

        public List<dto_booth> _GetFollowingDTOs(string per_id)
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
                
                List<dto_booth> res = new List<dto_booth>();
                foreach (booth b in fol)
                {
                    biz_booth biz = new biz_booth();
                    dto_booth dto = new dto_booth();
                    b.person = null;
                    dto = biz.ToDTO(NullHelper.PerNull(b), null);
                    res.Add(dto);
                }
                return res;
            }
        }

        public person GetPerson(string person_id/*, string descriminator*/, bool withbooth, bool withfavorites, bool withfollowing)
        {
            if (person_id == null)// ikke IsNullOrEmpty da user id fra identity kan være tom streng ved UnAuthorized
                throw new Exception("biz_salesman > GetPerson > person_id NULL [Check DB]");
            if (person_id == "")
                return null;
            //if (descriminator != "Salesman")
            //    return null;

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
                    .Where(x => x.Id == person_id/* && x.descriminator == descriminator*/);

                person p = _p.AsEnumerable().FirstOrDefault();

                if (p == null)
                    throw new Exception("A-OK, Handled.");

                NullHelper.PerNull(p, withbooth/* && descriminator == "Salesman"*/, withfavorites, withfollowing);
                if (withfollowing) NullHelper.PerNull(p.following.ToList());
                if (withfavorites) NullHelper.PerNull(p.favorites_product.ToList(), true);
                if (withfavorites) NullHelper.PerNull(p.favorites_collection.ToList(), true);

                return p;
            }
        }

        public T GetPersonDTO<T>(string person_id/*, string descriminator*/, bool withbooth, bool withfavorites, bool withfollowing) where T : dto_person, new()
        {
            T person = new T();
            person pers = GetPerson(person_id/*, descriminator*/, withbooth, withfavorites, withfollowing);

            person = this.ToDTO<T>(pers);
            return person;
        }
    }
}