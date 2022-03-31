using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.Interfaces;
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
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
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
            if (collection_id.IsNull())
                return null;

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * I know virtual should be removed from model entitys, to make it eager load
                 * */

                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<collection> _c = _db.collection
                                                .Include("booth")
                                                .Include("booth.person")
                                                .Include("booth.category_main")
                                                .Include("booth.region")
                                                .Include("category_main")
                                                .Include("category_second")
                                                .Include("foldera")
                                                .Include("folderb")
                                                .Include("image")
                                                .Include("tag")
                                                .Where(x=> x.Id == collection_id);

                collection c = _c.AsEnumerable().FirstOrDefault();
                        
                if (c.IsNull())
                    throw new Exception("A-OK, Handled.");
                else
                {
                    NullHelper.ProNull(c, withbooth, true, withtags, withconvesations, true, withproducts);
                    c.collection_param = IncludeParam(c.Id);
                }
                return c;
            }
        }

        public poco_collection GetCollectionPOCO(long? collection_id, bool withproducts, bool withbooth, bool convesations, bool withboothsalesman, bool withtags)
        {
            poco_collection col_poco = new poco_collection();
            collection collection = GetCollection(collection_id, withproducts, withbooth, convesations, withboothsalesman, withtags);

            col_poco.ToPoco(collection, null, "");
            col_poco.SetupToClient<poco_collection>();
            return col_poco;//den kan godt være null, eller hvad?!?

        }

        public List<collection> GetCollections(int booth_id, string la_search, string lb_search, bool select_inactive, bool withbooth, bool withtags, bool withfoldera)
        {
            la_search = la_search == null ? "" : la_search;
            lb_search = lb_search == null ? "" : lb_search;

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * I know virtual should be removed from model entitys, to make it eager load
                 * */

                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<collection> _c = _db.collection
                                                    .Include("booth")
                                                    .Include("booth.person")
                                                    .Include("category_main")
                                                    .Include("category_second")
                                                    .Include("foldera")
                                                    .Include("folderb")
                                                    .Include("image")
                                                    .Include("tag")
                                                    .Where(x=> x.booth_id == booth_id && (x.active || select_inactive) &&
                                                      ((la_search == "" && lb_search == "") ||
                                                      (la_search != "" && x.foldera.name == la_search && lb_search == "" && x.folderb == null) ||
                                                      (lb_search != "" && x.folderb.name == lb_search) ||
                                                      (la_search != "" && x.foldera.name == la_search && lb_search == "")));

                IEnumerable<collection> c = _c.AsEnumerable().ToList();
            
                if (c.IsNull())
                    throw new Exception("A-OK, Handled.");

                if (c.Any())
                {
                    NullHelper.ProNull(c.ToList(), withbooth, true, withtags, false, true, false);
                    return c.ToList();
                }

                return new List<collection>();
            }
        }

        public List<poco_collection> GetCollectionPOCOs(int booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withbooth, bool withtags, bool withfoldera)
        {
            List<collection> collections = GetCollections(booth_id, lev_a_search, lev_b_search, select_inactive, withbooth, withtags, withfoldera);

            return this.ToPocoList(collections, null, "");
        }

        public override long Save() { return SaveCollection(); }
        private long SaveCollection()
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                collection c = new collection();
                this.ToCollection(true, ref c, _db);
                c = _db.collection.Add(c);
                _db.SaveChanges();

                //EbazarDB _db2 = DAL.GetInstance().GetContext();
                //FixCats(_db2, c, true);
                //_db2.SaveChanges();

                return c.Id;
            }
        }

        public override void Update() { UpdateCollection(); }
        private void UpdateCollection()
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                collection collection = _db.collection.Where(c => c.Id == (int)this.id).FirstOrDefault();
                if (collection == null)
                    throw new Exception("A-OK, Check.");
                this.ToCollection(false, ref collection, _db);

                _db.SaveChanges();
                _db.Dispose();
            }
        }

        public override void Delete(long id, EbazarDB _db) { DeleteCollection(id, _db); }
        private void DeleteCollection(long id, EbazarDB _db)
        {
            if (_db.IsNull())
                throw new Exception("A-OK, Handled.");

            collection col = _db.collection
                .Include("tag")
                .Include("tag.product")
                .Include("tag.collection")
                .Include("image")
                .Include("favorites")
                .Include("conversation")
                .Include("conversation.comment")
                .Include("collection_param")
                .Include("booth")
                .Include("category_main")
                .Include("category_second")
                .Where(c => c.Id == (int)id).FirstOrDefault();
            if (col.IsNull())
                throw new Exception("A-OK, Handled.");

            foreach (image ci in col.image.ToList())
            {
                _db.image.Remove(ci);
            }

            foreach (person per in col.favorites.ToList())
            {
                per.favorites_collection.Remove(col);
                col.favorites.Remove(per);
            }

            foreach (conversation c in col.conversation.ToList())
            {
                foreach (comment _c in c.comment.ToList())
                {
                    _db.comment.Remove(_c);
                }
                _db.conversation.Remove(c);
            }

            foreach (product pr in col.product.ToList())
            {
                col.product.Remove(pr);
            }

            foreach (tag t in col.tag.ToList())//kig RemoveTag nedenfor
            {
                col.tag.Remove(t);
                t.collection.Remove(col);
                if ((t.product.Count) < 1 && t.collection.Count < 1)
                    _db.tag.Remove(t);
            }

            foreach(collection_param cp in col.collection_param.ToList())
            {
                _db.collection_param.Remove(cp);
            }

            booth b = col.booth;

            category main = col.category_main;
            main.collection_main.Remove(col);

            category sec = col.category_second;
            sec.collection_second.Remove(col);

            if (main.product_main.Where(c => c.booth_id == b.Id).Count() + main.collection_main.Where(c => c.booth_id == b.Id).Count() == 0)
                b.category_main.Remove(main);

            _db.collection.Remove(col);
        }

        public override bool RemoveTag(long tag_id, bool is_updating)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


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
        }

        public bool RemoveParam(int param_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


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

        }

        public void SetFolder(int fld_id, long collection_id, TYPE type)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


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
        }

        public override void RemoveImage(string image_name) { RemoveCollectionImage(image_name); }
        private void RemoveCollectionImage(string image_name)
        {
            if (string.IsNullOrEmpty(image_name))
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


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
        }

        public void AddProduct(long product_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                product pro = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
                if (pro == null)
                    throw new Exception("A-OK, Handled.");
                collection collection = _db.collection.Where(c => c.Id == (int)this.id).FirstOrDefault();
                collection.product.Add(pro);
            }
        }

        public void RemoveProduct(long product_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                product pro = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
                if (pro == null)
                    throw new Exception("A-OK, Handled.");

                collection collection = _db.collection.Where(c => c.Id == (int)this.id).FirstOrDefault();
                collection.product.Remove(pro);
            }
        }

        public override poco_booth GetBoothPOCO()
        {
            poco_booth booth_poco = new poco_booth();
            booth boo = booth_poco.GetBooth(this.booth_id, "", "", false, false, false, false, false, false, true);

            booth_poco.ToPoco(boo, null);
            return booth_poco;

        }

        public void ToPoco(collection c, List<Hit> rel, string booth)
        {
            if (c.IsNull())
                throw new Exception("A-OK, Check.");

            //if (rel == null)
            //    throw new Exception("A-OK, Check.");

            //if (booth == null)
            //    throw new Exception("A-OK, Check.");

            this.id = c.Id;
            this.booth_id = c.booth_id;
            this.category_main_id = this.category_main_id != 0 ? this.category_main_id : c.category_main_id;
            this.category_second_id = this.category_second_id != 0 ? this.category_second_id : c.category_second_id;
            this.active = c.active;

            poco_folder flda = new poco_folder();
            this.foldera = c.foldera != null ? flda.GetFolderAPOCO(c.foldera.Id, false, false) : new poco_folder() { name = "default" };

            poco_folder fldb = new poco_folder();
            this.folderb = c.folderb != null ? fldb.GetFolderBPOCO(c.folderb.Id) : new poco_folder() { name = "default" };

            this.name = !c.name.IsNullOrEmpty() ? c.name : this.name;
            this.sysname = !c.sysname.IsNullOrEmpty() ? c.sysname : this.sysname;
            this.created_on = !c.created_on.IsNull() ? c.created_on : this.created_on;
            this.modified = !c.modified.IsNull() ? c.modified : this.modified;
            this.price = !c.joinedprice.IsNullOrEmpty() ? c.joinedprice : this.price;
            this.status_stock = !c.status_stock.IsNullOrEmpty() ? c.status_stock : this.status_stock;
            this.status_condition = !c.status_condition.IsNullOrEmpty() ? c.status_condition : this.status_condition;
            this.description = !c.description.IsNullOrEmpty() ? c.description : "";
            this.note = !c.note.IsNullOrEmpty() ? c.note : "";

            if (!c.booth.IsNull())
            {
                this.booth_poco = new poco_booth();
                this.booth_poco.ToPoco(NullHelper.ProNull(c.booth), null);
            }
            if (!c.image.IsNull())
            {
                poco_collectionimage pi = new poco_collectionimage();
                this.image_pocos = new List<IImage>();
                this.image_pocos = pi.ToPocoList(c.image.ToList());
            }
            if (!c.tag.IsNull() && c.tag.Count() > 0)
            {
                foreach (tag t2 in c.tag)
                    NullHelper.ProNull(t2);
                poco_tag t = new poco_tag();
                this.tag_pocos = new List<poco_tag>();
                this.tag_pocos = t.ToPocoList(c.tag.ToList());
            }

            ICollection<param> _params = null;
            if (!c.collection_param.IsNull() && c.collection_param.Count() > 0)
                _params = c.collection_param.Select(x => x.param).ToList();//.GetTags(db);
            if (!_params.IsNull() && _params.Count() > 0)
            {
                List<param> ps = new List<param>();
                foreach (param par in _params)
                {
                    List<value> list = new List<value>();
                    foreach (value v in c.collection_param.Select(x => x.value).Where(x=>x.IsNotNull()))
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

            if (!c.conversation.IsNull() && c.conversation.Count() > 0/* && withconversations*/)
            {
                poco_conversation poco = new poco_conversation();
                this.conversations = poco.ToPocoList(c.conversation);
            }
            if (!c.product.IsNull())
            {
                this.product_pocos = new List<poco_product>();
                foreach (product p in c.product)
                {
                    poco_product poco = new poco_product(false);
                    poco.ToPoco(p, null, "");
                    this.product_pocos.Add(poco);
                }
            }
            this.relevant = !rel.IsNull() ? rel.Where(x => x.booth == booth && x.product == this.name).Count() > 0 : false;
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
                poco.SetupToClient<poco_collection>();
                res.Add(poco);
            }
            return res;
        }

        public void FixCats(EbazarDB _db, collection col, bool create)
        {
            booth boo = _db.booth.Where(b => b.Id == this.booth_id).FirstOrDefault();
            if (boo.category_main.Where(cat => cat.Id == this.category_main_id).Count() == 0)
            {
                category new_cat = _db.category.Where(c => c.name != ".ingen" && c.is_parent && c.Id == this.category_main_id).FirstOrDefault();
                if (new_cat.IsNull())
                    throw new Exception("A-OK, Handled.");
                boo.category_main.Add(new_cat);
            }

            int? old_cat_id = col.category_main_id != this.category_main_id ? (int?)col.category_main_id : null;
            int? old_cat_sec_id = col.category_second_id != this.category_second_id ? (int?)col.category_second_id : null;
            if (col.category_main_id == 0 || create)//create
            {
                    category main = _db.category.Include("children").Where(c => c.name != ".ingen" && c.is_parent).FirstOrDefault();
                    category sec = main.children.OrderBy(x => x.priority).FirstOrDefault();

                    col.category_main_id = main.Id;
                    col.category_second_id = sec.Id;

                    poco_category.update = true;
            }
            else if (old_cat_id != null)//new cat, changing
            {
                ///JUST TEST///
                collection a = _db.collection.Where(z => z.Id == this.id).FirstOrDefault();
                List<category> list_main = _db.category.Where(x => x.collection_main.Where(z => z.Id == a.Id).Count() > 0).ToList();
                List<category> list_sec = _db.category.Where(x => x.collection_second.Where(z => z.Id == a.Id).Count() > 0).ToList();
                ///JUST TEST///

                category old_cat = _db.category.Where(cat => cat.Id == old_cat_id).FirstOrDefault();
                
                if (old_cat.product_main.Where(cat => cat.booth_id == boo.Id).Count() + old_cat.collection_main.Where(cat => cat.booth_id == boo.Id).Count() == 0)
                    boo.category_main.Remove(old_cat);

                category main = _db.category.Where(c => c.Id == this.category_main_id).FirstOrDefault();
                category sec = main.children.OrderBy(c => c.priority).ElementAt(main.children.Count() - 1);

                col.category_main_id = main.Id;
                col.category_second_id = sec.Id;

                poco_category.update = true;
            }
            else if (old_cat_sec_id != null)//cat 2 changed
            {
                col.category_main_id = this.category_main_id;
                col.category_second_id = this.category_second_id;

                poco_category.update = true;
            }
            else//no changes
            {
                col.category_main_id = this.category_main_id;
                col.category_second_id = this.category_second_id;
            }
        }

        public void ToCollection(bool new_collection, ref collection col, EbazarDB _db)
        {
            if (_db == null)
                throw new Exception("A-OK, Handled.");

            DateTime now = DateTime.Now;

            FixCats(_db, col, false);

            col.foldera = !this.foldera.IsNull() ? _db.folder.Where(l => l.Id == this.foldera.id).FirstOrDefault() : col.foldera;
            col.folderb = !this.folderb.IsNull() ? _db.folder.Where(l => l.Id == this.folderb.id).FirstOrDefault() : col.folderb;
            col.active = this.active;
            col.name = !this.name.IsNullOrEmpty() ? this.name : col.name;
            col.sysname = !this.sysname.IsNullOrEmpty() ? this.sysname : col.sysname;
            col.created_on = !new_collection && !this.created_on.IsNull() ? this.created_on : now;
            col.modified = now;
            col.joinedprice = !this.price.IsNullOrEmpty() ? this.price : col.joinedprice;
            col.status_stock = !this.status_stock.IsNullOrEmpty() ? this.status_stock : col.status_stock;
            col.status_condition = !this.status_condition.IsNullOrEmpty() ? this.status_condition : col.status_condition;
            col.description = !this.description.IsNullOrEmpty() ? this.description : "";
            col.note = !this.note.IsNullOrEmpty() ? this.note : "";

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