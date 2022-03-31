using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using www.e_bazar.dk.Extensions;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_category
    {
        //private EbazarDB db;
        /*private poco_category()
        {

        }*/
        public poco_category()
        {
            //this.db = new EbazarDB();
        }
        /*~poco_category()
        {
            db?.Dispose();
        }*/

        public int category_id { get; set; }
        [Required]
        [StringLength(50)]
        public string name { get; set; }
        public bool is_parent { get; set; }
        public int priority { get; set; }
        [StringLength(100)]
        public string description { get; set; }
        public int? parent_id { get; set; }
        public int booths_with_category_count { get; set; }
        public virtual List<poco_category> children { get; set; }
        public virtual poco_category parent { get; set; }
        public virtual List<poco_booth> booth { get; set; }
        public virtual List<poco_params> params_dao { get; set; }

        public static bool update = true;
        static List<poco_category> nn = new List<poco_category>();
        static List<poco_category> nw = new List<poco_category>();
        static List<poco_category> wn = new List<poco_category>();
        static List<poco_category> ww = new List<poco_category>();

        private List<poco_category> proxy(bool withchildren, bool withdefault) 
        {
            using (EbazarDB _db = new EbazarDB())
            {


                List<poco_category> list = new List<poco_category>();
                poco_category poco = new poco_category();
                foreach (category c in _db.category.ToList())
                {
                    if ((c.is_parent && c.name != ".ingen") || (c.is_parent && c.name == ".ingen" && withdefault))
                    {
                        poco = new poco_category();
                        poco.ToPOCO(c, withchildren, true);
                        list.Add(poco);
                    }
                }
                return list.OrderBy(c=>c.priority).ToList();
            }
        }
        
        public List<poco_category> _GetAll(bool withchildren, bool withdefault = false)
        {
            if (update)
                nn = proxy(false, false);
            if (update)
                nw = proxy(false, true);
            if (update)
                wn = proxy(true, false);
            if (update)
                ww = proxy(true, true);

            update = false;
            if (!withchildren && !withdefault)
                return nn;
            else if (!withchildren && withdefault)
                return nw;
            else if (withchildren && !withdefault)
                return wn;
            return ww;
        }/**/

        /*public List<category> GetAllBoothParent(booth b, bool withdefault)
        {
            if (b == null)
                throw new Exception("A-OK, Check.");

            string _default = withdefault ? "withdefault" : ".ingen";

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                List<category> list = new List<category>();
                if (b.category_main == null)
                    list = _db.category.Where(x=>x.is_parent && x.booth.Where(z=>z.Id == b.Id).Count() == 1 && x.name != _default).OrderBy(c => c.priority).ToList();
                else
                    list = b.category_main.Where(x=>x.name != _default).OrderBy(c => c.priority).ToList();

                if (list.IsNull())
                    throw new Exception();

                return list;
            }
        }/**/

        /*public List<poco_category> _GetAllBoothPOCO(int booth_id, bool onlytop)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                List<poco_category> list = new List<poco_category>();
                poco_category poco = new poco_category();
                booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
                if (booth == null)
                    throw new Exception("A-OK, Handled.");
            
                foreach (category c in booth.category_main.ToList())
                {
                    poco = new poco_category();
                    poco.ToPOCO(c, !onlytop, false);
                    list.Add(poco);
                }
                return list.OrderBy(c => c.priority).ToList();
            }
        }/**/

        public List<poco_category> ToPocoList(ICollection<category> cats, bool withchildren, bool withparam)
        {
            if (cats == null)
                throw new Exception("A-OK, Check");

            List<poco_category> list = new List<poco_category>();
            foreach (category c in cats.ToList())
            {
                //if(withchildren)
                //    c.children = Categorys.
                poco_category cat_poco = new poco_category();
                cat_poco.ToPOCO(c, withchildren, withparam);
                list.Add(cat_poco);
            }
            return list;
        }/**/        
        
        public void ToPOCO(category cat, bool withchildren, bool withparam)
        {
            
            if (cat == null)
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                this.category_id = cat.Id;
                this.name = cat.name;
                this.is_parent = cat.is_parent;
                this.priority = cat.priority;
                this.description = cat.description;
                this.parent_id = cat.parent_id;
                if (this.is_parent)
                {
                    RelevantHelper helper = RelevantHelper.Create(false);
                    poco_product dao_pro = new poco_product();
                    poco_collection dao_col = new poco_collection();
                    List<booth> shops = _db.booth
                        .ToList()
                        .Where(s => (s.product != null &&
                            s.product.ToList().Where(p => (p.booth != null &&
                            p.category_main_id == (int)cat.Id &&
                            cat.is_parent &&
                            dao_pro.IsRelevant(p, null, true, s.name, helper))).Count() > 0) ||
                            s.collection != null && (s.collection.ToList().Where(c => (c.booth != null &&
                            c.category_main_id == (int)cat.Id &&
                            cat.is_parent &&
                            dao_col.IsRelevant(null, c, true, s.name, helper))).Count() > 0))
                        .Distinct().ToList();

                    this.booths_with_category_count = shops.Count();
                }
                else
                {
                    if (cat.parent == null)
                        throw new Exception("dao_category.ToDao > cat.Parent NULL");
                    RelevantHelper helper = RelevantHelper.Create(false);
                    poco_product dao_pro = new poco_product();
                    poco_collection dao_col = new poco_collection();
                    List<booth> shops = _db.booth
                        .ToList()
                        .Where(s =>
                            (s.product != null && s.product.ToList().Where(p => (p.booth != null &&
                            p.category_main_id == (int)cat.parent.Id &&
                            (int)p.category_second_id == cat.Id &&
                            dao_pro.IsRelevant(p, null, true, s.name, helper))).Count() > 0) ||
                            (s.collection != null && s.collection.ToList().Where(c => c.booth != null &&
                            c.category_main_id == (int)cat.parent.Id &&
                            (int)c.category_second_id == cat.Id &&
                            dao_col.IsRelevant(null, c, true, s.name, helper)).Count() > 0))
                        .Distinct().ToList();

                    this.booths_with_category_count = shops.Count();
                }
                if (withchildren)
                {
                    this.children = new List<poco_category>();
                    foreach (category c in cat.children)
                    {
                        poco_params params_dao = new poco_params();
                        poco_category dao1 = new poco_category();
                        dao1.ToPOCO(c, false, false);
                    
                        List<param> _p = _db.param
                            .Include("value1")
                            .Include("category")
                            .Where(x => x.category_id == c.Id)
                            .ToList();
                        if (_p == null)
                            throw new Exception("A-OK, Handled.");
                    
                        category _c = _db.category
                            .Include("param")
                            .Where(x => x.Id == c.parent.Id)
                            .FirstOrDefault();
                        if (_c == null)
                            throw new Exception("A-OK, Handled.");

                        List<poco_params> list1 = params_dao.ToPOCO_List(_p);
                        List<poco_params> list2 = params_dao.ToPOCO_List(_c.param.ToList());
                    
                        list1.AddRange(list2);
                        dao1.params_dao = list1;
                        this.children.Add(dao1);
                    }
                    this.children = this.children.OrderBy(c => c.priority).ToList();
                }
                else
                    this.children = new List<poco_category>();

                poco_category dao2 = new poco_category();
                if (cat.parent != null)
                {
                    dao2.ToPOCO(cat.parent, false, false);
                    this.parent = dao2;
                }            

                if (cat.param != null && withparam)
                {
                    poco_params params_dao = new poco_params();
                    this.params_dao = params_dao.ToPOCO_List(cat.param.ToList());
                }
            }
        }
    }
}