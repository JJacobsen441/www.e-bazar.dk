using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.SharedClasses;
using static www.e_bazar.dk.Models.DTOs.poco_booth;
//using static www.e_bazar.dk.SharedClasses.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_collection : booth_item, IBoothItem
    {
        public poco_collection()
        {
        }
                
        
          
        public virtual List<poco_product> product_pocos { get; set; }

        

        private List<collection_param> IncludeParam(long col_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            //using (EbazarDB db = new EbazarDB())
            {
                List<collection_param> pars = _db.collection_param
                    .Include("param")
                    .Include("value")
                    .Where(x => x.collection_id != null && x.collection_id == col_id)
                    .ToList();
                if (pars == null)
                    throw new Exception("A-OK, Handled.");
                return pars;
            }
        }

        public collection GetCollection(long? collection_id, bool withproducts, bool withbooth, bool withconvesations, bool withboothsalesman, bool withtags)
        {
            //if (tags == null)
            //    throw new Exception("A-OK, Check.");

            if (collection_id == null)
                return null;

            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_booth b_poco = new poco_booth();
            poco_product p_poco = new poco_product(false);
            poco_conversation c_poco = new poco_conversation();
            poco_tag t_poco = new poco_tag();
            collection collection = (from c in _db.collection
                                     join b in _db.booth on c.booth_id equals b.Id
                                     where c.Id == collection_id
                                     select new
                                     {
                                         Id = c.Id,
                                         category_main = c.category_main,
                                         category_second = c.category_second,
                                         category_main_id = c.category_main_id,
                                         category_second_id = c.category_second_id,
                                         name = c.name,
                                         sysname = c.sysname,
                                         created_on = c.created_on,
                                         modified = c.modified,
                                         joinedprice = c.joinedprice,
                                         //status_delivery = c.status_delivery,
                                         status_stock = c.status_stock,
                                         status_condition = c.status_condition,
                                         description = c.description,
                                         note = c.note,
                                         active = c.active,

                                         image = c.image.OrderBy(i => i.created_on).ToList(),//ci_poco.GetImages((int)c.Id),
                                                                                             //tag = c.tag.ToList(),//t_poco.GetCollectionTags((int)c.Id),
                                         booth_id = c.booth_id,
                                         //booth = withbooth ? c.booth : null,//b_poco.GetBoothByCollectionId((int)c.Id) : null,
                                         //product = withproducts ? c.product.ToList() : null,//p_poco.GetProductsByCollectionId((int)c.Id, false, false) : null,
                                         //conversation = withconvesations ? c.conversation.ToList() : null//c_poco.GetConversationsCollection((int)c.Id) : null

                                         fold_a = c.foldera,
                                         fold_b = c.folderb,
                                         fold_a_id = c.folder_a_id,
                                         fold_b_id = c.folder_b_id
                                     }).AsEnumerable()
                                                .Select(c => new collection
                                                {
                                                    Id = c.Id,
                                                    category_main = c.category_main,
                                                    category_second = c.category_second,
                                                    category_main_id = c.category_main_id,
                                                    category_second_id = c.category_second_id,
                                                    name = c.name,
                                                    sysname = c.sysname,
                                                    created_on = c.created_on,
                                                    modified = c.modified,
                                                    joinedprice = c.joinedprice,
                                                    //status_delivery = c.status_delivery,
                                                    status_stock = c.status_stock,
                                                    status_condition = c.status_condition,
                                                    description = c.description,
                                                    note = c.note,
                                                    active = c.active,

                                                    image = c.image.OrderBy(i => i.created_on).ToList(),//ci_poco.GetImages((int)c.Id),
                                                    tag = withtags ? /*c.tag.ToList(),*/t_poco.GetCollectionTags((int)c.Id) : null,
                                                    booth_id = c.booth_id,
                                                    booth = withbooth ? b_poco.GetBoothByCollectionId(c.Id, withboothsalesman) : null,
                                                    product = withproducts ? p_poco.GetProductsByCollectionId((int)c.Id, false, false) : null,
                                                    conversation = withconvesations ? c_poco.GetConversations(-1, (int)c.Id, -1, TYPE.COLLECTION) : null,

                                                    foldera = c.fold_a,
                                                    folderb = c.fold_b,
                                                    folder_a_id = c.fold_a_id,
                                                    folder_b_id = c.fold_b_id,
                                                    collection_param = IncludeParam(c.Id)
                                                })
                                                .Cast<collection>().ToList().FirstOrDefault();
            if (collection == null)
                throw new Exception("A-OK, Handled.");
            return collection;// collection_poco.SetupCollectionToClient(new EbazarDB());
            //else
            //    collection_poco = null;//yes, den kan godt være null
            //return collection_poco;
        }

        public poco_collection GetCollectionPOCO(long? collection_id, bool withproducts, bool withbooth, bool convesations, bool withboothsalesman, bool withtags)
        {
            poco_collection col_poco = new poco_collection();
            collection collection = GetCollection(collection_id, withproducts, withbooth, convesations, withboothsalesman, withtags);

            //if (withbooth && collection.booth == null)
            //    return null;
            col_poco.ToPoco(collection, null, "");
            col_poco.SetupToClient(collection);
            return col_poco;//yes, den kan godt være null

        }

        public List<collection> GetCollections(int booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withbooth, bool withtags, bool withfoldera)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_tag t_poco = new poco_tag();
            poco_booth b_poco = new poco_booth();

            folder default_a = new folder();
            default_a.name = "foldera_default";
            folder default_b = new folder();
            default_b.name = "folderb_default";

            lev_a_search = lev_a_search == null ? "" : lev_a_search;
            lev_b_search = lev_b_search == null ? "" : lev_b_search;
            IEnumerable<collection> collection = (from c in _db.collection
                                                    .Include("category_main")
                                                    .Include("category_second")
                                                  join la in _db.folder on c.folder_a_id equals la.Id into leva
                                                  join lb in _db.folder on c.folder_b_id equals lb.Id into levb
                                                  from la in leva.DefaultIfEmpty()
                                                  from lb in levb.DefaultIfEmpty()
                                                  where c.booth_id == booth_id && (c.active || select_inactive) &&
                                                  ((lev_a_search == "" && lev_b_search == "") ||
                                                  (lev_a_search != "" && la.name == lev_a_search && lev_b_search == "" && c.folderb == null) ||
                                                  (lev_b_search != "" && lb.name == lev_b_search) ||
                                                  (lev_a_search != "" && la.name == lev_a_search && lev_b_search == "") ? true : false) ? true : false
                                                  orderby c.booth.category_main.Where(c => c.Id == category_main_id).FirstOrDefault().name ascending
                                                  select new
                                                  {
                                                      Id = c.Id,
                                                      category_main_id = c.category_main_id,
                                                      category_second_id = c.category_second_id,
                                                      category_main = c.category_main,
                                                      category_second = c.category_second,
                                                      level_a_id = c.folder_a_id,//.levela.Id,
                                                      level_b_id = c.folder_b_id,//.levelb.Id,
                                                      levela = c.foldera,//lev_a.GetLevelAPOCO(lev_a_id, false, false),
                                                      levelb = c.folderb,//lev_b.GetLevelBPOCO(lev_b_id),
                                                      name = c.name,
                                                      sysname = c.sysname,
                                                      created_on = c.created_on,
                                                      modified = c.modified,
                                                      joinedprice = c.joinedprice,
                                                      //status_delivery = c.status_delivery,
                                                      status_stock = c.status_stock,
                                                      status_condition = c.status_condition,
                                                      description = c.description,
                                                      note = c.note,
                                                      active = c.active,

                                                      booth_id = c.booth_id,
                                                      booth = withbooth ? c.booth : null, //b_poco.GetBoothPOCO(booth_id, "", "", false, false, true, false, false) : null,
                                                      image = c.image.OrderBy(i => i.created_on).ToList()//,ci_poco.GetCollectionImagePOCOs((int)c.id),
                                                                                                         //conversations = withconversations ? conversation_poco.GetConversationsCollection((int)c.id) : null,
                                                                                                         //product_pocos = p_poco.GetProductPOCOsByCollectionId(this.collection_id, false),
                                                                                                         //tag = withtags ? c.tag.ToList() : null//t_poco.GetCollectionTagPOCOs((int)c.id)
                                                  }).AsEnumerable()
                                           .Select(c => new collection
                                           {
                                               Id = (int)c.Id,
                                               category_main_id = c.category_main_id,
                                               category_second_id = c.category_second_id,
                                               category_main = c.category_main,
                                               category_second = c.category_second,
                                               folder_a_id = c.level_a_id,//.levela.Id,
                                               folder_b_id = c.level_b_id,//.levelb.Id,
                                               foldera = c.levela,//lev_a.GetLevelAPOCO(lev_a_id, false, false),
                                               folderb = c.levelb,//lev_b.GetLevelBPOCO(lev_b_id),
                                               name = c.name,
                                               sysname = c.sysname,
                                               created_on = c.created_on,
                                               modified = c.modified,
                                               joinedprice = c.joinedprice,
                                               //status_delivery = c.status_delivery,
                                               status_stock = c.status_stock,
                                               status_condition = c.status_condition,
                                               description = c.description,
                                               note = c.note,
                                               active = c.active,

                                               booth_id = c.booth_id,
                                               booth = withbooth ? /*c.booth : null, */b_poco.GetBooth(booth_id, "", "", false, false, true, false, withfoldera, false, true) : null,
                                               image = c.image.OrderBy(i => i.created_on).ToList(),//ci_poco.GetCollectionImagePOCOs((int)c.id),
                                               //conversations = withconversations ? conversation_poco.GetConversationsCollection((int)c.id) : null,
                                               //product_pocos = p_poco.GetProductPOCOsByCollectionId(this.collection_id, false),
                                               tag = withtags ? /*c.tag.ToList() : null*/t_poco.GetCollectionTags((int)c.Id) : null
                                           });
            //foreach (poco_collection poco in pocos)
            //    poco.SetupCollectionToClient(new EbazarDB());
            if (collection == null)
                throw new Exception("A-OK, Handled.");

            if (collection.Any())
                return collection.ToList();
            return new List<collection>();
        }

        public List<poco_collection> GetCollectionPOCOs(int booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withbooth, bool withtags, bool withfoldera)
        {
            lev_a_search = lev_a_search == null ? "" : lev_a_search;
            lev_b_search = lev_b_search == null ? "" : lev_b_search;
            List<collection> collections = GetCollections(booth_id, lev_a_search, lev_b_search, select_inactive, withbooth, withtags, withfoldera);

            /*poco_collection col_relevant = null;
            if (set_relevant)
            {
                col_relevant = new poco_collection(null);
                col_relevant.SetupRelevant();
            }*/
            return this.ToPocoList(collections, null, "");

        }

        public override long Save() { return SaveCollection(); }
        private long SaveCollection()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            collection c = new collection();
            this.ToCollection(true, ref c, _db);
            c = _db.collection.Add(c);

            _db.SaveChanges();
            _db.Dispose();

            return c.Id;
        }

        public override void Update() { UpdateCollection(); }
        private void UpdateCollection()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            collection collection = _db.collection.Where(c => c.Id == (int)this.id).FirstOrDefault();
            if (collection == null)
                throw new Exception("A-OK, Check.");
            this.ToCollection(false, ref collection, _db);

            _db.SaveChanges();
            _db.Dispose();
        }

        public override void Delete(long id, EbazarDB _db = null) { DeleteCollection(id, _db); }
        private void DeleteCollection(long id, EbazarDB _db = null)
        {
            if (_db == null)
                _db = DAL.GetInstance().GetContext();

            collection col = _db.collection.Where(c => c.Id == (int)id).FirstOrDefault();
            if (col == null)
                throw new Exception("A-OK, Handled.");

            foreach (image ci in col.image.ToList())
                _db.image.Remove(ci);
            foreach (conversation c in col.conversation.ToList())
            {
                for (int i = 0; i < c.comment.ToList().Count(); i++)
                {
                    comment co = _db.comment.ToList().ElementAt(i);
                    _db.comment.Remove(co);
                }
                _db.conversation.Remove(c);
            }
            foreach (product pr in col.product.ToList())
                col.product.Remove(pr);
            foreach (tag t in col.tag.ToList())//kig RemoveTag nedenfor
            {
                col.tag.Remove(t);
                if ((/*t.booth.Count + */t.product.Count) < 1 && t.collection.Count <= 1)
                    _db.tag.Remove(t);

            }
            booth b = col.booth;
            category main = col.category_main;
            main.collection_main.Remove(col);
            if (main.product_main.Where(c => c.booth_id == b.Id).Count() + main.collection_main.Where(c => c.booth_id == b.Id).Count() == 0)
                b.category_main.Remove(main);

            _db.collection.Remove(col);
            //db.SaveChanges();
        }

        public override bool RemoveTag(long tag_id, bool is_updating)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            bool ok = false;

            collection collection = _db.collection.Where(p => p.Id == (int)this.id).Select(p => p).FirstOrDefault();
            if (collection == null)
                throw new Exception("A-OK, Handled.");
            tag tag = collection.tag.Where(t => t.Id == tag_id).Select(t => t).FirstOrDefault();
            if (tag != null)
                ok = collection.tag.Remove(tag);
            else
                ok = false;

            if (ok && (/*tag.booth.Count + */tag.product.Count) < 1 && tag.collection.Count <= 1)
                _db.tag.Remove(tag);
            _db.SaveChanges();

            return ok;
        }

        public bool RemoveParam(int param_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            collection collection = _db.collection.Where(p => p.Id == this.id).Select(p => p).FirstOrDefault();
            if (collection == null)
                return false;

            collection_param pp = collection.collection_param.Where(ppa => ppa.param_id == param_id).FirstOrDefault();
            if (pp != null)
            {
                param param = _db.param
                    //.Include("product_param")
                    //.Include("collection_param")
                    .Where(t => t.Id == param_id).Select(t => t).FirstOrDefault();
                if (param != null)
                {
                    collection.collection_param.Remove(pp);
                    param.collection_param.Remove(pp);
                    _db.collection_param.Remove(pp);

                }
                _db.SaveChanges();
            }
            return true;

        }

        public void SetFolder(int fld_id, long collection_id, TYPE type)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            collection collection = _db.collection.Where(p => p.Id == collection_id).FirstOrDefault();
            if (collection == null)
                throw new Exception("A-OK, Handled.");

            if (fld_id == -1)
            {
                if (type == TYPE.FOLDER_A)
                {
                    collection.foldera = null;
                    collection.folderb = null;
                }
                else
                    collection.folderb = null;
            }
            else
            {
                if (type == TYPE.FOLDER_A)
                {
                    collection.foldera = _db.folder.Where(l => l.Id == fld_id).FirstOrDefault();
                    if (collection.folderb != null)
                    {
                        folder fldb = _db.folder.Where(l => l.Id == collection.folderb.Id).FirstOrDefault();
                        if (fldb != null)
                        {
                            if (fldb.collection != null)
                                fldb.collection.Remove(collection);
                        }
                        collection.folderb = null;
                    }
                }
                else
                    collection.folderb = _db.folder.Where(l => l.Id == fld_id).FirstOrDefault();
            }
            _db.SaveChanges();
        }

        public override void RemoveImage(string image_name) { RemoveCollectionImage(image_name); }
        private void RemoveCollectionImage(string image_name)
        {
            if (string.IsNullOrEmpty(image_name))
                throw new Exception("A-OK, Check.");

            EbazarDB _db = DAL.GetInstance().GetContext();

            image collectionimage = _db.image.Where(i => i.name == image_name && i.collection_id == (int)this.id).Select(t => t).FirstOrDefault();
            if (collectionimage == null)
                throw new Exception("A-OK, Handled.");

            collection collection = _db.collection.Where(p => p.Id == (int)this.id).Select(p => p).FirstOrDefault();
            if (collection != null)
            {
                collection.image.Remove(collectionimage);
                _db.image.Remove(collectionimage);
                _db.SaveChanges();
                _db.Dispose();
            }
            else
                throw new Exception("A-OK, Handled.");
        }

        public void AddProduct(long product_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            product pro = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
            if (pro == null)
                throw new Exception("A-OK, Handled.");
            collection collection = _db.collection.Where(c => c.Id == (int)this.id).FirstOrDefault();
            collection.product.Add(pro);
        }

        public void RemoveProduct(long product_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            product pro = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
            if (pro == null)
                throw new Exception("A-OK, Handled.");

            collection collection = _db.collection.Where(c => c.Id == (int)this.id).FirstOrDefault();
            collection.product.Remove(pro);
        }

        public override poco_booth GetBoothPOCO()
        {
            poco_booth booth_poco = new poco_booth();
            booth boo = booth_poco.GetBooth(this.booth_id, "", "", false, false, false, false, false, false, true);

            booth_poco.ToPoco(boo, null);
            return booth_poco;

        }

        //string[] opt = new string[6];
        //private enum OTYPE { BASIC, TYPE2, TYPE3 }
        //private string OPTION(string[] arr, int start, int len, OTYPE type)
        //{
        //    string res = "";
        //    switch (type)
        //    {
        //        case OTYPE.BASIC:
        //            foreach (string s in arr)
        //                res += s != "" ? s.Trim().ToLower() + " " : "";
        //            break;
        //        case OTYPE.TYPE2:
        //            if (len <= arr.Length && !string.IsNullOrEmpty(arr[start]))
        //                res = arr[start].Trim().ToLower();
        //            break;
        //        case OTYPE.TYPE3:
        //            if (len <= arr.Length && !string.IsNullOrEmpty(arr[start]) && !string.IsNullOrEmpty(arr[start + 1]))
        //                res = arr[start].Trim().ToLower() + " " + arr[start + 1].Trim().ToLower();
        //            break;
        //        default:
        //            break;
        //    }
        //    return res;
        //}

        //public void SetupRelevant()
        //{
        //    this.relevant_hits = new List<Hit>();
        //    this.search = ThisSession.Search;
        //    this.categorys = ThisSession.Category;
        //    this.zip = ThisSession.Zip;
        //    this.fra = ThisSession.Fra;
        //    this.til = ThisSession.Til;
        //    this.kun_med_fast = ThisSession.FastPris;

        //    this.options = search.Trim().ToLower().Split(' ');
        //    this.opt[0] = OPTION(options, -1, -1, OTYPE.BASIC);
        //    this.opt[1] = OPTION(options, 0, 1, OTYPE.TYPE2);
        //    this.opt[2] = OPTION(options, 1, 2, OTYPE.TYPE2);
        //    this.opt[3] = OPTION(options, 2, 3, OTYPE.TYPE2);
        //    this.opt[4] = OPTION(options, 0, 2, OTYPE.TYPE3);
        //    this.opt[5] = OPTION(options, 1, 3, OTYPE.TYPE3);

        //    this.cats = !string.IsNullOrEmpty(categorys) ? categorys.Split('-') : new string[] { "" };
        //    cats = cats.Where(x => x != "").ToArray();
        //    this.cat = cats.Count() > 0 ? cats[0] : "alle";
        //}

        //public bool IsRelevant(collection pro, bool counting, string b_name, Helpers helper)
        //{
        //    if (pro == null)
        //        throw new Exception("A-OK, Check.");

        //    string[] opt = helper.opt;
        //    string op1 = helper.opt[0], op2 = helper.opt[1], op3 = helper.opt[2], op4 = helper.opt[3], op5 = helper.opt[4], op6 = helper.opt[5];
        //    string[] cats = helper.cats;
        //    string cat = helper.cat;
        //    int fra = helper.fra, til = helper.til, zip = helper.zip;

        //    cat_main = pro.category_main.name;
        //    cat_second = pro.category_second != null ? pro.category_second.name : null;

        //    bool ok;
        //    string desc = StringHelper.OnlyAlphanumeric(pro.description.ToLower().Trim(), false, false, "notag", Statics.Characters.Space(), out ok);

        //    bool relevantA = pro.joinedprice == NOP.INGEN_PRIS.ToString() ?
        //                    pro.active && !string.IsNullOrEmpty(pro.joinedprice) && true :
        //                    (pro.active && !string.IsNullOrEmpty(pro.joinedprice) && int.Parse(pro.joinedprice) >= fra && int.Parse(pro.joinedprice) <= til);

        //    bool relevantB = (opt[0] == "") ?
        //                    true :
        //                    ((opt.Where(x => x != "" && pro.name.ToLower().Trim().Contains(x))).Count() > 0 && pro.active ||
        //                    (pro.tag != null && opt.Where(x => pro.tag.Where(t => t.name == x).Count() > 0).Count() > 0 && pro.active) ||
        //                    (opt.Where(x => x != "" && desc.Contains(x)).Count() > 0 && pro.active));

        //    bool relevantC = (
        //                    (cat == "alle") ?
        //                    pro.active &&
        //                    true :

        //                    (!counting && cat != "alle" && cats.Count() == 1) ?
        //                    pro.active &&
        //                    cat == cat_main :

        //                    (!counting && cat != "alle" && cats.Count() > 1) ?
        //                    pro.active &&
        //                    cat == cat_main &&
        //                    (pro.category_second != null && cats.Contains(cat_second)) :

        //                    (counting && cat != "alle") ?
        //                    pro.active &&
        //                    cat == cat_main :

        //                    false)
        //                    ;

        //    bool relevantD = pro.booth != null && pro.booth.region != null ? (zip != 0 ? pro.booth.region.zip == zip : true) &&
        //        Areas.IsRelevant(pro.booth.region.zip) : true;

        //    bool relevantF = CheckParam(pro, helper);

        //    this.relevant = relevantA && relevantB && relevantC && relevantD && relevantF;
        //    if (this.relevant)
        //        this.relevant_hits.Add(new Hit() { booth = b_name, product = pro.name });
        //    return this.relevant;
        //}

        //public bool IsRelevant(collection pro, string b_name, Helpers helper)
        //{
        //    bool relevant = CheckParam(pro, helper);
        //    if (relevant)
        //        this.relevant_hits.Add(new Hit() { booth = b_name, product = pro.name });
        //    this.relevant = relevant;
        //    return relevant;
        //}

        //public bool CheckParam(collection pro, Helpers helper)
        //{
        //    if (pro.IsNull())
        //        throw new Exception("A-OK, Check.");

        //    string[] opt = helper.opt;

        //    /*
        //     * HACK - just a precaution
        //     * */
        //    EbazarDB _db = DAL.GetInstance().GetDB();
        //    if (pro.collection_param == null || !pro.collection_param.Any())
        //        pro.collection_param = _db.collection_param.Where(x => x.collection_id == pro.Id).ToList();

        //    bool relevantF = false;

        //    if (ThisSession.Params.Count == 0)
        //        relevantF = true;

        //    List<param> l1 = null;
        //    List<value> l2 = null;

        //    if (!relevantF)
        //    {
        //        l1 = pro.collection_param.Select(x => x.param).ToList();
        //        l2 = pro.collection_param.Select(x => x.value).ToList();

        //        foreach (string s in opt)
        //        {
        //            if (s == "")
        //                continue;
        //            if (Params(s, "S", l1, l2) || Params(s, "MS", l1, l2) || Params(s, "M", l1, l2))
        //                relevantF = true;
        //        }
        //    }

        //    if (!relevantF)
        //    {
        //        foreach (poco_params pa in ThisSession.Params)
        //        {
        //            if (pa.type == "MS" || pa.type == "M")
        //            {
        //                foreach (poco_value val in pa.values_daos)
        //                {
        //                    if (Params(val.value, pa.type, l1, l2))
        //                        relevantF = true;
        //                }
        //            }
        //            else
        //            {
        //                if (Params(pa.name, pa.type, l1, l2))
        //                    relevantF = true;
        //            }
        //        }
        //    }

        //    return relevantF;
        //}

        //private bool Params(string pick, string type, List<param> l1, List<value> l2)
        //{
        //    if (string.IsNullOrEmpty(pick))
        //        throw new Exception("A-OK, Check.");
        //    if (string.IsNullOrEmpty(type))
        //        throw new Exception("A-OK, Check.");

        //    if (type == "S")
        //    {
        //        foreach (param pa in l1)
        //        {
        //            if (pa.type == "S" && pa.name == pick)
        //                return true;
        //        }
        //    }
        //    else if (type == "MS" || type == "M")
        //    {
        //        foreach (param pa in l1)
        //        {
        //            foreach (value val in l2)
        //            {
        //                if (val.IsNull())
        //                    continue;
        //                if (val.param == pa && val.value1 == pick)
        //                    return true;
        //            }
        //        }
        //    }

        //    return false;
        //}/**/

        public void SetupToClient(collection collection)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            if (this.booth_poco != null)
            {
                if (collection != null && collection.category_main != null)
                {
                    poco_category cat_poco = new poco_category();
                    List<poco_category> cats_top = cat_poco._GetAll(true, true);//.Where(x=>x.name != ".ingen").ToList();

                    this.category_main_selectlist = new SelectList(cats_top, "category_id", "name", this.category_main_id);

                    if (this.category_second_id == 0)
                        this.category_second_selectlist = new SelectList(cats_top.OrderBy(x => x.priority).FirstOrDefault().children, "category_id", "name", this.category_second_id);
                    else
                        this.category_second_selectlist = new SelectList(cats_top.Where(x => x.category_id == this.category_main_id).FirstOrDefault().children, "category_id", "name", this.category_second_id);
                }

                List<folder> lista = new List<folder>() { new folder() { Id = -1, name = "ingen.." } };
                lista = lista.Concat(_db.folder.Where(l => l.booth_id == this.booth_poco.booth_id).OrderBy(l => l.priority)).ToList();

                if (collection != null)
                {
                    folder tmpa = collection.foldera;

                    if (lista.Count() > 0 && tmpa != null)
                        this.foldera_selectlist = new SelectList(lista, "Id", "name", tmpa.Id);
                    else
                        this.foldera_selectlist = new SelectList(lista, "Id", "name");

                    List<folder> listb = new List<folder>() { new folder() { Id = -1, name = "default" } };
                    if (tmpa != null)
                    {
                        listb = listb.Concat(_db.folder.Where(l => l.parent_id == tmpa.Id).OrderBy(l => l.priority)).ToList();
                        //folder tmpb = _db.collection.Where(p => p.Id == this.id).FirstOrDefault().folderb;
                        folder tmpb = collection.folderb;
                        if (listb.Count() > 0 && tmpb != null)
                            this.folderb_selectlist = new SelectList(listb, "Id", "name", tmpb.Id);
                        else
                            this.folderb_selectlist = new SelectList(listb, "Id", "name");
                    }
                    else
                        this.folderb_selectlist = new SelectList(listb, "Id", "name");
                }
            }



            this.status_stock = string.IsNullOrEmpty(this.status_stock) ? STOCK.PÅ_LAGER.ToString() : this.status_stock;
            this.status_stock_selectlist = EnumHelper.SelectListFor(Texts.GetStockEnum(this.status_stock));

            this.status_condition = string.IsNullOrEmpty(this.status_condition) ? CONDITION.VELHOLDT.ToString() : this.status_condition;
            this.status_condition_selectlist = EnumHelper.SelectListFor(Texts.GetConditionEnum(this.status_condition));

            this.price = string.IsNullOrEmpty(this.price) ? NOP.INGEN_PRIS.ToString() : this.price;
            this.note = string.IsNullOrEmpty(this.note) ? Texts.GetNopValue(NOP.NO_NOTE.ToString()) : this.note;
            this.description = string.IsNullOrEmpty(this.description) ? Texts.GetNopValue(NOP.NO_DESCRIPTION.ToString()) : this.description;

            if (this.tag_pocos == null || this.tag_pocos.Count == 0)
            {
                this.tag_pocos = null;
                this.tag_pocos_nop = Texts.GetNopValue(NOP.NO_TAGS.ToString());
            }
        }

        private tag Null(tag t)
        {
            if (t.collection != null)
                t.collection = null;
            if (t.product != null)
                t.product = null;

            return t;
        }

        protected person Null(person per)
        {
            if (per.booth != null)
                per.booth = null;
            if (per.boothrating != null)
                per.boothrating = null;
            if (per.comment != null)
                per.comment = null;
            if (per.conversation != null)
                per.conversation = null;
            if (per.favorites_collection != null)
                per.favorites_collection = null;
            if (per.favorites_product != null)
                per.favorites_product = null;
            if (per.following != null)
                per.following = null;

            return per;
        }

        protected List<category> Null(List<category> c)
        {
            List<category> list = new List<category>();
            foreach (category cat in c)
            {
                list.Add(Null(cat));
            }
            return list;
        }

        protected category Null(category cat)
        {
            if (cat.booth != null)
                cat.booth = null;
            //if (cat.children != null)
            //    cat.children = null;
            if (cat.collection_main != null)
                cat.collection_main = null;
            if (cat.collection_second != null)
                cat.collection_second = null;
            //if (cat.param != null)
            //    cat.param = null;
            if (cat.parent != null)
                cat.parent = null;
            if (cat.product_main != null)
                cat.product_main = null;
            if (cat.product_second != null)
                cat.product_second = null;

            return cat;
        }

        protected booth Null(booth bth)
        {
            if (bth.boothrating != null)
                bth.boothrating = null;
            if (bth.category_main != null)
                bth.category_main = Null(bth.category_main.ToList());
            if (bth.collection != null)
                bth.collection = null;
            if (bth.conversation != null)
                bth.conversation = null;
            if (bth.foldera != null)
                bth.foldera = null;
            if (bth.followers != null)
                bth.followers = null;
            if (bth.person != null)
                bth.person = Null(bth.person);// Null(b.person);
            //if (bth.product != null)
            //    bth.product = null;
            //if (bth.region != null)
            //    bth.region = null;

            return bth;
        }

        public void ToPoco(collection c, List<Hit> rel, string booth)
        {
            if (c == null)
                throw new Exception("A-OK, Check.");

            this.id = c.Id;
            this.booth_id = c.booth_id;

            this.category_main_id = c.category_main_id;
            this.category_second_id = c.category_second_id;

            if (c.foldera != null)
            {
                poco_folder flda = new poco_folder();
                this.foldera = flda.GetFolderAPOCO(c.foldera.Id, false, false);
            }
            else
                this.foldera = new poco_folder() { name = "default" };

            if (c.folderb != null)
            {
                poco_folder fldb = new poco_folder();
                this.folderb = fldb.GetFolderBPOCO(c.folderb.Id);
            }
            else
                this.folderb = new poco_folder() { name = "default" };

            if (!string.IsNullOrEmpty(c.name))
                this.name = c.name;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(c.sysname))
                this.sysname = c.sysname;
            else
                throw new Exception("A-OK, Handled.");

            if (c.created_on != null)
                this.created_on = c.created_on;
            else
                throw new Exception("A-OK, Handled.");

            if (c.modified != null)
                this.modified = c.modified;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(c.joinedprice))
                this.price = c.joinedprice;
            else
                throw new Exception("A-OK, Handled.");

            //if (!string.IsNullOrEmpty(p.status_delivery))
            //    this.status_delivery = p.status_delivery;
            //else
            //    throw new Exception();

            if (!string.IsNullOrEmpty(c.status_stock))
                this.status_stock = c.status_stock;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(c.status_condition))
                this.status_condition = c.status_condition;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(c.description))
                this.description = c.description;
            else
                this.description = "";

            if (!string.IsNullOrEmpty(c.note))
                this.note = c.note;
            else
                this.note = "";

            this.active = c.active;
            if (c.booth != null/* && withbooth*/)
            {
                this.booth_poco = new poco_booth();
                this.booth_poco.ToPoco(Null(c.booth), null);
            }
            if (c.image != null)
            {
                poco_collectionimage pi = new poco_collectionimage();
                this.image_pocos = new List<IImage>();
                this.image_pocos = pi.ToPocoList(c.image.ToList());
            }
            if (c.tag != null && c.tag.Count() > 0)
            {
                foreach (tag t2 in c.tag)
                    Null(t2);
                poco_tag t = new poco_tag();
                this.tag_pocos = new List<poco_tag>();
                this.tag_pocos = t.ToPocoList(c.tag.ToList());
            }

            ICollection<param> _params = null;
            if (c.collection_param != null && c.collection_param.Count() > 0)
                _params = c.collection_param.Select(x => x.param).ToList();//.GetTags(db);
            if (_params != null && _params.Count() > 0)
            {
                List<param> ps = new List<param>();
                foreach (param par in _params)
                {
                    List<value> list = new List<value>();
                    foreach (value v in c.collection_param.Select(x => x.value).Where(Statics.IsNotNull))
                    {
                        if (v.param_id == par.Id)
                            list.Add(v);
                    }
                    param _par = new param() { Id = par.Id, name = par.name, category_id = par.category_id, type = par.type, prio = par.prio, value1 = list };
                    ps.Add(_par);
                }
                poco_params pa = new poco_params();
                this.param_daos = new List<poco_params>();
                this.param_daos = pa.ToPOCO_List(ps.ToList());
            }

            if (c.conversation != null && c.conversation.Count() > 0/* && withconversations*/)
            {
                poco_conversation poco = new poco_conversation();
                this.conversations = poco.ToPocoList(c.conversation);
            }
            if (c.product != null/* && withproducts*/)
            {
                this.product_pocos = new List<poco_product>();
                foreach (product p in c.product)
                {
                    poco_product poco = new poco_product(false);
                    poco.ToPoco(p, null, "");
                    this.product_pocos.Add(poco);
                }
            }
            this.relevant = rel != null ? rel.Where(x => x.booth == booth && x.product == this.name).Count() > 0 : false;
            //if (set_relevant)
            //    this.IsRelevant(c, true, false);
        }

        public List<poco_collection> ToPocoList(ICollection<collection> collections, List<Hit> rel, string booth)
        {
            if (collections == null)
                throw new Exception("A-OK, Check.");

            List<poco_collection> res = new List<poco_collection>();
            foreach (collection c in collections.ToList())
            {
                poco_collection poco = new poco_collection();
                poco.ToPoco(c, rel, booth);
                poco.SetupToClient(c);
                res.Add(poco);
            }
            return res;
        }

        public void ToCollection(bool new_collection, ref collection col, EbazarDB _db = null)
        {
            if (_db == null)
                _db = DAL.GetInstance().GetContext();

            DateTime now = DateTime.Now;
            if (this.category_main_id != null)
            {
                booth boo = _db.booth.Where(b => b.Id == this.booth_id).FirstOrDefault();
                if (boo.category_main.Where(cat => cat.Id == this.category_main_id).Count() == 0)
                {
                    category new_cat = _db.category.Where(cat => cat.Id == this.category_main_id).FirstOrDefault();
                    boo.category_main.Add(new_cat);
                }

                int? old_cat_id = col.category_main_id != this.category_main_id ? (int?)col.category_main_id : null;
                int? old_cat_sec_id = col.category_second_id != this.category_second_id ? (int?)col.category_second_id : null;
                if (col.category_main_id == 0)//opret
                {
                    //category main = db.category.Where(c => c.Id == this.category_main_id).FirstOrDefault();
                    //category sec = main.children.OrderBy(c => c.priority).ElementAt(main.children.Count() - 1);

                    //col.category_main_id = main.Id;
                    //col.category_second_id = sec.Id;

                    category main = _db.category.Include("children").Where(c => c.Id == this.category_main_id && c.is_parent).FirstOrDefault();
                    category sec = main.children.OrderBy(x => x.priority).FirstOrDefault();

                    col.category_main_id = main.Id;
                    col.category_second_id = sec.Id;

                    poco_category.update = true;
                }
                else if (old_cat_id != null)//vi bytter
                {
                    category old_cat = _db.category.Where(cat => cat.Id == old_cat_id).FirstOrDefault();
                    old_cat.collection_main.Remove(col);
                    if (old_cat.product_main.Where(cat => cat.booth_id == boo.Id).Count() + old_cat.collection_main.Where(cat => cat.booth_id == boo.Id).Count() == 0)
                        boo.category_main.Remove(old_cat);

                    category main = _db.category.Where(c => c.Id == this.category_main_id).FirstOrDefault();
                    category sec = main.children.OrderBy(c => c.priority).ElementAt(main.children.Count() - 1);

                    col.category_main_id = main.Id;
                    col.category_second_id = sec.Id;

                    poco_category.update = true;
                }
                else if (old_cat_sec_id != null)//cat 2 er blevet ændret
                {
                    col.category_main_id = this.category_main_id;
                    col.category_second_id = this.category_second_id;

                    poco_category.update = true;
                }
                else//der er ikke blevet ændret i kategorier
                {
                    col.category_main_id = this.category_main_id;
                    col.category_second_id = this.category_second_id;
                }
            }
            else
                throw new Exception("A-OK, Handled.");

            //col.category_second_id = this.category_second_id;

            if (this.foldera != null)
                col.foldera = _db.folder.Where(l => l.Id == this.foldera.id).FirstOrDefault();

            if (this.folderb != null)
                col.folderb = _db.folder.Where(l => l.Id == this.folderb.id).FirstOrDefault();

            if (!string.IsNullOrEmpty(this.name))
                col.name = this.name;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(this.sysname))
                col.sysname = this.sysname;
            else
                throw new Exception("A-OK, Handled.");

            if (!new_collection && this.created_on != null)
                col.created_on = this.created_on;
            else
                col.created_on = now;

            col.modified = now;

            if (!string.IsNullOrEmpty(this.price.ToString()))
                col.joinedprice = this.price;
            else
                throw new Exception("A-OK, Handled.");

            //if (this.status_delivery != null)
            //c.status_delivery = this.status_delivery;
            //else
            //    throw new Exception();

            if (this.status_stock != null)
                col.status_stock = this.status_stock;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(this.status_condition))
                col.status_condition = this.status_condition;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(this.description))
                col.description = this.description;
            else
                col.description = "";

            if (!string.IsNullOrEmpty(this.note))
                col.note = this.note;
            else
                col.note = "";

            col.active = this.active;

            if (this.booth_poco != null)
            {
                col.booth = _db.booth.Where(b => b.Id == this.booth_poco.booth_id).FirstOrDefault();
                if (new_collection)
                    col.booth.modified = now;
            }
            else
                throw new Exception("A-OK, Handled.");
        }
    }
}