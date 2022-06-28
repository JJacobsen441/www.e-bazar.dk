using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;
using static www.e_bazar.dk.Models.DTOs.dto_booth;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_product : biz_booth_item//, IBoothItem
    {
        public biz_product()
        {
            withcollection = false;
        }
        public biz_product(bool withcollection)
        {
            this.withcollection = withcollection;
        }        

        private bool withcollection;
        
        private List<product_param> IncludeParam(long pr_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
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
        }

        public product GetProduct(long product_id, bool withbooth, bool withproducts, bool withconversations, bool withtag)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * I know virtual should be removed from model entitys, to make it eager load
                 * */

                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<product> _p = _db.product
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
                                                .Where(x=>x.Id == product_id);

                product p = _p.AsEnumerable().FirstOrDefault();
            
                if (p.IsNull())
                    throw new Exception("A-OK, Handled.");
                else
                {
                    NullHelper.ProNull(p, withbooth, withcollection, true, withtag, withconversations, true);
                    p.product_param = IncludeParam(p.Id);
                    return p;
                }
            }
        }

        public dto_product GetProductDTO(long product_id, bool withbooth, bool withproducts, bool withconversations, bool withtags)
        {
            biz_product biz = new biz_product(true);
            dto_product dto = new dto_product();
            product product = GetProduct(product_id, withbooth, withproducts, withconversations, withtags);

            dto = biz.ToDTO(product, null, "");
            dto = biz.SetupToClient<dto_product>(dto);
            return dto;

        }

        public List<product> GetProducts(int booth_id, string la_search, string lb_search, bool select_inactive, bool withbooth, bool overrideonlycollection, bool withtags, bool withfoldera, bool withdefault)
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
                    NullHelper.ProNull(p.ToList(), withbooth, withcollection, true, withtags, false, true);
                    return p.ToList();
                }
                return new List<product>();
            }
        }

        public List<dto_product> GetProductDTOs(int booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withbooth, bool overrideonlycollection, bool withtags, bool withfoldera, bool withdefault)
        {
            List<product> products = GetProducts(booth_id, lev_a_search, lev_b_search, select_inactive, withbooth, overrideonlycollection, withtags, withfoldera, withdefault);
                        
            return this.ToDTOList(products, null, "");
        }

        public List<product> GetProductsByCollectionId(int collection_id, bool withbooth, bool withtags)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                /*
                 * I know virtual should be removed from model entitys, to make it eager load
                 * */

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
                    NullHelper.ProNull(p.ToList(), withbooth, withcollection, false, withtags, false, false);

                    return p.ToList();
                }
                return new List<product>();
            }
        }

        public List<dto_product> GetProductDTOsByCollectionId(int collection_id, bool withbooth, bool withtags)
        {
            List<product> products = GetProductsByCollectionId(collection_id, withbooth, withtags);

            return this.ToDTOList(products, null, "");

        }

        public override long Save<T>(T dto) { return SaveProduct(dto); }
        private long SaveProduct<T>(T dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                dto_product _p = dto as dto_product;
                product p = new product();
                this.ToProduct(true, _p, ref p, _db);
                _db.product.Add(p);
                _db.SaveChanges();

            
                //EbazarDB _db2 = DAL.GetInstance().GetContext();
                //FixCats(_db2, p, true);
                //_db2.SaveChanges();

                return p.Id;
            }
        }

        public override void Update<T>(T dto) { UpdateProduct(dto); }
        private void UpdateProduct<T>(T dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto_product _p = dto as dto_product;
                product product = _db.product.Where(p => p.Id == _p.id).FirstOrDefault();
                if (product == null)
                    throw new Exception("A-OK, Check.");
                this.ToProduct(false, _p, ref product, _db);

                _db.SaveChanges();
                _db.Dispose();
            }
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

        public override bool RemoveTag<T>(T dto, long tag_id, bool is_updating)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                bool ok = false;

                product product = _db.product.Where(p => p.Id == dto.id).Select(p => p).FirstOrDefault();
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
        }

        public override bool RemoveParam<T>(T dto, int param_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                product product = _db.product.Where(p => p.Id == dto.id).Select(p => p).FirstOrDefault();
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
        }

        public void SetFolder(int fld_id, long product_id, TYPE type)//indstiller dropdowns
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


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
        }

        public override void RemoveImage<T>(string image_name, T dto) { RemoveProductImage(image_name, dto); }
        private void RemoveProductImage<T>(string image_name, T dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto_product _p = dto as dto_product;

                if (string.IsNullOrEmpty(image_name))
                    throw new Exception("A-OK, Check.");

                image productimage = _db.image.Where(p => p.name == image_name && p.product_id == _p.id).Select(t => t).FirstOrDefault();
                if (productimage == null)
                    throw new Exception("A-OK, Handled.");

                product product = _db.product.Where(p => p.Id == _p.id).Select(p => p).FirstOrDefault();
                if (product == null)
                    throw new Exception("A-OK, Handled.");

                product.image.Remove(productimage);
                _db.image.Remove(productimage);
                _db.SaveChanges();
                _db.Dispose();
            }
        }

        public override dto_booth GetBoothDTO<T>(T dto)
        {
            biz_booth biz = new biz_booth();
            booth boo = biz.GetBooth(dto.booth_id, "", "", false, false, false, false, false, false, false);

            dto_booth _dto = biz.ToDTO(boo, null);
            return _dto;

        }

        public dto_product ToDTO(product p, List<Hit> rel, string booth)
        {
            if (p.IsNull())
                throw new Exception("A-OK, Check.");

            //if (rel == null)
            //    throw new Exception("A-OK, Check.");

            //if (booth == null)
            //    throw new Exception("A-OK, Check.");

            dto_product dto = new dto_product();

            dto.id = p.Id;
            dto.booth_id = p.booth_id;
            dto.category_main_id = dto.category_main_id != 0 ? dto.category_main_id : p.category_main_id;
            dto.category_second_id = dto.category_second_id != 0 ? dto.category_second_id : p.category_second_id;
            dto.collection_id = p.collection_id;
            dto.only_collection = p.only_collection;
            dto.active = p.active;

            biz_folder flda = new biz_folder();
            dto.foldera = p.foldera != null ? flda.GetFolderADTO(p.foldera.Id, false, false) : new dto_folder() { name = "default" };

            biz_folder fldb = new biz_folder();
            dto.folderb = p.folderb != null ? fldb.GetFolderBDTO(p.folderb.Id) : new dto_folder() { name = "default" };

            dto.name = !p.name.IsNullOrEmpty() ? p.name : dto.name;
            dto.sysname = !p.sysname.IsNullOrEmpty() ? p.sysname : dto.sysname;
            dto.created_on = !p.created_on.IsNull() ? p.created_on : dto.created_on;
            dto.modified = !p.modified.IsNull() ? p.modified : dto.modified;
            dto.price = !p.price.IsNullOrEmpty() ? p.price : dto.price;
            dto.status_stock = !p.status_stock.IsNullOrEmpty() ? p.status_stock : dto.status_stock;
            dto.status_condition = !p.status_condition.IsNullOrEmpty() ? p.status_condition : dto.status_condition;
            dto.no_of_units = p.no_of_units > 0 ? p.no_of_units : dto.no_of_units;
            dto.description = !p.description.IsNullOrEmpty() ? p.description : "";
            dto.note = !p.note.IsNullOrEmpty() ? p.note : "";

            if (!p.booth.IsNull())
            {
                biz_booth biz = new biz_booth();
                dto.booth_dto = biz.ToDTO(NullHelper.ProNull(p.booth), null);
            }

            if (!p.image.IsNull())
            {
                biz_image pi = new biz_image();
                dto.image_dtos = new List<dto_image>();
                dto.image_dtos = pi.ToDTOList(p.image);
            }

            if (!p.tag.IsNull() && p.tag.Count() > 0)
            {
                foreach (tag t2 in p.tag)
                    NullHelper.ProNull(t2);
                biz_tag t = new biz_tag();
                dto.tag_dtos = new List<dto_tag>();
                dto.tag_dtos = t.ToDTOList(p.tag);
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
                biz_params pa = new biz_params();
                dto.param_dtos = new List<dto_params>();
                dto.param_dtos = pa.ToDTO_List(ps.ToList());
            }

            if (!p.collection.IsNull())
            {
                biz_collection biz = new biz_collection();
                dto.collection = biz.ToDTO(p.collection, null, "");
            }
            
            if (!p.conversation.IsNull() && p.conversation.Count() > 0/* && withconversations*/)
            {
                biz_conversation biz = new biz_conversation();
                dto.conversations = biz.ToDTOList(p.conversation);
            }

            dto.relevant = !rel.IsNull() ? rel.Where(x => x.booth == booth && x.product == dto.name).Count() > 0 : false;

            return dto;
        }

        public List<dto_product> ToDTOList(ICollection<product> products, List<Hit> rel, string booth)
        {
            if (products == null)
                throw new Exception("A-OK, Check.");

            List<dto_product> res = new List<dto_product>();
            foreach (product p in products.ToList())
            {
                dto_product dto = new dto_product();
                dto = this.ToDTO(p, rel, booth);
                dto = this.SetupToClient<dto_product>(dto);
                res.Add(dto);
            }
            return res;
        }

        public void FixCats(EbazarDB _db, product pro, dto_product dto, bool create)
        {
            booth bth = _db.booth.Where(b => b.Id == dto.booth_id).FirstOrDefault();
            //if (boo.category_main.Where(c => c.Id == dto.category_main_id).Count() == 0)
            //{
            //    category new_cat = _db.category.Where(c => c.name != ".ingen" && c.is_parent && c.Id == dto.category_main_id).FirstOrDefault();
            //    if (new_cat.IsNull())
            //        throw new Exception("A-OK, Handled.");
            //    boo.category_main.Add(new_cat);
            //}
            int? old_cat_id = pro.category_main_id != dto.category_main_id ? (int?)pro.category_main_id : null;
            int? old_cat_sec_id = pro.category_second_id != dto.category_second_id ? (int?)pro.category_second_id : null;
            if (pro.category_main_id == 0 || create)//create
            {
                category main = _db.category.Include("children").Where(c => c.name != ".ingen" && c.is_parent).FirstOrDefault();
                category sec = main.children.OrderBy(x => x.priority).FirstOrDefault();
                 
                pro.category_main_id = main.Id;
                pro.category_second_id = sec.Id;

                biz_category.update = true;

                bth.category_main.Add(main);
            }
            else if (old_cat_id != null)//new cat, changing
            {
                ///JUST TEST///
                product a = _db.product.Where(z => z.Id == dto.id).FirstOrDefault();
                List<category> list_main = _db.category.Where(x => x.product_main.Where(z => z.Id == a.Id).Count() > 0).ToList();
                List<category> list_sec = _db.category.Where(x => x.product_second.Where(z => z.Id == a.Id).Count() > 0).ToList();
                ///JUST TEST///

                category old_cat = _db.category.Where(c => c.Id == old_cat_id).FirstOrDefault();
                
                if (old_cat.product_main.Where(c => c.booth_id == bth.Id).Count() + old_cat.collection_main.Where(c => c.booth_id == bth.Id).Count() == 0)
                    bth.category_main.Remove(old_cat);

                category main = _db.category.Where(c => c.Id == dto.category_main_id).FirstOrDefault();
                category sec = main.children.OrderBy(c => c.priority).ElementAt(main.children.Count() - 1);

                pro.category_main_id = main.Id;
                pro.category_second_id = sec.Id;
                bth.category_main.Add(main);

                biz_category.update = true;
            }
            else if (old_cat_sec_id != null)//cat 2 changed
            {
                pro.category_main_id = dto.category_main_id;
                pro.category_second_id = dto.category_second_id;

                biz_category.update = true;
            }
            else//no changes
            {
                pro.category_main_id = dto.category_main_id;
                pro.category_second_id = dto.category_second_id;
            }            
        }

        public void ToProduct(bool new_product, dto_product dto, ref product pro, EbazarDB _db)
        {
            if (_db == null)
                throw new Exception("A-OK, Handled.");

            DateTime now = DateTime.Now;

            FixCats(_db, pro, dto, false);

            pro.foldera = !dto.foldera.IsNull() ? _db.folder.Where(l => l.Id == dto.foldera.id).FirstOrDefault() : pro.foldera;
            pro.folderb = !dto.folderb.IsNull() ? _db.folder.Where(l => l.Id == dto.folderb.id).FirstOrDefault() : pro.folderb;
            pro.active = dto.active;
            pro.name = !dto.name.IsNullOrEmpty() ? dto.name : pro.name;
            pro.sysname = !dto.sysname.IsNullOrEmpty() ? dto.sysname : pro.sysname;
            pro.created_on = !new_product && !dto.created_on.IsNull() ? dto.created_on : now;
            pro.modified = now;
            pro.price = !dto.price.IsNullOrEmpty() ? dto.price : pro.price;
            pro.status_stock = !dto.status_stock.IsNullOrEmpty() ? dto.status_stock : pro.status_stock;
            pro.status_condition = !dto.status_condition.IsNullOrEmpty() ? dto.status_condition : pro.status_condition;
            pro.description = !dto.description.IsNullOrEmpty() ? dto.description : "";
            pro.note = !dto.note.IsNullOrEmpty() ? dto.note : "";

            pro.only_collection = dto.only_collection;
            pro.no_of_units = dto.no_of_units > 0 ? dto.no_of_units : pro.no_of_units;

            if (dto.booth_dto != null)
            {
                pro.booth = _db.booth.Where(b => b.Id == dto.booth_dto.booth_id).FirstOrDefault();
                if (new_product)
                    pro.booth.modified = now;
            }
            else
                throw new Exception("A-OK, Handled.");
        }
    }
}