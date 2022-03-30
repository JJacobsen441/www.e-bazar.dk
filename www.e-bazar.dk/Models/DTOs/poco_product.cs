using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

            List<product_param> pars = _db.product_param
                .Include("param")
                .Include("value")
                .Where(x => x.product_id != null && x.product_id == pr_id)
                .ToList();
            if (pars == null)
                throw new Exception("A-OK, Handled.");
            return pars;
        }

        public product GetProduct(long product_id, bool withbooth, bool withproducts, bool withconversations, bool withtag)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            _db.Configuration.ProxyCreationEnabled = false;
            _db.Configuration.LazyLoadingEnabled = false;

            IQueryable<product> _p = _db.product
                                            .Include("booth")
                                            .Include("booth.person")
                                            .Include("booth.category_main")
                                            .Include("category_main")
                                            .Include("category_second")
                                            .Include("foldera")
                                            .Include("folderb")
                                            .Include("image")
                                            .Include("tag")
                                            .Where(x=>x.Id == product_id);

            product p = _p.AsEnumerable().FirstOrDefault();
            
            if (p.IsNull())
                throw new Exception("A-OK, Handled.");
            else
            {
                NullHelper.PNull(p, withbooth, withcollection, true, withtag, withconversations, true);
                p.product_param = IncludeParam(p.Id);
                return p;
            }
        }

        public poco_product GetProductPOCO(long product_id, bool withbooth, bool withproducts, bool withconversations, bool withtags)
        {
            poco_product pro_poco = new poco_product(true);
            product product = GetProduct(product_id, withbooth, withproducts, withconversations, withtags);

            pro_poco.ToPoco(product, null, "");
            pro_poco.SetupToClient<poco_product>();
            return pro_poco;

        }

        public List<product> GetProducts(int booth_id, string la_search, string lb_search, bool select_inactive, bool withbooth, bool overrideonlycollection, bool withtags, bool withfoldera, bool withdefault)
        {
            la_search = la_search == null ? "" : la_search;
            lb_search = lb_search == null ? "" : lb_search;

            EbazarDB _db = DAL.GetInstance().GetContext();

            _db.Configuration.ProxyCreationEnabled = false;
            _db.Configuration.LazyLoadingEnabled = false;

            IQueryable<product> _p = _db.product
                                            .Include("booth")
                                            .Include("booth.person")
                                            .Include("category_main")
                                            .Include("category_second")
                                            .Include("foldera")
                                            .Include("folderb")
                                            .Include("image")
                                            .Include("tag")
                                            .Where(x=>x.booth_id == booth_id && (!x.only_collection || overrideonlycollection) && (x.active || select_inactive) &&
                                            ((la_search == "" && lb_search == "") ||
                                            (la_search != "" && x.foldera.name == la_search && lb_search == "" && x.folderb == null) ||
                                            (lb_search != "" && x.folderb.name == lb_search) ||
                                            (la_search != "" && x.foldera.name == la_search && lb_search == "")));

            IEnumerable<product> p = _p.AsEnumerable().ToList();
            
            if (p.IsNull())
                throw new Exception("A-OK, Handled.");

            if (p.Any())
            {
                NullHelper.PNull(p.ToList(), withbooth, withcollection, true, withtags, false, true);
                return p.ToList();
            }
            return new List<product>();
        }

        public List<poco_product> GetProductPOCOs(int booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withbooth, bool overrideonlycollection, bool withtags, bool withfoldera, bool withdefault)
        {
            List<product> products = GetProducts(booth_id, lev_a_search, lev_b_search, select_inactive, withbooth, overrideonlycollection, withtags, withfoldera, withdefault);
                        
            return this.ToPocoList(products, null, "");
        }

        public List<product> GetProductsByCollectionId(int collection_id, bool withbooth, bool withtags)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            _db.Configuration.ProxyCreationEnabled = false;
            _db.Configuration.LazyLoadingEnabled = false;

            IQueryable<product> _p = _db.product
                                            .Include("booth")
                                            .Include("collection")
                                            .Include("image")
                                            .Include("tag")
                                            .Where(x=>x.collection != null && x.collection_id == collection_id);

            IEnumerable<product> p = _p.AsEnumerable().ToList();

            if (p.IsNull())
                throw new Exception("A-OK, Handled.");

            if (p.Any())
            {
                NullHelper.PNull(p.ToList(), withbooth, withcollection, false, withtags, false, false);

                return p.ToList();
            }
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
            EbazarDB _db1 = DAL.GetInstance().GetContext();

            product p = new product();
            this.ToProduct(true, ref p, _db1);
            _db1.product.Add(p);
            _db1.SaveChanges();

            
            //EbazarDB _db2 = DAL.GetInstance().GetContext();
            //FixCats(_db2, p, true);
            //_db2.SaveChanges();

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

        public override void Delete(long id, EbazarDB _db) { DeleteProduct(id, _db); }
        private void DeleteProduct(long id, EbazarDB _db)
        {
            if (_db.IsNull())
                throw new Exception("A-OK, Handled.");

            product p = _db.product
                .Include("tag")
                .Include("tag.product")
                .Include("tag.collection")
                .Include("image")
                .Include("favorites")
                .Include("conversation")
                .Include("conversation.comment")
                .Include("product_param")
                .Include("booth")
                .Include("category_main")
                .Include("category_second")
                .Where(pr => pr.Id == id).FirstOrDefault();
            if (p.IsNull())
                throw new Exception("A-OK, Handled.");

            foreach (image pi in p.image.ToList())
            {
                _db.image.Remove(pi);
            }

            foreach (person per in p.favorites.ToList())
            {
                per.favorites_product.Remove(p);
                p.favorites.Remove(per);
            }

            foreach (conversation c in p.conversation.ToList())
            {
                foreach (comment _c in c.comment.ToList())
                {
                    _db.comment.Remove(_c);
                }
                _db.conversation.Remove(c);
            }

            foreach (tag t in p.tag.ToList())//kig RemoveTag nedenfor
            {
                p.tag.Remove(t);
                t.product.Remove(p);
                if ((t.collection.Count) < 1 && t.product.Count < 1)
                    _db.tag.Remove(t);
            }

            foreach (product_param pp in p.product_param.ToList())
            {
                _db.product_param.Remove(pp);
            }

            booth b = p.booth;
            
            category main = p.category_main;
            main.product_main.Remove(p);
            category sec = p.category_second;
            sec.product_second.Remove(p);

            if (main.product_main.Where(c => c.booth_id == b.Id).Count() + main.collection_main.Where(c => c.booth_id == b.Id).Count() == 0)
                b.category_main.Remove(main);
            
            _db.product.Remove(p);
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

        public void FixCats(EbazarDB _db, product pro, bool create)
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
            if (pro.category_main_id == 0 || create)//opret
            //if (create)//opret
            {
                    category main = _db.category.Include("children").Where(c => c.name != ".ingen" && c.is_parent).FirstOrDefault();
                    category sec = main.children.OrderBy(x => x.priority).FirstOrDefault();
                 
                    pro.category_main_id = main.Id;
                    pro.category_second_id = sec.Id;

                //if(pro.Id != 0)
                {
                    //if(!create)
                    //main.product_main.Add(pro);
                    //if(!create)
                    //sec.product_second.Add(pro);
                }

                    poco_category.update = true;
            }
            else if (old_cat_id != null)//vi bytter
            {
                ///JUST TEST///
                product a = _db.product.Where(z => z.Id == this.id).FirstOrDefault();
                List<category> list_main = _db.category.Where(x => x.product_main.Where(z => z.Id == a.Id).Count() > 0).ToList();
                List<category> list_sec = _db.category.Where(x => x.product_second.Where(z => z.Id == a.Id).Count() > 0).ToList();
                ///JUST TEST///

                category old_cat = _db.category.Where(c => c.Id == old_cat_id).FirstOrDefault();
                //old_cat.product_main.Remove(pro);

                //category old_cat_sec = _db.category.Where(c => c.Id == this.category_second_id).FirstOrDefault();
                //old_cat_sec.product_second.Remove(pro);

                if (old_cat.product_main.Where(c => c.booth_id == boo.Id).Count() + old_cat.collection_main.Where(c => c.booth_id == boo.Id).Count() == 0)
                    boo.category_main.Remove(old_cat);

                category main = _db.category.Where(c => c.Id == this.category_main_id).FirstOrDefault();
                category sec = main.children.OrderBy(c => c.priority).ElementAt(main.children.Count() - 1);

                pro.category_main_id = main.Id;
                pro.category_second_id = sec.Id;

                //main.product_main.Add(pro);
                //sec.product_second.Add(pro);

                poco_category.update = true;
            }
            else if (old_cat_sec_id != null)//cat 2 er blevet ændret
            {
                //category old_cat_sec = _db.category.Where(c => c.Id == old_cat_sec_id).FirstOrDefault();
                //old_cat_sec.product_second.Remove(pro);
                //category new_cat_sec = _db.category.Where(c => c.Id == this.category_second_id).FirstOrDefault();
                //new_cat_sec.product_second.Add(pro);

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

        public void ToProduct(bool new_product, ref product pro, EbazarDB _db)
        {
            if (_db == null)
                throw new Exception("A-OK, Handled.");

            DateTime now = DateTime.Now;

            //if(pro.Id != 0)
                FixCats(_db, pro, false);

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