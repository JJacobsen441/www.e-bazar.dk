using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_category
    {
        public static bool update = true;
        static List<dto_category> nn = new List<dto_category>();
        static List<dto_category> nw = new List<dto_category>();
        static List<dto_category> wn = new List<dto_category>();
        static List<dto_category> ww = new List<dto_category>();

        private List<dto_category> proxy(bool withchildren, bool withdefault) 
        {
            using (EbazarDB _db = new EbazarDB())
            {


                List<dto_category> list = new List<dto_category>();
                biz_category biz = new biz_category();
                dto_category dto = new dto_category();
                foreach (category c in _db.category.ToList())
                {
                    if ((c.is_parent && c.name != ".ingen") || (c.is_parent && c.name == ".ingen" && withdefault))
                    {
                        dto = biz.ToDTO(c, withchildren, true);
                        list.Add(dto);
                    }
                }
                return list.OrderBy(c=>c.priority).ToList();
            }
        }
        
        public List<dto_category> _GetAll(bool withchildren, bool withdefault = false)
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

        /*public List<biz_category> _GetAllBoothPOCO(int booth_id, bool onlytop)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                List<biz_category> list = new List<biz_category>();
                biz_category poco = new biz_category();
                booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
                if (booth == null)
                    throw new Exception("A-OK, Handled.");
            
                foreach (category c in booth.category_main.ToList())
                {
                    poco = new biz_category();
                    poco.ToPOCO(c, !onlytop, false);
                    list.Add(poco);
                }
                return list.OrderBy(c => c.priority).ToList();
            }
        }/**/

        public List<dto_category> ToPocoList(ICollection<category> cats, bool withchildren, bool withparam)
        {
            if (cats == null)
                throw new Exception("A-OK, Check");

            List<dto_category> list = new List<dto_category>();
            foreach (category c in cats.ToList())
            {
                //if(withchildren)
                //    c.children = Categorys.
                biz_category biz = new biz_category();
                dto_category dto = new dto_category();
                dto = biz.ToDTO(c, withchildren, withparam);
                list.Add(dto);
            }
            return list;
        }/**/        
        
        public dto_category ToDTO(category cat, bool withchildren, bool withparam)
        {
            
            if (cat == null)
                throw new Exception("A-OK, Check.");

            dto_category dto = new dto_category();

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto.category_id = cat.Id;
                dto.name = cat.name;
                dto.is_parent = cat.is_parent;
                dto.priority = cat.priority;
                dto.description = cat.description;
                dto.parent_id = cat.parent_id;
                if (dto.is_parent)
                {
                    RelevantHelper helper = RelevantHelper.Create(false);
                    biz_product dao_pro = new biz_product();
                    biz_collection dao_col = new biz_collection();
                    List<booth> shops = _db.booth
                        .ToList()
                        .Where(s => (s.product != null &&
                            s.product.ToList().Where(p => (p.booth != null &&
                            p.category_main_id == (int)cat.Id &&
                            cat.is_parent &&
                            dao_pro.IsRelevant((dto_booth_item)null, p, null, true, s.name, helper))).Count() > 0) ||
                            s.collection != null && (s.collection.ToList().Where(c => (c.booth != null &&
                            c.category_main_id == (int)cat.Id &&
                            cat.is_parent &&
                            dao_col.IsRelevant((dto_booth_item)null, null, c, true, s.name, helper))).Count() > 0))
                        .Distinct().ToList();

                    dto.booths_with_category_count = shops.Count();
                }
                else
                {
                    if (cat.parent == null)
                        throw new Exception("dao_category.ToDao > cat.Parent NULL");
                    RelevantHelper helper = RelevantHelper.Create(false);
                    biz_product dao_pro = new biz_product();
                    biz_collection dao_col = new biz_collection();
                    List<booth> shops = _db.booth
                        .ToList()
                        .Where(s =>
                            (s.product != null && s.product.ToList().Where(p => (p.booth != null &&
                            p.category_main_id == (int)cat.parent.Id &&
                            (int)p.category_second_id == cat.Id &&
                            dao_pro.IsRelevant((dto_booth_item)null, p, null, true, s.name, helper))).Count() > 0) ||
                            (s.collection != null && s.collection.ToList().Where(c => c.booth != null &&
                            c.category_main_id == (int)cat.parent.Id &&
                            (int)c.category_second_id == cat.Id &&
                            dao_col.IsRelevant((dto_booth_item)null, null, c, true, s.name, helper)).Count() > 0))
                        .Distinct().ToList();

                    dto.booths_with_category_count = shops.Count();
                }
                if (withchildren)
                {
                    dto.children = new List<dto_category>();
                    foreach (category c in cat.children)
                    {
                        biz_params params_biz = new biz_params();
                        biz_category biz_cat = new biz_category();
                        dto_category dto1 = new dto_category();
                        dto1 = biz_cat.ToDTO(c, false, false);
                    
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

                        List<dto_params> list1 = params_biz.ToDTO_List(_p);
                        List<dto_params> list2 = params_biz.ToDTO_List(_c.param.ToList());
                    
                        list1.AddRange(list2);
                        dto1.params_dto = list1;
                        dto.children.Add(dto1);
                    }
                    dto.children = dto.children.OrderBy(c => c.priority).ToList();
                }
                else
                    dto.children = new List<dto_category>();

                dto_category dto2 = new dto_category();
                biz_category biz = new biz_category();
                if (cat.parent != null)
                {
                    dto2 = biz.ToDTO(cat.parent, false, false);
                    dto.parent = dto2;
                }            

                if (cat.param != null && withparam)
                {
                    biz_params params_dao = new biz_params();
                    dto.params_dto = params_dao.ToDTO_List(cat.param.ToList());
                }
            }

            return dto;
        }
    }
}