using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class poco_product : booth_item, IBoothItem
    {
        public poco_product()
        {
            withcollection = false;
        }
        public poco_product(bool withcollection)
        {
            this.withcollection = withcollection;
        }        

        private bool withcollection;
        
        
        [DisplayName("Antal enheder")]
        public int no_of_units { get; set; }
        public bool only_collection { get; set; }        
        
        public long? collection_id { get; set; }
        public virtual poco_collection collection { get; set; }
        


        private List<product_param> IncludeParam(long pr_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            //using (EbazarDB db = new EbazarDB())
            {

                List<product_param> pars = _db.product_param
                    .Include("param")
                    .Include("value")
                    .Where(x => x.product_id != null && x.product_id == pr_id)
                    .ToList();
                if (pars == null)
                    throw new Exception("A-OK, Handled.");
                return pars;
            }
            _db?.Dispose();
        }

        public product GetProduct(long product_id, bool withbooth, bool withproducts, bool withconversations, bool withtag)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            //nessesary objects
            poco_booth b_poco = new poco_booth();
            poco_productimage pi_poco = new poco_productimage();
            poco_collection col_poco = new poco_collection();
            poco_conversation con_poco = new poco_conversation();
            poco_tag t_poco = new poco_tag();

            product product = (from p in _db.product
                               join b in _db.booth on p.booth_id equals b.Id
                               where p.Id == product_id
                               select new
                               {
                                   Id = p.Id,
                                   category_main = p.category_main,
                                   category_second = p.category_second,
                                   category_main_id = p.category_main_id,
                                   category_second_id = p.category_second_id,
                                   name = p.name,
                                   sysname = p.sysname,
                                   created_on = p.created_on,
                                   modified = p.modified,
                                   price = p.price,
                                   status_stock = p.status_stock,
                                   status_condition = p.status_condition,
                                   description = p.description,
                                   note = p.note,
                                   no_of_units = p.no_of_units,
                                   only_collection = p.only_collection,
                                   active = p.active,
                                   image = p.image.OrderBy(i => i.created_on).ToList(),//pi_poco.GetProductImagePOCOs(p.id),
                                   booth_id = p.booth_id,
                                   collection_id = p.collection_id,
                                   foldera = p.foldera,
                                   folderb = p.folderb,
                                   folder_a_id = p.folder_a_id,
                                   folder_b_id = p.folder_b_id
                               }).AsEnumerable()/**/
            //select p).AsEnumerable()
            .Select(p => new product
            {
                Id = p.Id,
                category_main = p.category_main,
                category_second = p.category_second,
                category_main_id = p.category_main_id,
                category_second_id = p.category_second_id,
                name = p.name,
                sysname = p.sysname,
                created_on = p.created_on,
                modified = p.modified,
                price = p.price,
                status_stock = p.status_stock,
                status_condition = p.status_condition,
                description = p.description,
                note = p.note,
                no_of_units = p.no_of_units,
                only_collection = p.only_collection,
                active = p.active,
                image = p.image.OrderBy(i => i.created_on).ToList(),//pi_poco.GetProductImagePOCOs(p.id),
                tag = withtag ? t_poco.GetProductTags(p.Id) : null,
                booth_id = p.booth_id,
                booth = withbooth ? b_poco.GetBoothByProductId(p.Id) : null,
                conversation = withconversations ? con_poco.GetConversations(p.Id, -1, -1, TYPE.PRODUCT) : null,
                collection_id = p.collection_id,
                collection = withcollection ? col_poco.GetCollection(p.collection_id, withproducts, false, false, false, false) : null,
                foldera = p.foldera,//.fold_a,
                folderb = p.folderb,//.fold_b,
                folder_a_id = p.folder_a_id,//.fold_a_id,
                folder_b_id = p.folder_b_id,//.fold_b_id,
                product_param = IncludeParam(p.Id)
            }).FirstOrDefault();/**//*Cast<product>().*/
            if (product == null)
                throw new Exception("A-OK, Handled.");
            return product;
        }

        public poco_product GetProductPOCO(long product_id, bool withbooth, bool withproducts, bool withconversations, bool withtags)
        {
            poco_product pro_poco = new poco_product(true);
            product product = GetProduct(product_id, withbooth, withproducts, withconversations, withtags);

            //if (product.booth == null && withbooth)
            //    return null;
            pro_poco.ToPoco(product, null, "");
            pro_poco.SetupToClient(product);
            return pro_poco;

        }

        public List<product> GetProducts(int booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withbooth, bool overrideonlycollection, bool withtags, bool withfoldera, bool withdefault)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_booth b_poco = new poco_booth();
            poco_collection col_poco = new poco_collection();
            poco_tag t_poco = new poco_tag();

            folder default_a = new folder();
            default_a.Id = -1;
            default_a.name = "foldera_default";
            folder default_b = new folder();
            default_b.Id = -1;
            default_b.name = "folderb_default";

            lev_a_search = lev_a_search == null ? "" : lev_a_search;
            lev_b_search = lev_b_search == null ? "" : lev_b_search;
            IEnumerable<product> products = (from p in _db.product
                                             .Include("category_main")
                                             .Include("category_second")
                                             join la in _db.folder on p.folder_a_id equals la.Id into leva
                                             join lb in _db.folder on p.folder_b_id equals lb.Id into levb
                                             from la in leva.DefaultIfEmpty()
                                             from lb in levb.DefaultIfEmpty()
                                             where (p.booth_id == booth_id && (!p.only_collection || overrideonlycollection)) && (p.active || select_inactive) &&
                                             ((lev_a_search == "" && lev_b_search == "") ||
                                             (lev_a_search != "" && la.name == lev_a_search && lev_b_search == "" && p.folderb == null) ||
                                             (lev_b_search != "" && lb.name == lev_b_search) ||
                                             (lev_a_search != "" && la.name == lev_a_search && lev_b_search == "") ? true : false) ? true : false
                                             //orderby p.booth.category_main.Where(c => c.Id == category_main_id).FirstOrDefault().name ascending
                                             select new
                                             {
                                                 Id = p.Id,
                                                 category_main_id = p.category_main_id,
                                                 category_second_id = p.category_second_id,
                                                 category_main = p.category_main,
                                                 category_second = p.category_second,
                                                 name = p.name,
                                                 sysname = p.sysname,
                                                 created_on = p.created_on,
                                                 modified = p.modified,
                                                 price = p.price,
                                                 status_stock = p.status_stock,
                                                 status_condition = p.status_condition,
                                                 description = p.description,
                                                 note = p.note,
                                                 no_of_units = p.no_of_units,
                                                 only_collection = p.only_collection,
                                                 active = p.active,
                                                 booth_id = p.booth_id,
                                                 image = p.image.OrderBy(i => i.created_on).ToList(),
                                                 collection_id = withcollection ? p.collection_id : null
                                             }).AsEnumerable()
                                        .Select(p => new product
                                        {
                                            Id = p.Id,
                                            category_main_id = p.category_main_id,
                                            category_second_id = p.category_second_id,
                                            category_main = p.category_main,
                                            category_second = p.category_second,
                                            name = p.name,
                                            sysname = p.sysname,
                                            created_on = p.created_on,
                                            modified = p.modified,
                                            price = p.price,
                                            status_stock = p.status_stock,
                                            status_condition = p.status_condition,
                                            description = p.description,
                                            note = p.note,
                                            no_of_units = p.no_of_units,
                                            only_collection = p.only_collection,
                                            active = p.active,
                                            booth_id = p.booth_id,
                                            booth = withbooth ? b_poco.GetBooth(p.booth_id, "", "", true, false, false, true/*sætter den bare da der allerede er teget højde for det*/, withfoldera, false, withdefault) : null,
                                            image = p.image.OrderBy(i => i.created_on).ToList(),
                                            tag = withtags ? t_poco.GetProductTags(p.Id) : null,
                                            collection_id = withcollection ? p.collection_id : null,
                                            collection = withcollection ? col_poco.GetCollection(p.collection_id, false, false, false, false, false) : null
                                        }).ToList();
            if (products == null)
                throw new Exception("A-OK, Handled.");


            if (products.Any())
                return products.ToList();
            return new List<product>();
        }

        public List<poco_product> GetProductPOCOs(int booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withbooth, bool overrideonlycollection, bool withtags, bool withfoldera, bool withdefault)
        {
            List<product> products = GetProducts(booth_id, lev_a_search, lev_b_search, select_inactive, withbooth, overrideonlycollection, withtags, withfoldera, withdefault);

            /*poco_product pro_relevant = null;
            if (set_relevant)
            {
                pro_relevant = new poco_product(null, false);
                pro_relevant.SetupRelevant();
            }*/
            return this.ToPocoList(products, null, "");

        }

        public List<product> GetProductsByCollectionId(int collection_id, bool withbooth, bool withtags)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_booth b_poco = new poco_booth();
            poco_collection col_poco = new poco_collection();
            poco_tag t_poco = new poco_tag();
            //poco_levela lev_a = new poco_levela() { db = new EbazarDB() };
            //poco_levelb lev_b = new poco_levelb() { db = new EbazarDB() };

            IEnumerable<product> products = (from p in _db.product
                                             where p.collection != null && p.collection.Id == collection_id
                                             select new
                                             {
                                                 Id = p.Id,
                                                 category_main_id = p.category_main_id,
                                                 category_second_id = p.category_second_id,
                                                 name = p.name,
                                                 sysname = p.sysname,
                                                 created_on = p.created_on,
                                                 modified = p.modified,
                                                 price = p.price,
                                                 status_stock = p.status_stock,
                                                 status_condition = p.status_condition,
                                                 description = p.description,
                                                 note = p.note,
                                                 no_of_units = p.no_of_units,
                                                 only_collection = p.only_collection,
                                                 active = p.active,
                                                 booth_id = p.booth_id,
                                                 //booth = withbooth ? b_poco.GetBooth(booth_id, "", "", false, false, true, false, false) : null,
                                                 image = p.image.OrderBy(i => i.created_on).ToList(),//pi_poco.GetImages(p.Id),
                                                                                                     //tag = p.tag.ToList(),//t_poco.GetProductTags(p.Id),
                                                 collection_id = p.collection_id,
                                                 //collection = withcollection ? col_poco.GetCollection(p.collection_id, false, true, false) : null
                                             }).AsEnumerable()
                                      .Select(p => new product
                                      {
                                          Id = p.Id,
                                          category_main_id = p.category_main_id,
                                          category_second_id = p.category_second_id,
                                          name = p.name,
                                          sysname = p.sysname,
                                          created_on = p.created_on,
                                          modified = p.modified,
                                          price = p.price,
                                          //status_delivery = p.status_delivery,
                                          status_stock = p.status_stock,
                                          status_condition = p.status_condition,
                                          description = p.description,
                                          note = p.note,
                                          no_of_units = p.no_of_units,
                                          only_collection = p.only_collection,
                                          active = p.active,
                                          booth_id = p.booth_id,
                                          booth = withbooth ? b_poco.GetBooth(booth_id, "", "", true, false, false, true, false, false, false) : null,
                                          image = p.image.OrderBy(i => i.created_on).ToList(),//pi_poco.GetImages(p.Id),
                                          tag = withtags ? /*p.tag.ToList(),*/t_poco.GetProductTags(p.Id) : null,
                                          collection_id = p.collection_id,
                                          collection = withcollection ? col_poco.GetCollection(p.collection_id, false, true, false, false, false) : null
                                      });
            if (products == null)
                throw new Exception("A-OK, Handled.");

            if (products.Any())
                return products.ToList();
            return new List<product>();
        }

        public List<poco_product> GetProductPOCOsByCollectionId(int collection_id, bool withbooth, bool withtags)
        {
            List<product> products = GetProductsByCollectionId(collection_id, withbooth, withtags);

            return this.ToPocoList(products, null, "");

        }

        public override long Save() { return SaveProduct(); }
        private long SaveProduct()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            product p = new product();
            this.ToProduct(true, ref p, _db);
            p = _db.product.Add(p);

            _db.SaveChanges();
            _db.Dispose();

            return p.Id;
        }

        public override void Update() { UpdateProduct(); }
        private void UpdateProduct()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            product product = _db.product.Where(p => p.Id == this.id).FirstOrDefault();
            if (product == null)
                throw new Exception("A-OK, Check.");
            this.ToProduct(false, ref product, _db);

            _db.SaveChanges();
            _db.Dispose();
        }

        public override void Delete(long id, EbazarDB _db = null) { DeleteProduct(id, _db); }
        private void DeleteProduct(long id, EbazarDB _db = null)
        {
            if (_db == null)
                _db = DAL.GetInstance().GetContext();

            product p = _db.product.Where(pr => pr.Id == id).FirstOrDefault();
            if (p == null)
                throw new Exception("A-OK, Handled.");

            foreach (image pi in p.image.ToList())
                _db.image.Remove(pi);
            foreach (person per in p.favorites.ToList())
                p.favorites.Remove(per);
            foreach (conversation c in p.conversation.ToList())
            {
                for (int i = 0; i < c.comment.ToList().Count(); i++)
                {
                    comment co = _db.comment.ToList().ElementAt(i);
                    _db.comment.Remove(co);
                }
                _db.conversation.Remove(c);
            }
            foreach (tag t in p.tag.ToList())//kig RemoveTag nedenfor
            {
                p.tag.Remove(t);
                if ((t.collection.Count) < 1 && t.product.Count <= 1)
                    _db.tag.Remove(t);
            }

            booth b = p.booth;
            category main = p.category_main;
            main.product_main.Remove(p);
            if (main.product_main.Where(c => c.booth_id == b.Id).Count() + main.collection_main.Where(c => c.booth_id == b.Id).Count() == 0)
                b.category_main.Remove(main);
            _db.product.Remove(p);
            //db.SaveChanges();
        }

        public override bool RemoveTag(long tag_id, bool is_updating)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            bool ok = false;

            product product = _db.product.Where(p => p.Id == this.id).Select(p => p).FirstOrDefault();
            if (product == null)
                throw new Exception("A-OK, Handled.");

            tag tag = product.tag.Where(t => t.Id == tag_id).Select(t => t).FirstOrDefault();
            if (tag == null)
                ok = false;
            else
                ok = product.tag.Remove(tag);

            if (ok && (/*tag.booth.Count + */tag.collection.Count) < 1 && tag.product.Count <= 1)
                _db.tag.Remove(tag);
            _db.SaveChanges();

            return ok;
        }

        public bool RemoveParam(int param_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            product product = _db.product.Where(p => p.Id == this.id).Select(p => p).FirstOrDefault();
            if (product == null)
                return false;

            product_param pp = product.product_param.Where(ppa => ppa.param_id == param_id).FirstOrDefault();
            if (pp != null)
            {
                param param = _db.param
                    //.Include("product_param")
                    //.Include("collection_param")
                    .Where(t => t.Id == param_id).Select(t => t).FirstOrDefault();
                if (param != null)
                {
                    product.product_param.Remove(pp);
                    param.product_param.Remove(pp);
                    _db.product_param.Remove(pp);
                }
                _db.SaveChanges();
            }
            return true;
        }

        public void SetFolder(int fld_id, long product_id, TYPE type)//indstiller dropdowns
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            product product = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
            if (product == null)
                throw new Exception("A-OK, Handled.");

            if (fld_id == -1)
            {
                if (type == TYPE.FOLDER_A && product.foldera != null)
                    product.foldera.product.Remove(product);
                if (product.folderb != null)
                    product.folderb.product.Remove(product);
            }
            else
            {
                if (type == TYPE.FOLDER_A)
                {
                    product.foldera = _db.folder.Where(l => l.Id == fld_id).FirstOrDefault();
                    if (product.folderb != null)
                    {
                        folder fldb = _db.folder.Where(l => l.Id == product.folderb.Id).FirstOrDefault();
                        if (fldb != null)
                        {
                            if (fldb.product != null)
                                fldb.product.Remove(product);
                        }
                        product.folderb = null;
                    }
                }
                else
                    product.folderb = _db.folder.Where(l => l.Id == fld_id).FirstOrDefault();

            }
            _db.SaveChanges();
        }

        public override void RemoveImage(string image_name) { RemoveProductImage(image_name); }
        private void RemoveProductImage(string image_name)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            if (string.IsNullOrEmpty(image_name))
                throw new Exception("A-OK, Check.");

            image productimage = _db.image.Where(p => p.name == image_name && p.product_id == this.id).Select(t => t).FirstOrDefault();
            if (productimage == null)
                throw new Exception("A-OK, Handled.");

            product product = _db.product.Where(p => p.Id == this.id).Select(p => p).FirstOrDefault();
            if (product == null)
                throw new Exception("A-OK, Handled.");

            product.image.Remove(productimage);
            _db.image.Remove(productimage);
            _db.SaveChanges();
            _db.Dispose();
        }

        public override poco_booth GetBoothPOCO()
        {
            poco_booth booth_poco = new poco_booth();
            booth boo = booth_poco.GetBooth(this.booth_id, "", "", false, false, false, false, false, false, false);

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

        //public bool IsRelevant(product pro, bool counting, string b_name, Helpers helper)
        //{
        //    if (pro == null)
        //        throw new Exception("A-OK, Check.");

        //    string[] opt = helper.opt;
        //    string op1 = helper.opt[0], op2 = helper.opt[0], op3 = helper.opt[0], op4 = helper.opt[0], op5 = helper.opt[0], op6 = helper.opt[0];
        //    string[] cats = helper.cats;
        //    string cat = helper.cat;
        //    int fra = helper.fra, til = helper.til, zip = helper.zip;
            
        //    cat_main = pro.category_main.name;
        //    cat_second = pro.category_second != null ? pro.category_second.name : null;

        //    bool ok;
        //    string desc = StringHelper.OnlyAlphanumeric(pro.description.ToLower().Trim(), false, false, "notag", Statics.Characters.Space(), out ok);

        //    bool relevantA = pro.price == NOP.INGEN_PRIS.ToString() ?
        //                    pro.active && !string.IsNullOrEmpty(pro.price) && true :
        //                    (pro.active && !string.IsNullOrEmpty(pro.price) && int.Parse(pro.price) >= fra && int.Parse(pro.price) <= til);

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

        //public bool IsRelevant(product pro, string b_name, Helpers helper)
        //{
        //    bool relevant = CheckParam(pro, helper);
        //    if (relevant)
        //        this.relevant_hits.Add(new Hit() { booth = b_name, product = pro.name });
        //    this.relevant = relevant;
        //    return relevant;
        //}

        //public bool CheckParam(product pro, Helpers helper)
        //{
        //    if (pro.IsNull())
        //        throw new Exception("A-OK, Check.");


        //    string[] opt = helper.opt;
        //    /*
        //     * HACK - just a precaution
        //     * */
        //    EbazarDB _db = DAL.GetInstance().GetDB();
        //    if (pro.product_param == null || !pro.product_param.Any())
        //        pro.product_param = _db.product_param.Where(x => x.product_id == pro.Id).ToList();

        //    bool relevantF = false;

        //    if (ThisSession.Params.Count == 0)
        //        relevantF = true;

        //    List<param> l1 = null;
        //    List<value> l2 = null;

        //    if (!relevantF)
        //    {
        //        l1 = pro.product_param.Select(x => x.param).ToList();
        //        l2 = pro.product_param.Select(x => x.value).ToList();

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
        //}

        public void SetupToClient(product product)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            if (this.booth_poco != null)
            {
                if (product != null && product.category_main != null)
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

                if (product != null)
                {
                    folder tmpa = product.foldera;

                    if (lista.Count() > 0 && tmpa != null)
                        this.foldera_selectlist = new SelectList(lista, "Id", "name", tmpa.Id);
                    else
                        this.foldera_selectlist = new SelectList(lista, "Id", "name", -1);

                    List<folder> listb = new List<folder>() { new folder() { Id = -1, name = "ingen.." } };
                    if (tmpa != null)
                    {
                        listb = listb.Concat(_db.folder.Where(l => l.parent_id == tmpa.Id).OrderBy(l => l.priority)).ToList();
                        //folder tmpb = _db.product.Where(p => p.Id == this.id).FirstOrDefault().folderb;
                        folder tmpb = product.folderb;
                        if (listb.Count() > 0 && tmpb != null)
                            this.folderb_selectlist = new SelectList(listb, "Id", "name", tmpb.Id);
                        else
                            this.folderb_selectlist = new SelectList(listb, "Id", "name", -1);
                    }
                    else
                        this.folderb_selectlist = new SelectList(listb, "Id", "name", -1);
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
            if (bth.product != null)
                bth.product = null;
            //if (bth.region != null)
            //    bth.region = null;

            return bth;
        }

        public void ToPoco(product p, List<Hit> rel, string booth)
        {
            if (p == null)
                throw new Exception("A-OK, Check.");

            this.id = p.Id;
            this.booth_id = p.booth_id;

            this.category_main_id = p.category_main_id;
            this.category_second_id = p.category_second_id;
            this.collection_id = p.collection_id;

            if (p.foldera != null)
            {
                poco_folder flda = new poco_folder();
                this.foldera = flda.GetFolderAPOCO(p.foldera.Id, false, false);
            }
            else
                this.foldera = new poco_folder() { name = "default" };

            if (p.folderb != null)
            {
                poco_folder fldb = new poco_folder();
                this.folderb = fldb.GetFolderBPOCO(p.folderb.Id);
            }
            else
                this.folderb = new poco_folder() { name = "default" };

            if (!string.IsNullOrEmpty(p.name))
                this.name = p.name;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(p.sysname))
                this.sysname = p.sysname;
            else
                throw new Exception("A-OK, Handled.");

            if (p.created_on != null)
                this.created_on = p.created_on;
            else
                throw new Exception("A-OK, Handled.");

            if (p.modified != null)
                this.modified = p.modified;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(p.price))
                this.price = p.price;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(p.status_stock))
                this.status_stock = p.status_stock;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(p.status_condition))
                this.status_condition = p.status_condition;
            else
                throw new Exception("A-OK, Handled.");

            this.only_collection = p.only_collection;
            this.active = p.active;

            if (p.no_of_units > 0)
                this.no_of_units = p.no_of_units;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(p.description))
                this.description = p.description;
            else
                this.description = "";

            if (!string.IsNullOrEmpty(p.note))
                this.note = p.note;
            else
                this.note = "";

            if (p.booth != null/* && withbooth*/)
            {
                this.booth_poco = new poco_booth();
                this.booth_poco.ToPoco(Null(p.booth), null);
            }

            if (p.image != null)
            {
                poco_productimage pi = new poco_productimage();
                this.image_pocos = new List<IImage>();
                this.image_pocos = pi.ToPocoList(p.image);
            }

            if (p.tag != null && p.tag.Count() > 0)
            {
                foreach (tag t2 in p.tag)
                    Null(t2);
                poco_tag t = new poco_tag();
                this.tag_pocos = new List<poco_tag>();
                this.tag_pocos = t.ToPocoList(p.tag);
            }

            ICollection<param> _params = null;
            if (p.product_param != null && p.product_param.Count() > 0)
                _params = p.product_param.Select(x => x.param).ToList();//.GetTags(db);
            if (_params != null && _params.Count() > 0)
            {
                List<param> ps = new List<param>();
                foreach (param par in _params)
                {
                    List<value> list = new List<value>();
                    foreach (value v in p.product_param.Select(x => x.value).Where(Statics.IsNotNull))
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

            if (p.collection != null)
            {
                this.collection = new poco_collection();
                this.collection.ToPoco(p.collection, null, "");
            }
            //if (p.booth != null/* && withbooth*/)
            //{
            //    this.booth_poco = new poco_booth(null);
            //    this.booth_poco.ToPoco(p.booth/*, withsalesman*/);
            //}
            if (p.conversation != null && p.conversation.Count() > 0/* && withconversations*/)
            {
                poco_conversation poco = new poco_conversation();
                this.conversations = poco.ToPocoList(p.conversation);
            }

            this.relevant = rel != null ? rel.Where(x => x.booth == booth && x.product == this.name).Count() > 0 : false;
            //if (set_relevant)
            //    this.IsRelevant(p, true, false);
        }

        public List<poco_product> ToPocoList(ICollection<product> products, List<Hit> rel, string booth)
        {
            if (products == null)
                throw new Exception("A-OK, Check.");

            List<poco_product> res = new List<poco_product>();
            foreach (product p in products.ToList())
            {
                poco_product poco = new poco_product(false);
                poco.ToPoco(p, rel, booth);
                poco.SetupToClient(p);
                res.Add(poco);
            }
            return res;
        }

        public void ToProduct(bool new_product, ref product pro, EbazarDB _db = null)
        {
            if (_db == null)
                _db = DAL.GetInstance().GetContext();

            DateTime now = DateTime.Now;


            if (this.category_main_id != null)
            {
                booth boo = _db.booth.Where(b => b.Id == this.booth_id).FirstOrDefault();
                if (boo.category_main.Where(c => c.Id == this.category_main_id).Count() == 0)
                {
                    category new_cat = _db.category.Where(c => c.Id == this.category_main_id).FirstOrDefault();
                    boo.category_main.Add(new_cat);
                }
                int? old_cat_id = pro.category_main_id != this.category_main_id ? (int?)pro.category_main_id : null;
                int? old_cat_sec_id = pro.category_second_id != this.category_second_id ? (int?)pro.category_second_id : null;
                if (pro.category_main_id == 0)//opret
                {
                    category main = _db.category.Include("children").Where(c => c.Id == this.category_main_id && c.is_parent).FirstOrDefault();
                    category sec = main.children.OrderBy(x => x.priority).FirstOrDefault();

                    pro.category_main_id = main.Id;
                    pro.category_second_id = sec.Id;

                    poco_category.update = true;
                }
                else if (old_cat_id != null)//vi bytter
                {
                    category old_cat = _db.category.Where(c => c.Id == old_cat_id).FirstOrDefault();
                    old_cat.product_main.Remove(pro);
                    if (old_cat.product_main.Where(c => c.booth_id == boo.Id).Count() + old_cat.collection_main.Where(c => c.booth_id == boo.Id).Count() == 0)
                        boo.category_main.Remove(old_cat);

                    category main = _db.category.Where(c => c.Id == this.category_main_id).FirstOrDefault();
                    category sec = main.children.OrderBy(c => c.priority).ElementAt(main.children.Count() - 1);

                    pro.category_main_id = main.Id;
                    pro.category_second_id = sec.Id;

                    poco_category.update = true;
                }
                else if (old_cat_sec_id != null)//cat 2 er blevet ændret
                {
                    pro.category_main_id = this.category_main_id;
                    pro.category_second_id = this.category_second_id;

                    poco_category.update = true;
                }
                else//der er ikke blevet ændret i kategorier
                {
                    pro.category_main_id = this.category_main_id;
                    pro.category_second_id = this.category_second_id;
                }
            }
            else
                throw new Exception("A-OK, Handled.");

            //pro.category_second_id = this.category_second_id;

            if (this.foldera != null)
                pro.foldera = _db.folder.Where(l => l.Id == this.foldera.id).FirstOrDefault();


            if (this.folderb != null)
                pro.folderb = _db.folder.Where(l => l.Id == this.folderb.id).FirstOrDefault();

            if (!string.IsNullOrEmpty(this.name))
                pro.name = this.name;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(this.sysname))
                pro.sysname = this.sysname;
            else
                throw new Exception("A-OK, Handled.");

            if (!new_product && this.created_on != null)
                pro.created_on = this.created_on;
            else
                pro.created_on = now;

            pro.modified = now;

            if (!string.IsNullOrEmpty(this.price.ToString()))
                pro.price = this.price;
            else
                throw new Exception("A-OK, Handled.");

            //if (!string.IsNullOrEmpty(this.status_delivery))
            //    p.status_delivery = this.status_delivery;
            //else
            //    throw new Exception();

            if (!string.IsNullOrEmpty(this.status_stock))
                pro.status_stock = this.status_stock;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(this.status_condition))
                pro.status_condition = this.status_condition;
            else
                throw new Exception("A-OK, Handled.");

            pro.only_collection = this.only_collection;
            pro.active = this.active;

            if (this.no_of_units > 0)
                pro.no_of_units = this.no_of_units;
            else
                throw new Exception("A-OK, Handled.");

            if (!string.IsNullOrEmpty(this.description))
                pro.description = this.description;
            else
                pro.description = "";

            if (!string.IsNullOrEmpty(this.note))
                pro.note = this.note;
            else
                pro.note = "";

            if (this.booth_poco != null)
            {
                pro.booth = _db.booth.Where(b => b.Id == this.booth_poco.booth_id).FirstOrDefault();
                if (new_product)
                    pro.booth.modified = now;
            }
            else
                throw new Exception("A-OK, Handled.");
        }
    }
}