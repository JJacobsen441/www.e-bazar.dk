using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;
using static www.e_bazar.dk.Models.DTOs.dto_booth;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_booth
    {
        public biz_booth()
        {
        }
        
        public booth GetBooth(int? booth_id, string la_search, string lb_search, bool select_inactive, bool withproducts, bool withcollections, bool overrideonlycollection, bool withlevela, bool withconversations, bool withdefault)
        {
            if (booth_id.IsNull())
                throw new Exception("A-OK, Check");

            if (la_search.IsNull())
                la_search = "";

            if (lb_search.IsNull())
                lb_search = "";

            string _default = withdefault ? "withdefault" : ".ingen";

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * I know virtual should be removed from model entitys, to make it eager load
                 * */

                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                /*
                 * an extension method for combining includes, would be cool
                 * */

                IQueryable<booth> _b = _db.booth
                                .Include("region")
                                .Include("category_main")
                                .Include("person")
                                .Include("product")
                                .Include("product.foldera")
                                .Include("product.folderb")
                                .Include("product.tag")
                                .Include("product.category_main")
                                .Include("product.category_second")
                                .Include("product.image")
                                .Include("collection")
                                .Include("collection.foldera")
                                .Include("collection.folderb")
                                .Include("collection.tag")
                                .Include("collection.category_main")
                                .Include("collection.category_second")
                                .Include("collection.image")
                                .Include("conversation")
                                .Include("foldera")
                                .Where(x => x.Id == booth_id);

                _b.ToTraceStringD();

                booth b = _b.AsEnumerable().FirstOrDefault();

                _b.ToTraceStringD();

                if (b.IsNull())
                    throw new Exception("A-OK, Handled.");
                else
                {
                    /*if (b.product.IsNull()) ;
                    if (b.collection.IsNull()) ;
                    if (b.category_main.IsNull()) ;
                    if (b.region.IsNull()) ;
                    if (b.boothrating.IsNull()) ;
                    if (b.person.IsNull()) ;
                    if (b.conversation.IsNull()) ;
                    if (b.foldera.IsNull()) ;
                    if (b.followers.IsNull()) ;/**/

                    b.product = withproducts ? NullHelper.BthNull(b.product.Where(z => z.booth_id == booth_id && ((z.active || select_inactive) && (!z.only_collection || overrideonlycollection) &&
                                    ((la_search == "" && lb_search == "") ||
                                     (z.foldera != null && la_search != "" && z.foldera.name == la_search && lb_search == "" && z.folderb == null) ||
                                     (z.folderb != null && lb_search != "" && z.folderb.name == lb_search) ||
                                     (z.foldera != null && la_search != "" && z.foldera.name == la_search && lb_search == "")))).ToList(), true) : null;
                    b.collection = withcollections ? NullHelper.BthNull(b.collection.Where(z => z.booth_id == booth_id && ((z.active || select_inactive) &&
                                     ((la_search == "" && lb_search == "") ||
                                      (z.foldera != null && la_search != "" && z.foldera.name == la_search && lb_search == "" && z.folderb == null) ||
                                      (z.folderb != null && lb_search != "" && z.folderb.name == lb_search) ||
                                      (z.foldera != null && la_search != "" && z.foldera.name == la_search && lb_search == "")))).ToList(), true) : null;
                    b.category_main = NullHelper.BthNull(b.category_main.Where(z => z.name != _default).OrderBy(c => c.priority).ToList());
                    b.boothrating = NullHelper.BthNull(b.boothrating.ToList());
                    b.conversation = withconversations ? b.conversation : null;
                    b.foldera = withlevela ? b.foldera : null;
                    b.followers = null;
                    b.region = NullHelper.BthNull(b.region);
                    b.person = NullHelper.BthNull(b.person);
                    //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                
                    return b;
                }
            }
        }

        public dto_booth GetBoothDTO(int? booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withproducts, bool withcollections, bool overrideonlecollection, bool withlevela, bool withconversations, bool withdefault)
        {
            dto_booth dto = new dto_booth();
            booth booth = GetBooth(booth_id, lev_a_search, lev_b_search, select_inactive, withproducts, withcollections, overrideonlecollection, withlevela, withconversations, withdefault);

            RelevantHelper helper = RelevantHelper.Create(true);

            biz_product pro = new biz_product(false);
            dto_product d_pro = new dto_product();
            if (booth.product != null)
            {
                foreach (product p in booth.product)
                    pro.IsRelevant(d_pro, p, null, false, booth.name, helper);
            }

            biz_collection col = new biz_collection();
            dto_collection d_col = new dto_collection();
            if (booth.collection != null)
            {
                foreach (collection c in booth.collection)
                    col.IsRelevant(d_col, null, c, false, booth.name, helper);
            }


            List<Hit> hits = new List<Hit>();
            List<Hit> rel = hits.Concat(d_pro.relevant_hits).ToList();
            rel = rel.Concat(d_col.relevant_hits).ToList();

            dto = this.ToDTO(booth, rel);
            return dto;

        }

        public List<booth> GetNewestBooths(int skip, int take)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * I know virtual should be removed from model entitys, to make it eager load
                 * */

                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<booth> _b = _db.booth
                                            .Include("region")
                                            .Include("boothrating")
                                            .Include("person")
                                            .Where(x=>x.product.Where(p => p.active).Count() > 0 || x.collection.Where(c => c.active).Count() > 0)
                                            .GroupBy(x => x.Id).Select(g => g.FirstOrDefault())
                                            .Include("region")
                                            .Include("boothrating")
                                            .Include("person")
                                            .OrderByDescending(b => b.created_on).Skip(skip).Take(take);

                IEnumerable<booth> booths = _b.AsEnumerable().ToList();

                if (booths.IsNull())
                    throw new Exception("A-OK, Handled.");
            
                if (booths.Any())
                {
                    foreach (booth b in booths)
                    {
                        /*if (b.product.IsNull()) ;
                        if (b.collection.IsNull()) ;
                        if (b.category_main.IsNull()) ;
                        if (b.region.IsNull()) ;
                        if (b.boothrating.IsNull()) ;
                        if (b.person.IsNull()) ;
                        if (b.conversation.IsNull()) ;
                        if (b.foldera.IsNull()) ;
                        if (b.followers.IsNull()) ;/**/

                        b.product = null;
                        b.collection = null;
                        b.category_main = null;
                        b.boothrating = NullHelper.BthNull(b.boothrating.ToList());
                        b.conversation = null;
                        b.foldera = null;
                        b.followers = null;
                        b.region = NullHelper.BthNull(b.region);
                        b.person = NullHelper.BthNull(b.person);
                        //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                    }

                    return booths.ToList();
                }
                return new List<booth>();
            }
        }
        public List<dto_booth> GetNewestBoothDTOs(int skip, int take)
        {
            List<dto_booth> list = new List<dto_booth>();
            List<booth> booths = GetNewestBooths(skip, take);

            list = this.ToDTOList(booths, null);
            return list;
        }

        public List<booth> GetBoothsByPerson(string salesman_id)
        {
            if (string.IsNullOrEmpty(salesman_id))
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * I know virtual should be removed from model entitys, to make it eager load
                 * */

                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<booth> _b = _db.booth
                                                .Include("person")
                                                .Include("boothrating")
                                                .Include("region")
                                                .Where(x => x.person.Id == salesman_id)
                                                .GroupBy(x => x.Id).Select(g => g.FirstOrDefault())
                                                .Include("person")
                                                .Include("boothrating")
                                                .Include("region")
                                                .OrderByDescending(b => b.created_on);

                IEnumerable<booth> booths = _b.AsEnumerable().ToList();

                if (booths.IsNull())
                    throw new Exception("A-OK, Handled.");
            
                if (booths.Any())
                {
                    foreach (booth b in booths)
                    {
                        /*if (b.product.IsNull()) ;
                        if (b.collection.IsNull()) ;
                        if (b.category_main.IsNull()) ;
                        if (b.region.IsNull()) ;
                        if (b.boothrating.IsNull()) ;
                        if (b.person.IsNull()) ;
                        if (b.conversation.IsNull()) ;
                        if (b.foldera.IsNull()) ;
                        if (b.followers.IsNull()) ;/**/

                        b.product = null;
                        b.collection = null;
                        b.category_main = null;
                        b.boothrating = NullHelper.BthNull(b.boothrating.ToList());
                        b.conversation = null;
                        b.foldera = null;
                        b.followers = null;
                        b.region = NullHelper.BthNull(b.region);
                        b.person = NullHelper.BthNull(b.person);
                        //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                    }

                    return booths.ToList();
                }
                return new List<booth>();
            }
        }

        public List<dto_booth> GetBoothsByPersonDTO(string person_id)
        {
            List<booth> booths = GetBoothsByPerson(person_id);
            List<dto_booth> list = this.ToDTOList(booths, null);
            return list;
        }

        public void SetItems(dto_booth dto, bool orderbyrelevance)
        {
            dto.items = dto.product_dtos != null ? dto.items.Concat(dto.product_dtos).ToList() : dto.items;
            dto.items = dto.collection_dtos != null ? dto.items.Concat(dto.collection_dtos).ToList() : dto.items;
            if (orderbyrelevance)
            {
                dto.items = dto.items.OrderByDescending(i => i.created_on).ToList();
                dto.items = dto.items.OrderByDescending(i => i.relevant).ToList();
            }
        }
        
        public List<dto_booth_item> GetRelevantItems(dto_booth dto, int skip, int take)
        {
            if (dto.items.Count() == 0)
                SetItems(dto, true);
            if (take != -1)
                return dto.items.Skip(skip).Take(take).ToList();
            return dto.items.ToList();
        }

        public dto_booth_item GetNewestItem(dto_booth dto)
        {
            if (dto.items.Count() == 0)
                SetItems(dto, true);
            List<dto_booth_item> list = dto.items;
            if (list.Count > 0)
                return list.FirstOrDefault();
            else
                return new dto_booth_item() { created_on = DateTime.MinValue };
        }

        public List<booth> GetBooths(string salesman_id, bool withsalesman)
        {
            if (string.IsNullOrEmpty(salesman_id))
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * I know virtual should be removed from model entitys, to make it eager load
                 * */

                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<booth> _b = _db.booth
                                            .Include("person")
                                            .Include("boothrating")
                                            .Include("region")
                                            .Where(x => x.person_id == salesman_id);

                IEnumerable<booth> booths = _b.AsEnumerable().ToList();

                if (booths == null)
                    throw new Exception("A-OK, Handled.");

                if (booths.Any())
                {
                    foreach (booth b in booths)
                    {
                        /*if (b.product.IsNull()) ;
                        if (b.collection.IsNull()) ;
                        if (b.category_main.IsNull()) ;
                        if (b.region.IsNull()) ;
                        if (b.boothrating.IsNull()) ;
                        if (b.person.IsNull()) ;
                        if (b.conversation.IsNull()) ;
                        if (b.foldera.IsNull()) ;
                        if (b.followers.IsNull()) ;/**/

                        b.product = null;
                        b.collection = null;
                        b.category_main = null;
                        b.boothrating = NullHelper.BthNull(b.boothrating.ToList());
                        b.conversation = null;
                        b.foldera = null;
                        b.followers = null;
                        b.region = NullHelper.BthNull(b.region);
                        b.person = withsalesman ? NullHelper.BthNull(b.person) : null;
                        //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                    }

                    return booths.ToList();
                }
                return new List<booth>();
            }
        }

        public List<dto_booth> GetBoothDTOs(string salesman_id, bool withsalesman)
        {
            List<booth> booths = GetBooths(salesman_id, withsalesman);
            List<dto_booth> list = this.ToDTOList(booths, null);
            return list;
        }

        public void DeleteBooth(int id, EbazarDB _db)
        {
            if (_db == null)
                throw new Exception("A-OK, Handled.");

            booth result = _db.booth.Where(b => b.Id == id).FirstOrDefault();
            if (result != null)
            {
                foreach (category cata in result.category_main.ToList())
                {
                    result.category_main.Remove(cata);
                }
                                                
                foreach (folder a in result.foldera.ToList())
                {
                    foreach (folder b in a.children.ToList())
                    { 
                        _db.folder.Remove(b);
                    }
                    _db.folder.Remove(a);
                }

                /*biz_product product_poco = new biz_product(false);//product/collection allerede fjernet i AdministrationCon?
                foreach (product p in result.product)
                    product_poco.Delete(p.Id, _db);
                
                biz_collection collection_poco = new biz_collection();
                foreach (collection c in result.collection)
                    collection_poco.Delete(c.Id, _db);*/
                
                biz_conversation con_poco = new biz_conversation();
                foreach (conversation c in result.conversation.ToList())
                {
                    foreach (comment com in c.comment.ToList())
                        _db.comment.Remove(com);
                    _db.conversation.Remove(c);
                }
                
                foreach (person p in result.followers.ToList())
                {
                    p.following.Remove(result);
                    result.followers.Remove(p);
                }

                foreach(boothrating br in result.boothrating.ToList())
                {
                    _db.boothrating.Remove(br);
                }

                _db.booth.Remove(result);
            }
        }        

        public void RemoveImage(dto_booth dto) { RemoveBoothImage(dto); }
        private void RemoveBoothImage(dto_booth dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                booth booth = _db.booth.Where(b => b.Id == dto.booth_id).Select(t => t).FirstOrDefault();
                if (booth == null)
                    throw new Exception("A-OK, handled.");

                booth.frontimage = null;
                _db.SaveChanges();
            }
        }

        public List<booth> GetBooths(int skip, int take, out int count, out List<Hit> rel_hits)
        {
            /*
             * when updating this function, remember to update biz_product.cs and biz_collection.cs -> IsRelevant
             * */

            rel_hits = new List<Hit>();
            //using (EbazarDB _db = new EbazarDB())
            //DAL.GetInstance().Dispose();
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                biz_salesman s_poco = new biz_salesman();
                biz_product pr_poco = new biz_product(false);
                biz_collection col_poco = new biz_collection();
                biz_category cat_poco = new biz_category();
                                
                string[] opt, cats;
                string op1, op2, op3, op4, op5, op6, cat;
                int fra, til, zip;
                RelevantHelper helper = RelevantHelper.Create(false);
                helper.GetVals(out opt, out op1, out op2, out op3, out op4, out op5, out op6, out cats, out cat, out fra, out til, out zip);

                bool cat_selected = CategorysHelper.CatsYesNo.Where(x => x.name == cat).Count() > 0;
                string option1 = opt[0], option2 = opt[1], option3 = opt[2];

                bool is_param = _db.param.Where(x => x.name == op2 || x.name == op3 || x.name == op4).Count() > 0 || _db.value.Where(x => x.value1 == op2 || x.value1 == op3 || x.value1 == op4).Count() > 0;

                int ato = 0;
                int afrom = 0;
                bool dk = false;
                foreach (string s in AreasHelper.selected)//allways only holds one entry
                {
                    if (s == "dk")
                        dk = true;
                    if (s == "dk")
                        continue;
                    Area area = AreasHelper.areas.Where(x => x.area.Replace(" ", "").ToLower() == s).FirstOrDefault();
                    if (area.IsNull())
                        throw new Exception();
                    ato = area.to;
                    afrom = area.from;
                    break;
                }

                ;

                /*
                 * where IsRelevantA(b) && ((p != null && p.name != "" && p.active) || (c != null && c.name != "" && c.active)) ||
                 * ((IsRelevantB(b, false) && p != null && p.name != "" && p.active && pro_relevant.IsRelevant(p, false, false, b.name)) ||
                 * (IsRelevantB(b, false) && c != null && c.name != "" && c.active && col_relevant.IsRelevant(c, false, false, b.name)))
                 * 
                 * person = s_poco.GetPerson(b.person_id, "Salesman", false, false, false),
                 * product = b.product.IsNull() && withproducts ? pr_poco.GetProducts(b.Id, "", "", false, false, false, true, false, true) : null,
                 * collection = b.collection.IsNull() && withproducts ? col_poco.GetCollections(b.Id, "", "", false, false, true, false) : null,
                 * */

                _db.Configuration.ProxyCreationEnabled = true;
                _db.Configuration.LazyLoadingEnabled = true;

                IQueryable<booth> _b = (
                         from b in _db.booth
                            .Include("region")
                            .Include("category_main")
                            .Include("boothrating")
                            .Include("product")
                            .Include("collection")
                            .Include("person")
                            
                         where (
                                
                                //search in booth
                                (option1 != "" ?
                                ((b.name.ToLower().Trim() == option1.ToLower().Trim())) ||
                                ((b.name.ToLower().Trim().Contains(option1.ToLower().Trim()))) ||
                                (opt.Where(x => x != "" && b.description.ToLower().Trim().Contains(x)).Count() > 0) : false) &&
                                ((b.product.Count() > 0)) &&
                                ((zip == 0 && dk ? true :
                                zip != 0 && dk ?
                                b.region.zip == zip :
                                (_db.region.Where(x => x.zip == b.region.zip && x.zip <= ato && x.zip >= afrom).Count() == 1))) &&
                                (b.product.Where(x => x != null && x.name != "" && x.active).Count() > 0 || b.collection.Where(x => x != null && x.name != "" && x.active).Count() > 0)) ||
                                //((b.product != null && b.product.Count() > 0 && b.product.Where(x => x.active).Count() > 0) || (b.collection != null && b.collection.Count() > 0 && b.collection.Where(x=>x.active).Count() > 0)) ||


                                //search in product
                                (((cat == "alle" ? true :
                                (cat != "alle" && option1 == "" ? true :
                                (cat != "alle" && option1 != "") ? cat_selected : false)) &&

                                //start product IsRelevant
                                /*
                                 * I omit p.price_int because I havent updated the databese yet. And in the DB they are stored as strings
                                 * */
                                (b.product.Where(p => (
                                    p != null && p.name != "" && p.active && p.booth != null && p.booth.region != null &&

                                    (((p.price == NOP.INGEN_PRIS.ToString()) && (p.active && p.price != null && p.price != "")) || 
                                    ((p.price != NOP.INGEN_PRIS.ToString()) && (p.active && p.price != null && p.price != "" /*&& p.price_int >= fra &&*//* p.price_int <= til*/))) &&
                                 
                                    (cat == "alle" && p.active) || 
                                    (cat != "alle" && cats.Count() == 1 && p.active && cat == p.category_main.name) ||
                                    (cat != "alle" && cats.Count() > 1 && p.active && cat == p.category_main.name && p.category_second != null && cats.Contains(p.category_second.name)) ||

                                    opt.Where(x => x != "" &&
                                   ((p.name.ToLower().Trim().Contains(x) && p.active) ||
                                    (p.tag.Where(t => t.name == x).Count() > 0 && p.active) ||
                                    (p.description.ToLower().Trim().Contains(x) && p.active))).Count() > 0)).Count() > 0) &&

                                (option1 == "" || is_param ||
                                (((opt.Where(x => x != "" && b.product.Where(p => 
                                   ((p.name.ToLower().Trim().Contains(x) && p.active) ||
                                    (p.tag.Where(t => t.name == x).Count() > 0 && p.active) ||
                                    (p.description.ToLower().Trim().Contains(x) && p.active))).Count() > 0)).Count() > 0))) &&

                                (zip == 0 && dk ? 
                                true :
                                zip != 0 && dk ?
                                b.region.zip == zip :
                                _db.region.Where(x => x.zip == b.region.zip && x.zip <= ato && x.zip >= afrom).Count() == 1)) ||
                                //end product IsRelevant




                                //search in collection
                                ((cat == "alle" ? true :
                                (cat != "alle" && option1 == "" ? true :
                                (cat != "alle" && option1 != "") ? cat_selected : false)) &&

                                //start collection IsRelevant
                                /*
                                 * I omit p.joinedprice_int because I havent updated the databese yet. And in the DB they are stored as strings
                                 * */
                                (b.collection.Where(c => (
                                    c != null && c.name != "" && c.active && c.booth != null && c.booth.region != null &&

                                    (((c.joinedprice == NOP.INGEN_PRIS.ToString()) && (c.active && c.joinedprice != null && c.joinedprice != "")) ||
                                    ((c.joinedprice != NOP.INGEN_PRIS.ToString()) && (c.active && c.joinedprice != null && c.joinedprice != "" /*&& p.joinedprice_int >= fra &&*//* p.joinedprice_int <= til*/))) &&

                                    (cat == "alle" && c.active) ||
                                    (cat != "alle" && cats.Count() == 1 && c.active && cat == c.category_main.name) ||
                                    (cat != "alle" && cats.Count() > 1 && c.active && cat == c.category_main.name && c.category_second != null && cats.Contains(c.category_second.name)))).Count() > 0) &&

                                (option1 == "" || is_param ||
                                (((opt.Where(x => x != "" && b.collection.Where(c => 
                                   ((c.name.ToLower().Trim().Contains(x) && c.active) ||
                                    (c.tag.Where(t => t.name == x).Count() > 0 && c.active) ||
                                    (c.description.ToLower().Trim().Contains(x) && c.active))).Count() > 0)).Count() > 0))) &&


                                (zip == 0 && dk ?
                                true :
                                zip != 0 && dk ?
                                b.region.zip == zip :
                                _db.region.Where(x => x.zip == b.region.zip && x.zip <= ato && x.zip >= afrom).Count() == 1)))
                                //end collection IsRelevant

                         orderby b.modified descending
                         group b by b.Id into g
                         select g.FirstOrDefault());

                _b.ToTraceStringD();

                IEnumerable<booth> booths = _b.AsEnumerable().ToList();
                if (booths.IsNull())
                    throw new Exception("A-OK, Handled.");

                ;

                biz_product pro_relevant = new biz_product(false);
                biz_collection col_relevant = new biz_collection();
                dto_product d_pro_relevant = new dto_product();
                dto_collection d_col_relevant = new dto_collection();
                booths = booths.Where(x =>
                x.product.Where(z => pro_relevant.IsRelevant((dto_booth_item)null, z, null, x.name, is_param, helper)).Count() > 0 ||
                x.collection.Where(z => col_relevant.IsRelevant((dto_booth_item)null, null, z, x.name, is_param, helper)).Count() > 0
                ).ToList();

            
                ;

                if (booths.IsNull())
                    throw new Exception("A-OK, Handled.");

                count = booths.Count();

                if (booths.Any())
                {
                    if (skip != -1)
                        booths = booths.Skip(skip).Take(take);

                    //setting IsRelevant
                    foreach(booth __b in booths)
                    {
                        foreach (product pro in __b.product)
                            pro_relevant.IsRelevant(d_pro_relevant, pro, null, false, __b.name, helper);
                        foreach (collection col in __b.collection)
                            pro_relevant.IsRelevant(d_col_relevant, null, col, false, __b.name, helper);
                    }

                    rel_hits = rel_hits.Concat(d_pro_relevant.relevant_hits).ToList();
                    rel_hits = rel_hits.Concat(d_col_relevant.relevant_hits).ToList();


                    /*
                    * HACK - just a precaution
                    * */

                    foreach (booth b in booths)
                    {
                        if (b.product.IsNull()) ;
                        if (b.collection.IsNull()) ;
                        if (b.category_main.IsNull()) ;
                        if (b.region.IsNull()) ;
                        if (b.boothrating.IsNull()) ;
                        if (b.person.IsNull()) ;
                        if (b.conversation.IsNull()) ;
                        if (b.foldera.IsNull()) ;
                        if (b.followers.IsNull()) ;

                        b.product = NullHelper.BthNull(b.product.ToList(), false);
                        b.collection = NullHelper.BthNull(b.collection.ToList(), false);
                        b.category_main = NullHelper.BthNull(b.category_main.ToList());
                        b.boothrating = NullHelper.BthNull(b.boothrating.ToList());
                        b.conversation = NullHelper.BthNull(b.conversation.ToList());
                        b.foldera = NullHelper.BthNull(b.foldera.ToList());
                        b.followers = NullHelper.BthNull(b.followers.ToList());

                        //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                        b.region = NullHelper.BthNull(b.region);
                        b.person = NullHelper.BthNull(b.person);
                    }

                    return booths.ToList();
                }
                return new List<booth>();
            }
        }

        public List<dto_booth> GetBoothDTOs(int skip, int take, out int count)
        {
            List<Hit> rel;
            List<dto_booth> list = new List<dto_booth>();
            List<booth> booths = GetBooths(skip, take, out count, out rel);

            list = this.ToDTOList(booths, rel);
            return list;
        }        

        public void setupaddress(dto_booth dto, booth b)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                if (b == null)
                    throw new Exception("A-OK, Check");

                dto.street_address = !string.IsNullOrEmpty(b.street_address) ? b.street_address : "";

                if (b.region == null)
                {
                    dto.region_dto = (from r in _db.region
                                        where r.zip == 0000 && r.town == "default"
                                        select new dto_region { Id = r.Id, zip = r.zip, town = r.town }).FirstOrDefault();
                }
                else
                {
                    biz_region biz = new biz_region();
                    dto.region_dto = new dto_region();
                    dto.region_dto = biz.ToDTO(b.region);
                }

                dto.country = !string.IsNullOrEmpty(b.country) ? b.country : "Danmark";
                dto.fulladdress = b.fulladdress;
                dto.region_id = dto.region_dto.Id;
            }
        }
        
        public List<booth> ToBoothList(List<dto_booth> booth_pocos, EbazarDB _db)
        {
            if (booth_pocos == null)
                throw new Exception("A-OK, Check.");

            List<booth> list = new List<booth>();
            foreach (dto_booth b in booth_pocos)
            {
                booth booth = this.ToBooth(false, b, _db);
                list.Add(booth);                
            }
            return list;
        }

        public booth ToBooth(bool new_booth, dto_booth dto, EbazarDB _db)
        {
            //DAL.GetInstance().DB = new EbazarDB();

            booth result = new booth();
            if (!new_booth)
                result = _db.booth
                    .Include(x => x.person)
                    .FirstOrDefault(b => b.Id == dto.booth_id);
            if (result == null)
                throw new Exception("A-OK, Handled.");

            result.description = !string.IsNullOrEmpty(dto.description) ? dto.description : "";
            result.frontimage = !string.IsNullOrEmpty(dto.frontimage) ? dto.frontimage : "";
            result.name = !string.IsNullOrEmpty(dto.name) ? dto.name : "";
            result.sysname = !string.IsNullOrEmpty(dto.sysname) ? dto.sysname : "";

            if (new_booth)
            {
                result.person_id = dto.salesman_dto.person_id;
                DateTime now = DateTime.Now;
                result.created_on = now;
                result.modified = now;
            }

            result.searchable = true;

            result.street_address = !string.IsNullOrEmpty(dto.street_address) ? dto.street_address : "";
            result.country = !string.IsNullOrEmpty(dto.country) ? dto.country : "";
            result.fulladdress = dto.fulladdress_str == "Full" ? true : false;
            region region = _db.region.FirstOrDefault(r => r.zip == dto.region_dto.zip && r.town == dto.region_dto.town);
            if (region == null)
                throw new Exception("A-OK, Handled.");
            result.region_id = region.Id;

            return result;
        }        

        public List<dto_booth> ToDTOList(ICollection<booth> booths, List<Hit> rel)
        {
            if (booths == null)
                throw new Exception("A-OK, Check");

            List<dto_booth> list = new List<dto_booth>();
            foreach (booth b in booths.ToList())
            {
                biz_booth biz = new biz_booth();
                dto_booth booth_dto = new dto_booth();
                booth_dto = biz.ToDTO(b, rel);
                list.Add(booth_dto);
            }
            return list;
        }

        public dto_booth ToDTO(booth b, List<Hit> rel)
        {
            dto_booth dto = new dto_booth();
            if (b == null)
                throw new Exception("A-OK, Check");

            dto.booth_id = b.Id;
            dto.created_on = b.created_on.IsNotNull() ? b.created_on : dto.created_on;
            dto.modified = b.modified.IsNotNull() ? b.modified : dto.modified;

            dto.name = b.name.IsNotNull() ? b.name : "";
            dto.sysname = b.sysname.IsNotNull() ? b.sysname : "";
            dto.frontimage = b.frontimage.IsNotNull() ? b.frontimage : "";

            dto.description = !string.IsNullOrEmpty(b.description) ? b.description : TextHelper.GetNopValue(NOP.NO_DESCRIPTION.ToString());

            if (b.boothrating != null)
            {
                dto.numberofratings = (from r in b.boothrating
                                        select r).Count();
                dto.boothrating = (from r in b.boothrating
                                    select (int?)r.rating).ToList().Average();
                if (dto.numberofratings == null)
                    dto.numberofratings = 0;
                if (b.boothrating.Count() == 0)
                {
                    dto.boothrating = 0.0;
                    dto.boothrating_nop = TextHelper.GetNopValue(NOP.NO_RATING.ToString());
                }
            }

            dto.salesman_id = !b.person_id.IsNull() ? b.person_id : dto.salesman_id;

            if (b.person.IsNotNull())
            {
                biz_salesman biz = new biz_salesman();
                
                person per = b.person;
                dto.salesman_dto = biz.ToDTO<dto_salesman>(NullHelper.BthNull(per));
            }

            setupaddress(dto, b);

            biz_product pro_poco = new biz_product(false);
            biz_collection col_poco = new biz_collection();

            biz_folder fld_poco = new biz_folder();
            biz_category cat_poco = new biz_category();

            dto.product_dtos = b.product.IsNotNull() ? pro_poco.ToDTOList(NullHelper.BthNull(b.product.ToList(), false), rel, b.name) : new List<dto_product>();
            dto.collection_dtos = b.collection.IsNotNull() ? col_poco.ToDTOList(NullHelper.BthNull(b.collection.ToList(), false), rel, b.name) : new List<dto_collection>();
            dto.foldera_dtos = b.foldera.IsNotNull() ? fld_poco.ToDTOList(NullHelper.BthNull(b.foldera.ToList())) : new List<dto_folder>();
            dto.category_main = b.category_main.IsNotNull() ? cat_poco.ToPocoList(NullHelper.BthNull(b.category_main/*.Where(x=>x.name != ".ingen")*/.ToList()), false/*used to be true*/, false) : new List<dto_category>();
            dto.hits_items = rel.IsNotNull() ? rel.Where(x => x.booth == dto.name).Count() : dto.hits_items;

            return dto;
        }
    }
}