using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data.Entity;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.SharedClasses;
//using static www.e_bazar.dk.SharedClasses.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_booth
    {
        public poco_booth()
        {
        }
        
        public int booth_id { get; set; }
        [DisplayName("Stand Navn")]
        [Required]
        [StringLength(50)]
        public string name { get; set; }
        [Required]
        [StringLength(100)]
        public string sysname { get; set; }
        [DisplayName("Oprettet")]
        public DateTime created_on { get; set; }
        public DateTime modified { get; set; }
        [DisplayName("Stand Beskrivelse")]
        [StringLength(500)]
        public string description { get; set; }
        [DisplayName("Stand Beskrivelse")]
        public string description_limit
        {
            get
            {
                if (string.IsNullOrEmpty(description))
                    return "";
                int len = description.Length < 230 ? description.Length : 230;

                return len < 230 ? description : (description.Substring(0, len) + "...");
            }
            set
            {
                description = value;
            }
        }

        [DisplayName("Stand Logo")]
        [StringLength(80)]
        public string frontimage { get; set; }
        [DisplayName("Antal vurderinger")]
        public int? numberofratings { get; set; }
        [DisplayName("Stand vurdering")]
        public double? boothrating { get; set; }
        [DisplayName("Stand vurdering")]
        public string boothrating_nop { get; set; }
        [StringLength(128)]
        public string salesman_id { get; set; }
        public poco_salesman salesman_poco { get; set; }

        public List<poco_product> product_pocos { get; set; }
        public List<poco_collection> collection_pocos { get; set; }

        [Required]
        [DisplayName("Gade navn")]
        [StringLength(50)]
        public string street_address { get; set; }

        [Required]
        [DisplayName("Land")]
        [StringLength(20)]
        public string country { get; set; }
        public bool fulladdress { get; set; }
        public string fulladdress_str { get; set; }
        public int region_id { get; set; }
        public virtual poco_region region_poco { get; set; }
        public virtual List<poco_conversation> conversation_pocos { get; set; }
        public virtual List<poco_folder> foldera_pocos { get; set; }
        public virtual List<poco_category> category_main { get; set; }

        public int hits_items { get; set; }
        public class Hit { public string booth; public string product; }
        private List<IBoothItem> items = new List<IBoothItem>();

        public bool relevant { get; set; }

        public booth GetBooth(int? booth_id, string la_search, string lb_search, bool select_inactive, bool withproducts, bool withcollections, bool overrideonlycollection, bool withlevela, bool withconversations, bool withdefault)
        {
            if (booth_id.IsNull())
                throw new Exception("A-OK, Check");

            if (la_search.IsNull())
                la_search = "";

            if (lb_search.IsNull())
                lb_search = "";

            string _default = withdefault ? "withdefault" : ".ingen";

            EbazarDB _db = DAL.GetInstance().GetContext();
            
            _db.Configuration.ProxyCreationEnabled = false;
            _db.Configuration.LazyLoadingEnabled = false;

            /*
             * en extension method til at combine includes, kunne være smart
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

            booth b = _b.AsEnumerable().FirstOrDefault();
            
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

                b.product = withproducts ? NullHelper.BNull(b.product.Where(z => z.booth_id == booth_id && ((z.active || select_inactive) && (!z.only_collection || overrideonlycollection) &&
                                ((la_search == "" && lb_search == "") ||
                                 (z.foldera != null && la_search != "" && z.foldera.name == la_search && lb_search == "" && z.folderb == null) ||
                                 (z.folderb != null && lb_search != "" && z.folderb.name == lb_search) ||
                                 (z.foldera != null && la_search != "" && z.foldera.name == la_search && lb_search == "")))).ToList(), true) : null;
                b.collection = withcollections ? NullHelper.BNull(b.collection.Where(z => z.booth_id == booth_id && ((z.active || select_inactive) &&
                                 ((la_search == "" && lb_search == "") ||
                                  (z.foldera != null && la_search != "" && z.foldera.name == la_search && lb_search == "" && z.folderb == null) ||
                                  (z.folderb != null && lb_search != "" && z.folderb.name == lb_search) ||
                                  (z.foldera != null && la_search != "" && z.foldera.name == la_search && lb_search == "")))).ToList(), true) : null;
                b.category_main = NullHelper.BNull(b.category_main.Where(z => z.name != _default).OrderBy(c => c.priority).ToList());
                b.boothrating = NullHelper.BNull(b.boothrating.ToList());
                b.conversation = withconversations ? b.conversation : null;
                b.foldera = withlevela ? b.foldera : null;
                b.followers = null;
                b.region = NullHelper.BNull(b.region);
                b.person = NullHelper.BNull(b.person);
                //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                
                return b;
            }
        }

        public poco_booth GetBoothPOCO(int? booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withproducts, bool withcollections, bool overrideonlecollection, bool withlevela, bool withconversations, bool withdefault)
        {
            poco_booth poco = new poco_booth();
            booth booth = GetBooth(booth_id, lev_a_search, lev_b_search, select_inactive, withproducts, withcollections, overrideonlecollection, withlevela, withconversations, withdefault);

            RelevantHelper helper = RelevantHelper.Create(false);
            
            poco_product pro = new poco_product(false);
            if (booth.product != null)
            {
                foreach (product p in booth.product)
                    pro.IsRelevant(p, null, false, booth.name, helper);
            }

            poco_collection col = new poco_collection();
            if (booth.collection != null)
            {
                foreach (collection c in booth.collection)
                    col.IsRelevant(null, c, false, booth.name, helper);
            }


            List<Hit> hits = new List<Hit>();
            List<Hit> rel = hits.Concat(pro.relevant_hits).ToList();
            rel = rel.Concat(col.relevant_hits).ToList();

            poco.ToPoco(booth, rel);
            return poco;

        }

        //public booth GetBoothByProductId(long product_id)
        //{
        //    EbazarDB _db = DAL.GetInstance().GetContext();
        //    poco_salesman salesman_poco = new poco_salesman();
        //    poco_category cat_poco = new poco_category();
        //    booth booth = (from b in _db.booth
        //                   .Include("category_main")
        //                   .Include("region")
        //                       //join s in db.salesman on b.salesman_id equals s.salesman_id
        //                   where b.product.Contains(_db.product.Where(p => p.Id == product_id).FirstOrDefault())
        //                   //where b.Id == booth_id
        //                   select new
        //                   {
        //                       Id = b.Id,
        //                       name = b.name,
        //                       sysname = b.sysname,
        //                       created_on = b.created_on,
        //                       modified = b.modified,
        //                       description = b.description,
        //                       frontimage = b.frontimage,
        //                       numberofratings = b.boothrating.ToList().Count(),
        //                       boothrating = b.boothrating.ToList(),

        //                       person_id = b.person_id,
        //                       //person = b.person,//sm_poco.GetPersonPOCO<poco_salesman>(b.salesman_id, false, false, false),
        //                       //product_dtos = pr_dto.GetProductDTOs(b.booth_id),
        //                       //tag_pocos = pt_poco.GetBoothTagPOCOs(b.booth_id),
        //                       street_address = b.street_address,
        //                       country = b.country,
        //                       fulladdress = b.fulladdress,
        //                       region_id = b.region_id,
        //                       region = b.region,//,
        //                       //category_main = b.category_main.ToList()//cat_poco.GetAllBooth(b.booth_id, false)
        //                       //category_main = cat_poco.GetAllBoothParent(b, true)
        //                       category_main = b.category_main
        //                   }).AsEnumerable()/**/
        //                   //select b).AsEnumerable()
        //                   .Select(b => new booth
        //                   {
        //                       Id = b.Id,
        //                       name = b.name,
        //                       sysname = b.sysname,
        //                       created_on = b.created_on,
        //                       modified = b.modified,
        //                       description = b.description,
        //                       frontimage = b.frontimage,
        //                       numberofratings = b.boothrating.ToList().Count(),
        //                       boothrating = b.boothrating.ToList(),

        //                       person_id = b.person_id,
        //                       person = salesman_poco.GetPerson(b.person_id, "Salesman", false, false, false),
        //                       //product_dtos = pr_dto.GetProductDTOs(b.booth_id),
        //                       //tag_pocos = pt_poco.GetBoothTagPOCOs(b.booth_id),
        //                       street_address = b.street_address,
        //                       country = b.country,
        //                       fulladdress = b.fulladdress,
        //                       region_id = b.region_id,
        //                       region = b.region,
        //                       //category_main = b.category_main.ToList()
        //                       category_main = cat_poco.GetAllBoothParent(new booth() { category_main = b.category_main }, true)//; //b.category_main
        //                   }).FirstOrDefault();

        //    //booth_poco.SetupBoothToClient(db);
        //    if (booth == null)
        //        throw new Exception("A-OK, Handled.");
        //    //booth.category_main = cat_poco.GetAllBoothParent(booth, true);
        //    return booth;
        //}
        //public poco_booth GetBoothPOCOByProductId(long product_id)
        //{
        //    poco_booth booth_poco = new poco_booth();
        //    booth booth = GetBoothByProductId(product_id);

        //    booth_poco.ToPoco(booth, null);
        //    //booth_poco.SetupBoothToClient(db);
        //    return booth_poco;

        //}

        //public booth GetBoothByCollectionId(long collection_id, bool withsalesman)
        //{
        //    /*EbazarDB _db = DAL.GetInstance().GetContext();
        //    poco_product pr_poco = new poco_product(false);
        //    poco_salesman s_poco = new poco_salesman();
        //    poco_category cat_poco = new poco_category();
        //    booth booth = (from b in _db.booth
        //                   .Include("category_main")
        //                   .Include("region")
        //                       //.Include("products")
        //                       //join s in db.salesman on b.salesman_id equals s.salesman_id
        //                   where b.collection.Contains(_db.collection.Where(c => c.Id == collection_id).FirstOrDefault())
        //                   //where b.Id == booth_id
        //                   select new
        //                   {
        //                       Id = b.Id,
        //                       name = b.name,
        //                       sysname = b.sysname,
        //                       created_on = b.created_on,
        //                       modified = b.modified,
        //                       description = b.description,
        //                       frontimage = b.frontimage,
        //                       boothrating = b.boothrating,

        //                       //person_id = b.person_id,
        //                       person_id = b.person_id,
        //                       //person = b.person,//sm_poco.GetPersonPOCO<poco_salesman>(b.salesman_id, false, false, false),
        //                       //product = pr_poco.GetProducts(b.Id, "", "", false, false, false),
        //                       //tag_pocos = pt_poco.GetBoothTagPOCOs(b.booth_id),
        //                       street_address = b.street_address,
        //                       country = b.country,
        //                       fulladdress = b.fulladdress,
        //                       region_id = b.region_id,
        //                       region = b.region,
        //                       //category_main = b.category_main.ToList()//Where(c=>c.Id == b.Id).ToList(),//cat_poco.GetAllBooth(b.Id, false)
        //                       //category_main = cat_poco.GetAllBoothParent(b, true)
        //                       category_main = b.category_main
        //                   })
        //                  .AsEnumerable()                          
        //                  .Select(b => new booth
        //                  {
        //                      Id = b.Id,
        //                      name = b.name,
        //                      sysname = b.sysname,
        //                      created_on = b.created_on,
        //                      description = b.description,
        //                      frontimage = b.frontimage,
        //                      boothrating = b.boothrating,
        //                      //person_id = b.person_id,
        //                      person_id = b.person_id,
        //                      person = withsalesman ? s_poco.GetPerson(b.person_id, "Salesman", false, false, false) : null,
        //                      product = pr_poco.GetProducts(b.Id, "", "", true, false, true, false, false, false),
        //                      //tag_pocos = pt_poco.GetBoothTagPOCOs(b.booth_id),
        //                      street_address = b.street_address,
        //                      country = b.country,
        //                      fulladdress = b.fulladdress,
        //                      region_id = b.region_id,
        //                      region = b.region,
        //                      category_main = cat_poco.GetAllBoothParent(new booth() { category_main = b.category_main }, true)//b.category_main
        //                  }).FirstOrDefault();/**/

        //    if (_b.IsNull())
        //        throw new Exception("A-OK, Handled.");
                
        //    return _b;
        //}

        //public poco_booth _GetBoothPOCOByCollectionId(long collection_id, bool withsalesman)
        //{
        //    poco_booth booth_poco = new poco_booth();
        //    booth booth = GetBoothByCollectionId(collection_id, withsalesman);

        //    booth_poco.ToPoco(booth, null);
        //    return booth_poco;
        //}

        public List<booth> GetNewestBooths(int skip, int take)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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
                    b.boothrating = NullHelper.BNull(b.boothrating.ToList());
                    b.conversation = null;
                    b.foldera = null;
                    b.followers = null;
                    b.region = NullHelper.BNull(b.region);
                    b.person = NullHelper.BNull(b.person);
                    //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                }

                return booths.ToList();
            }
            return new List<booth>();
        }
        public List<poco_booth> GetNewestBoothPOCOs(int skip, int take)
        {
            List<poco_booth> list = new List<poco_booth>();
            List<booth> booths = GetNewestBooths(skip, take);

            list = this.ToPocoList(booths, null);
            return list;

        }

        public List<booth> GetBoothsByPerson(string salesman_id)
        {
            if (string.IsNullOrEmpty(salesman_id))
                throw new Exception("A-OK, Check");

            EbazarDB _db = DAL.GetInstance().GetContext();

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
                    b.boothrating = NullHelper.BNull(b.boothrating.ToList());
                    b.conversation = null;
                    b.foldera = null;
                    b.followers = null;
                    b.region = NullHelper.BNull(b.region);
                    b.person = NullHelper.BNull(b.person);
                    //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                }

                return booths.ToList();
            }
            return new List<booth>();
        }

        public List<poco_booth> GetBoothsByPersonPOCO(string person_id)
        {
            List<booth> booths = GetBoothsByPerson(person_id);
            List<poco_booth> list = this.ToPocoList(booths, null);
            return list;
        }

        public void SetItems(bool orderbyrelevance)
        {
            items = product_pocos != null ? items.Concat(product_pocos).ToList() : items;
            items = collection_pocos != null ? items.Concat(collection_pocos).ToList() : items;
            if (orderbyrelevance)
            {
                items = items.OrderByDescending(i => i.created_on).ToList();
                items = items.OrderByDescending(i => i.relevant).ToList();
            }
        }
        
        public List<IBoothItem> GetRelevantItems(int skip, int take)
        {
            if (items.Count() == 0)
                SetItems(true);
            if (take != -1)
                return items.Skip(skip).Take(take).ToList();
            return items.ToList();
        }

        public IBoothItem GetNewestItem()
        {
            if (items.Count() == 0)
                SetItems(true);
            List<IBoothItem> list = items;
            if (list.Count > 0)
                return list.FirstOrDefault();
            else
                return new poco_product(false) { created_on = DateTime.MinValue };////////////////////////////er det den rigtige måde at gøre det på?
        }

        public List<booth> GetBooths(string salesman_id, bool withsalesman)
        {
            if (string.IsNullOrEmpty(salesman_id))
                throw new Exception("A-OK, Check");

            EbazarDB _db = DAL.GetInstance().GetContext();

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
                    b.boothrating = NullHelper.BNull(b.boothrating.ToList());
                    b.conversation = null;
                    b.foldera = null;
                    b.followers = null;
                    b.region = NullHelper.BNull(b.region);
                    b.person = withsalesman ? NullHelper.BNull(b.person) : null;
                    //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                }

                return booths.ToList();
            }
            return new List<booth>();
        }

        public List<poco_booth> GetBoothPOCOs(string salesman_id, bool withsalesman)
        {
            List<booth> booths = GetBooths(salesman_id, withsalesman);
            List<poco_booth> list = this.ToPocoList(booths, null);
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

                /*poco_product product_poco = new poco_product(false);//product/collection allerede fjernet i AdministrationCon?
                foreach (product p in result.product)
                    product_poco.Delete(p.Id, _db);
                
                poco_collection collection_poco = new poco_collection();
                foreach (collection c in result.collection)
                    collection_poco.Delete(c.Id, _db);*/
                
                poco_conversation con_poco = new poco_conversation();
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

        public bool RemoveTag(long tag_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            tag tag = _db.tag.Where(t => t.Id == tag_id && t.form == "booth").Select(t => t).FirstOrDefault();
            if (tag != null)
            {
                booth booth = _db.booth.Where(b => b.Id == this.booth_id).Select(b => b).FirstOrDefault();
                foreach (product pro in booth.product)
                {
                    if (pro.tag.Contains(tag))
                        return false;
                }
                foreach (collection col in booth.collection)
                {
                    if (col.tag.Contains(tag))
                        return false;
                }
                //if (booth != null)
                //    booth.tag.Remove(tag);

                if ((tag.collection.Count + tag.product.Count) < 1/* && tag.booth.Count <= 1*/)
                    _db.tag.Remove(tag);
                _db.SaveChanges();
            }
            else
                throw new Exception("A-OK, handled.");
            return true;
        }

        public void RemoveImage() { RemoveBoothImage(); }
        private void RemoveBoothImage()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            booth booth = _db.booth.Where(b => b.Id == this.booth_id).Select(t => t).FirstOrDefault();
            if (booth == null)
                throw new Exception("A-OK, handled.");

            booth.frontimage = null;
            _db.SaveChanges();

        }

        public List<booth> GetBooths(int skip, int take, out int count, out List<Hit> rel_hits)
        {
            /*
             * when updating this function, remember to update poco_product.cs and poco_collection.cs -> IsRelevant
             * */

            rel_hits = new List<Hit>();
            //using (EbazarDB _db = new EbazarDB())
            //DAL.GetInstance().Dispose();
            EbazarDB _db = DAL.GetInstance().GetContext();
            {
                poco_salesman s_poco = new poco_salesman();
                poco_product pr_poco = new poco_product(false);
                poco_collection col_poco = new poco_collection();
                poco_category cat_poco = new poco_category();

                poco_product pro_relevant = new poco_product(false);
                poco_collection col_relevant = new poco_collection();
                string[] opt, cats;
                string op1, op2, op3, op4, op5, op6, cat;
                int fra, til, zip;
                RelevantHelper helper = RelevantHelper.Create(false);
                helper.GetVals(out opt, out op1, out op2, out op3, out op4, out op5, out op6, out cats, out cat, out fra, out til, out zip);

                bool cat_selected = Categorys.CatsYesNo.Where(x => x.name == cat).Count() > 0;
                string option1 = opt[0], option2 = opt[1], option3 = opt[2];

                bool is_param = _db.param.Where(x => x.name == op2 || x.name == op3 || x.name == op4).Count() > 0 || _db.value.Where(x => x.value1 == op2 || x.value1 == op3 || x.value1 == op4).Count() > 0;

                int ato = 0;
                int afrom = 0;
                bool dk = false;
                foreach (string s in Areas.selected)//allways only holds one entry
                {
                    if (s == "dk")
                        dk = true;
                    if (s == "dk")
                        continue;
                    Area area = Areas.areas.Where(x => x.area.Replace(" ", "").ToLower() == s).FirstOrDefault();
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

                                //start product IsRelevant
                                (b.collection.Where(p => (
                                    p != null && p.name != "" && p.active && p.booth != null && p.booth.region != null &&

                                    (((p.joinedprice == NOP.INGEN_PRIS.ToString()) && (p.active && p.joinedprice != null && p.joinedprice != "")) ||
                                    ((p.joinedprice != NOP.INGEN_PRIS.ToString()) && (p.active && p.joinedprice != null && p.joinedprice != "" /*&& p.price_int >= fra &&*//* p.price_int <= til*/))) &&

                                    (cat == "alle" && p.active) ||
                                    (cat != "alle" && cats.Count() == 1 && p.active && cat == p.category_main.name) ||
                                    (cat != "alle" && cats.Count() > 1 && p.active && cat == p.category_main.name && p.category_second != null && cats.Contains(p.category_second.name)))).Count() > 0) &&

                                (option1 == "" || is_param ||
                                (((opt.Where(x => x != "" && b.collection.Where(p => 
                                   ((p.name.ToLower().Trim().Contains(x) && p.active) ||
                                    (p.tag.Where(t => t.name == x).Count() > 0 && p.active) ||
                                    (p.description.ToLower().Trim().Contains(x) && p.active))).Count() > 0)).Count() > 0))) &&


                                (zip == 0 && dk ?
                                true :
                                zip != 0 && dk ?
                                b.region.zip == zip :
                                _db.region.Where(x => x.zip == b.region.zip && x.zip <= ato && x.zip >= afrom).Count() == 1)))
                                //end product IsRelevant

                         orderby b.modified descending
                         group b by b.Id into g
                         select g.FirstOrDefault());

                _b.ToTraceStringA();

                IEnumerable<booth> booths = _b.AsEnumerable().ToList();
                if (booths.IsNull())
                    throw new Exception("A-OK, Handled.");

                ;

                booths = booths.Where(x =>
                x.product.Where(z => pro_relevant.IsRelevant(z, null, x.name, is_param, helper)).Count() > 0 ||
                x.collection.Where(z => col_relevant.IsRelevant(null, z, x.name, is_param, helper)).Count() > 0
                ).ToList();

            
                ;

                if (booths.IsNull())
                    throw new Exception("A-OK, Handled.");

                count = booths.Count();

                if (booths.Any())
                {
                    if (skip != -1)
                        booths = booths.Skip(skip).Take(take);

                    rel_hits = rel_hits.Concat(pro_relevant.relevant_hits).ToList();
                    rel_hits = rel_hits.Concat(col_relevant.relevant_hits).ToList();

                    /*
                    * HACK - just a precaution
                    * */
                
                    foreach (booth b in booths)
                    {
                        if (b.product.IsNull());
                        if (b.collection.IsNull());
                        if (b.category_main.IsNull());
                        if (b.region.IsNull());
                        if (b.boothrating.IsNull());
                        if (b.person.IsNull());
                        if (b.conversation.IsNull());
                        if (b.foldera.IsNull()) ;
                        if (b.followers.IsNull()) ;

                        b.product = NullHelper.BNull(b.product.ToList(), false);
                        b.collection = NullHelper.BNull(b.collection.ToList(), false);
                        b.category_main = NullHelper.BNull(b.category_main.ToList());
                        b.boothrating = NullHelper.BNull(b.boothrating.ToList());
                        b.conversation = NullHelper.BNull(b.conversation.ToList());
                        b.foldera = NullHelper.BNull(b.foldera.ToList());
                        b.followers = NullHelper.BNull(b.followers.ToList());

                        //var __e = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(b.region.GetType());
                        b.region = NullHelper.BNull(b.region);
                        b.person = NullHelper.BNull(b.person);
                    }
                    return booths.ToList();
                }
                return new List<booth>();
            }
        }

        public List<poco_booth> GetBoothPOCOs(int skip, int take, out int count)
        {
            List<Hit> rel;
            List<poco_booth> list = new List<poco_booth>();
            List<booth> booths = GetBooths(skip, take, out count, out rel);

            list = this.ToPocoList(booths, rel);
            return list;
        }

        /*public bool IsRelevantA(booth b, RelevantHelper helper)
        {
            if (b.IsNull())
                throw new Exception("A-OK, Check");

            if (helper.IsNull())
                throw new Exception("A-OK, Check");

            string[] opt, cats;
            string op1, op2, op3, op4, op5, op6, cat;
            int fra, til, zip;
            //RelevantHelper helper = RelevantHelper._Create(false);
            helper.GetVals(out opt, out op1, out op2, out op3, out op4, out op5, out op6, out cats, out cat, out fra, out til, out zip);

            bool ok;
            string desc = StringHelper.OnlyAlphanumeric(b.description.ToLower().Trim(), false, false, "notag", CharacterHelper.Space(), out ok);
            this.relevant = (
                            (opt[0] != "") ?
                            (!string.IsNullOrEmpty(opt[0]) && (b.name.ToLower().Trim() == opt[0].ToLower().Trim())) ||
                            (!string.IsNullOrEmpty(opt[0]) && (b.name.ToLower().Trim().Contains(opt[0].ToLower().Trim()))) ||
                            (opt.Where(x => x != "" && desc.Contains(x)).Count() > 0) :

                            false) &&
                            ((b.product != null && b.product.Count() > 0) || (b.collection != null && b.collection.Count > 0)) &&
                            ((zip == 0 ? true : b.region.zip == zip || Areas.IsRelevant(b.region.zip)));

            return this.relevant;
        }

        public bool IsRelevantB(booth b, RelevantHelper helper)
        {
            if (b.IsNull())
                throw new Exception("A-OK, Check");

            if (helper.IsNull())
                throw new Exception("A-OK, Check");

            string[] opt, cats;
            string op1, op2, op3, op4, op5, op6, cat;
            int fra, til, zip;
            //RelevantHelper helper = RelevantHelper._Create(false);
            helper.GetVals(out opt, out op1, out op2, out op3, out op4, out op5, out op6, out cats, out cat, out fra, out til, out zip);

            this.relevant = (cat == "alle" ?
                            true :
                            (cat != "alle" && (opt[0] == "" && opt[1] == "" && opt[2] == "") ?
                            true :
                            (cat != "alle" && (opt[0] != "" || opt[1] != "" || opt[2] != "")) ?
                            (Categorys.CatsYesNo.Where(t => t.name == cat).Count() > 0) :

                            false)) &&
                            ((b.product != null && b.product.Count() > 0) || (b.collection != null && b.collection.Count > 0)) &&
                            (zip != 0 ? b.region.zip == zip : true) &&
                            www.e_bazar.dk.SharedClasses.Areas.IsRelevant(b.region.zip);

            return this.relevant;
        }/**/

        public void setupaddress(booth b)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            if (b == null)
                throw new Exception("A-OK, Check");

            this.street_address = !string.IsNullOrEmpty(b.street_address) ? b.street_address : "";

            if (b.region == null)
            {
                this.region_poco = (from r in _db.region
                                    where r.zip == 0000 && r.town == "default"
                                    select new poco_region { Id = r.Id, zip = r.zip, town = r.town }).FirstOrDefault();
            }
            else
            {
                this.region_poco = new poco_region();
                this.region_poco.ToPOCO(b.region);
            }

            this.country = !string.IsNullOrEmpty(b.country) ? b.country : "Danmark";
            this.fulladdress = b.fulladdress;
            this.region_id = this.region_poco.Id;
        }
        
        public List<booth> _ToBoothList(List<poco_booth> booth_pocos)
        {
            if (booth_pocos == null)
                throw new Exception("A-OK, Check.");

            List<booth> list = new List<booth>();
            foreach (poco_booth b in booth_pocos)
            {
                booth booth = this.ToBooth(false);
                list.Add(booth);
            }
            return list;
        }

        public booth ToBooth(bool new_booth, EbazarDB _db = null)
        {
            //DAL.GetInstance().DB = new EbazarDB();

            if (_db == null)
                _db = DAL.GetInstance().GetContext();

            booth result = new booth();
            if (!new_booth)
                result = _db.booth
                    .Include(x => x.person)
                    .FirstOrDefault(b => b.Id == this.booth_id);
            if (result == null)
                throw new Exception("A-OK, Handled.");

            result.description = !string.IsNullOrEmpty(this.description) ? this.description : "";
            result.frontimage = !string.IsNullOrEmpty(this.frontimage) ? this.frontimage : "";
            result.name = !string.IsNullOrEmpty(this.name) ? this.name : "";
            result.sysname = !string.IsNullOrEmpty(this.sysname) ? this.sysname : "";

            if (new_booth/*result.person == null*/)
            {
                //result.person = _db.person.Where(s => s.Id == this.salesman_poco.person_id).FirstOrDefault();
                result.person_id = this.salesman_poco.person_id;
                DateTime now = DateTime.Now;
                result.created_on = now;
                result.modified = now;
            }

            result.searchable = true;

            result.street_address = !string.IsNullOrEmpty(this.street_address) ? this.street_address : "";
            result.country = !string.IsNullOrEmpty(this.country) ? this.country : "";
            result.fulladdress = this.fulladdress_str == "Full" ? true : false;
            region region = _db.region.FirstOrDefault(r => r.zip == this.region_poco.zip && r.town == this.region_poco.town);
            if (region == null)
                throw new Exception("A-OK, Handled.");
            result.region_id = region.Id;

            return result;
        }        

        public List<poco_booth> ToPocoList(ICollection<booth> booths, List<Hit> rel)
        {
            if (booths == null)
                throw new Exception("A-OK, Check");

            List<poco_booth> list = new List<poco_booth>();
            foreach (booth b in booths.ToList())
            {
                poco_booth booth_poco = new poco_booth();
                booth_poco.ToPoco(b, rel);
                list.Add(booth_poco);
            }
            return list;
        }

        public void ToPoco(booth b, List<Hit> rel)
        {
            if (b == null)
                throw new Exception("A-OK, Check");

            this.booth_id = b.Id;
            this.created_on = b.created_on.IsNotNull() ? b.created_on : this.created_on;
            this.modified = b.modified.IsNotNull() ? b.modified : this.modified;
            
            this.name = b.name.IsNotNull() ? b.name : "";
            this.sysname = b.sysname.IsNotNull() ? b.sysname : "";
            this.frontimage = b.frontimage.IsNotNull() ? b.frontimage : "";

            this.description = !string.IsNullOrEmpty(b.description) ? b.description : Texts.GetNopValue(NOP.NO_DESCRIPTION.ToString());

            if (b.boothrating != null)
            {
                this.numberofratings = (from r in b.boothrating
                                        select r).Count();
                this.boothrating = (from r in b.boothrating
                                    select (int?)r.rating).ToList().Average();
                if (this.numberofratings == null)
                    this.numberofratings = 0;
                if (b.boothrating.Count() == 0)
                {
                    this.boothrating = 0.0;
                    this.boothrating_nop = Texts.GetNopValue(NOP.NO_RATING.ToString());
                }
            }

            this.salesman_id = !b.person_id.IsNull() ? b.person_id : this.salesman_id;

            if (b.person.IsNotNull())
            {
                this.salesman_poco = new poco_salesman();
                //this.salesman_poco.ToPoco<poco_salesman>(Null(b.person));
                //string name = b.person.firstname;
                person per = b.person;
                this.salesman_poco.ToPoco<poco_salesman>(NullHelper.BNull(per));
            }

            setupaddress(b);

            poco_product pro_poco = new poco_product(false);
            poco_collection col_poco = new poco_collection();

            poco_folder fld_poco = new poco_folder();
            poco_category cat_poco = new poco_category();

            this.product_pocos = b.product.IsNotNull() ? pro_poco.ToPocoList(NullHelper.BNull(b.product.ToList(), false), rel, b.name) : new List<poco_product>();
            this.collection_pocos = b.collection.IsNotNull() ? col_poco.ToPocoList(NullHelper.BNull(b.collection.ToList(), false), rel, b.name) : new List<poco_collection>();
            this.foldera_pocos = b.foldera.IsNotNull() ? fld_poco.ToPocoList(NullHelper.BNull(b.foldera.ToList())) : new List<poco_folder>();
            this.category_main = b.category_main.IsNotNull() ? cat_poco.ToPocoList(NullHelper.BNull(b.category_main.Where(x=>x.name != ".ingen").ToList()), false/*used to be true*/, false) : new List<poco_category>();
            this.hits_items = rel.IsNotNull() ? rel.Where(x => x.booth == this.name).Count() : this.hits_items;
        }
    }
}