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
            pro_poco.SetupToClient<poco_product>();
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

        public void ToPoco(product p, List<Hit> rel, string booth)
        {
            if (p.IsNull())
                throw new Exception("A-OK, Check.");

            //if (rel == null)
            //    throw new Exception("A-OK, Check.");

            //if (booth == null)
            //    throw new Exception("A-OK, Check.");

            this.id = p.Id;
            this.booth_id = p.booth_id;
            this.category_main_id = this.category_main_id != 0 ? this.category_main_id : p.category_main_id;
            this.category_second_id = this.category_second_id != 0 ? this.category_second_id : p.category_second_id;
            this.collection_id = p.collection_id;
            this.only_collection = p.only_collection;
            this.active = p.active;

            poco_folder flda = new poco_folder();
            this.foldera = p.foldera != null ? flda.GetFolderAPOCO(p.foldera.Id, false, false) : new poco_folder() { name = "default" };

            poco_folder fldb = new poco_folder();
            this.folderb = p.folderb != null ? fldb.GetFolderBPOCO(p.folderb.Id) : new poco_folder() { name = "default" };

            this.name = !p.name.IsNullOrEmpty() ? p.name : this.name;
            this.sysname = !p.sysname.IsNullOrEmpty() ? p.sysname : this.sysname;
            this.created_on = !p.created_on.IsNull() ? p.created_on : this.created_on;
            this.modified = !p.modified.IsNull() ? p.modified : this.modified;
            this.price = !p.price.IsNullOrEmpty() ? p.price : this.price;
            this.status_stock = !p.status_stock.IsNullOrEmpty() ? p.status_stock : this.status_stock;
            this.status_condition = !p.status_condition.IsNullOrEmpty() ? p.status_condition : this.status_condition;
            this.no_of_units = p.no_of_units > 0 ? p.no_of_units : this.no_of_units;
            this.description = !p.description.IsNullOrEmpty() ? p.description : "";
            this.note = !p.note.IsNullOrEmpty() ? p.note : "";

            if (!p.booth.IsNull())
            {
                this.booth_poco = new poco_booth();
                this.booth_poco.ToPoco(NullHelper.PNull(p.booth), null);
            }

            if (!p.image.IsNull())
            {
                poco_productimage pi = new poco_productimage();
                this.image_pocos = new List<IImage>();
                this.image_pocos = pi.ToPocoList(p.image);
            }

            if (!p.tag.IsNull() && p.tag.Count() > 0)
            {
                foreach (tag t2 in p.tag)
                    NullHelper.PNull(t2);
                poco_tag t = new poco_tag();
                this.tag_pocos = new List<poco_tag>();
                this.tag_pocos = t.ToPocoList(p.tag);
            }

            ICollection<param> _params = null;
            if (!p.product_param.IsNull() && p.product_param.Count() > 0)
                _params = p.product_param.Select(x => x.param).ToList();//.GetTags(db);
            if (!_params.IsNull() && _params.Count() > 0)
            {
                List<param> ps = new List<param>();
                foreach (param par in _params)
                {
                    List<value> list = new List<value>();
                    foreach (value v in p.product_param.Select(x => x.value).Where(x=>x.IsNotNull()))
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

            if (!p.collection.IsNull())
            {
                this.collection = new poco_collection();
                this.collection.ToPoco(p.collection, null, "");
            }
            
            if (!p.conversation.IsNull() && p.conversation.Count() > 0/* && withconversations*/)
            {
                poco_conversation poco = new poco_conversation();
                this.conversations = poco.ToPocoList(p.conversation);
            }

            this.relevant = !rel.IsNull() ? rel.Where(x => x.booth == booth && x.product == this.name).Count() > 0 : false;
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
                poco.SetupToClient<poco_product>();
                res.Add(poco);
            }
            return res;
        }

        public void ToProduct(bool new_product, ref product pro, EbazarDB _db)
        {
            if (_db == null)
                throw new Exception("A-OK, Handled.");

            DateTime now = DateTime.Now;
                        
            {
                booth boo = _db.booth.Where(b => b.Id == this.booth_id).FirstOrDefault();
                if (boo.category_main.Where(c => c.Id == this.category_main_id).Count() == 0)
                {
                    category new_cat = _db.category.Where(c => c.name != ".ingen" && c.is_parent && c.Id == this.category_main_id).FirstOrDefault();
                    if (new_cat.IsNull())
                        throw new Exception("A-OK, Handled."); 
                    boo.category_main.Add(new_cat);
                }
                int? old_cat_id = pro.category_main_id != this.category_main_id ? (int?)pro.category_main_id : null;
                int? old_cat_sec_id = pro.category_second_id != this.category_second_id ? (int?)pro.category_second_id : null;
                if (pro.category_main_id == 0)//opret
                {
                    category main = _db.category.Include("children").Where(c => c.name != ".ingen" && c.is_parent).FirstOrDefault();
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

            pro.foldera = !this.foldera.IsNull() ? _db.folder.Where(l => l.Id == this.foldera.id).FirstOrDefault() : pro.foldera;
            pro.folderb = !this.folderb.IsNull() ? _db.folder.Where(l => l.Id == this.folderb.id).FirstOrDefault() : pro.folderb;
            pro.active = this.active;
            pro.name = !this.name.IsNullOrEmpty() ? this.name : pro.name;
            pro.sysname = !this.sysname.IsNullOrEmpty() ? this.sysname : pro.sysname;
            pro.created_on = !new_product && !this.created_on.IsNull() ? this.created_on : now;
            pro.modified = now;
            pro.price = !this.price.IsNullOrEmpty() ? this.price : pro.price;
            pro.status_stock = !this.status_stock.IsNullOrEmpty() ? this.status_stock : pro.status_stock;
            pro.status_condition = !this.status_condition.IsNullOrEmpty() ? this.status_condition : pro.status_condition;
            pro.description = !this.description.IsNullOrEmpty() ? this.description : "";
            pro.note = !this.note.IsNullOrEmpty() ? this.note : "";

            pro.only_collection = this.only_collection;
            pro.no_of_units = this.no_of_units > 0 ? this.no_of_units : pro.no_of_units;

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