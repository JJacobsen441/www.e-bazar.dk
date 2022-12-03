using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;
using static www.e_bazar.dk.Models.DTOs.dto_booth;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_collection : biz_booth_item//, IBoothItem
    {
        public biz_collection()
        {
        }
        
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
                                                .Include("booth.product")
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

        public dto_collection GetCollectionDTO(long? collection_id, bool withproducts, bool withbooth, bool convesations, bool withboothsalesman, bool withtags)
        {
            biz_collection biz = new biz_collection();
            dto_collection dto = new dto_collection();
            collection collection = GetCollection(collection_id, withproducts, withbooth, convesations, withboothsalesman, withtags);

            dto = biz.ToDTO(collection, null, "");
            dto = biz.SetupToClient<dto_collection>(dto);
            return dto;//den kan godt være null, eller hvad?!?

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

        public List<dto_collection> GetCollectionDTOs(int booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withbooth, bool withtags, bool withfoldera)
        {
            List<collection> collections = GetCollections(booth_id, lev_a_search, lev_b_search, select_inactive, withbooth, withtags, withfoldera);

            return this.ToDTOList(collections, null, "");
        }

        public override long Save<T>(T dto) { return SaveCollection(dto); }
        private long SaveCollection<T>(T dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                dto_collection _c = dto as dto_collection;
                collection c = new collection();
                this.ToCollection(true, _c, ref c, _db);
                c = _db.collection.Add(c);
                _db.SaveChanges();

                return c.Id;
            }
        }

        public override void Update<T>(T dto) { UpdateCollection(dto); }
        private void UpdateCollection<T>(T dto)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto_collection _c = dto as dto_collection;

                collection collection = _db.collection.Where(c => c.Id == (int)_c.id).FirstOrDefault();
                if (collection == null)
                    throw new Exception("A-OK, Check.");
                this.ToCollection(false, _c, ref collection, _db);

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

        public override bool RemoveTag<T>(T dto, long tag_id, bool is_updating)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                bool ok = false;

                collection collection = _db.collection.Where(p => p.Id == (int)dto.id).Select(p => p).FirstOrDefault();
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

        public override bool RemoveParam<T>(T dto, int param_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                collection collection = _db.collection.Where(p => p.Id == dto.id).Select(p => p).FirstOrDefault();
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

        public override void RemoveImage<T>(string image_name, T dto) { RemoveCollectionImage(image_name, dto); }
        private void RemoveCollectionImage<T>(string image_name, T dto)
        {
            if (string.IsNullOrEmpty(image_name))
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                dto_collection _c = dto as dto_collection;

                image collectionimage = _db.image.Where(i => i.name == image_name && i.collection_id == (int)_c.id).Select(t => t).FirstOrDefault();
                if (collectionimage == null)
                    throw new Exception("A-OK, Handled.");

                collection collection = _db.collection.Where(p => p.Id == (int)_c.id).Select(p => p).FirstOrDefault();
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

        public void AddProduct(dto_collection dto, long product_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                product pro = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
                if (pro == null)
                    throw new Exception("A-OK, Handled.");
                collection collection = _db.collection.Where(c => c.Id == (int)dto.id).FirstOrDefault();
                collection.product.Add(pro);
                _db.SaveChanges();
            }
        }

        public void RemoveProduct(dto_collection dto, long product_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                product pro = _db.product.Where(p => p.Id == product_id).FirstOrDefault();
                if (pro == null)
                    throw new Exception("A-OK, Handled.");

                collection collection = _db.collection.Where(c => c.Id == (int)dto.id).FirstOrDefault();
                collection.product.Remove(pro);
                _db.SaveChanges();
            }
        }

        public override dto_booth GetBoothDTO<T>(T dto)
        {
            biz_booth biz = new biz_booth();
            booth boo = biz.GetBooth(dto.booth_id, "", "", false, false, false, false, false, false, true);

            dto_booth _dto = biz.ToDTO(boo, null);
            return _dto;
        }

        public dto_collection ToDTO(collection c, List<Hit> rel, string booth)
        {
            if (c.IsNull())
                throw new Exception("A-OK, Check.");

            //if (rel == null)
            //    throw new Exception("A-OK, Check.");

            //if (booth == null)
            //    throw new Exception("A-OK, Check.");

            dto_collection dto = new dto_collection();

            dto.id = c.Id;
            dto.booth_id = c.booth_id;
            dto.category_main_id = dto.category_main_id != 0 ? dto.category_main_id : c.category_main_id;
            dto.category_second_id = dto.category_second_id != 0 ? dto.category_second_id : c.category_second_id;
            dto.active = c.active;

            biz_folder flda = new biz_folder();
            dto.foldera = c.foldera != null ? flda.GetFolderADTO(c.foldera.Id, false, false) : new dto_folder() { name = "default" };

            biz_folder fldb = new biz_folder();
            dto.folderb = c.folderb != null ? fldb.GetFolderBDTO(c.folderb.Id) : new dto_folder() { name = "default" };

            dto.name = !c.name.IsNullOrEmpty() ? c.name : dto.name;
            dto.sysname = !c.sysname.IsNullOrEmpty() ? c.sysname : dto.sysname;
            dto.created_on = !c.created_on.IsNull() ? c.created_on : dto.created_on;
            dto.modified = !c.modified.IsNull() ? c.modified : dto.modified;
            dto.price = !c.joinedprice.IsNullOrEmpty() ? c.joinedprice : dto.price;
            dto.status_stock = !c.status_stock.IsNullOrEmpty() ? c.status_stock : dto.status_stock;
            dto.status_condition = !c.status_condition.IsNullOrEmpty() ? c.status_condition : dto.status_condition;
            dto.description = !c.description.IsNullOrEmpty() ? c.description : "";
            dto.note = !c.note.IsNullOrEmpty() ? c.note : "";

            if (!c.booth.IsNull())
            {
                biz_booth biz = new biz_booth();
                dto.booth_dto = biz.ToDTO(NullHelper.ProNull(c.booth), null);
            }
            if (!c.image.IsNull())
            {
                biz_image pi = new biz_image();
                dto.image_dtos = new List<dto_image>();
                dto.image_dtos = pi.ToDTOList(c.image.ToList());
            }
            if (!c.tag.IsNull() && c.tag.Count() > 0)
            {
                foreach (tag t2 in c.tag)
                    NullHelper.ProNull(t2);
                biz_tag t = new biz_tag();
                dto.tag_dtos = new List<dto_tag>();
                dto.tag_dtos = t.ToDTOList(c.tag.ToList());
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
                biz_params pa = new biz_params();
                dto.param_dtos = new List<dto_params>();
                dto.param_dtos = pa.ToDTO_List(ps.ToList());
            }

            if (!c.conversation.IsNull() && c.conversation.Count() > 0/* && withconversations*/)
            {
                biz_conversation poco = new biz_conversation();
                dto.conversations = poco.ToDTOList(c.conversation);
            }
            if (!c.product.IsNull())
            {
                dto.product_dtos = new List<dto_product>();
                foreach (product p in c.product)
                {
                    biz_product biz = new biz_product(false);
                    dto_product _dto = biz.ToDTO(p, null, "");
                    dto.product_dtos.Add(_dto);
                }
            }
            dto.relevant = !rel.IsNull() ? rel.Where(x => x.booth == booth && x.product == dto.name).Count() > 0 : false;

            return dto;
        }

        public List<dto_collection> ToDTOList(ICollection<collection> collections, List<Hit> rel, string booth)
        {
            if (collections == null)
                throw new Exception("A-OK, Check.");

            List<dto_collection> res = new List<dto_collection>();
            foreach (collection c in collections.ToList())
            {
                biz_collection biz = new biz_collection();
                dto_collection dto = new dto_collection();
                dto = biz.ToDTO(c, rel, booth);
                dto = biz.SetupToClient<dto_collection>(dto);
                res.Add(dto);
            }
            return res;
        }

        public void FixCats(EbazarDB _db, collection col, dto_collection dto, bool create)
        {
            booth bth = _db.booth.Where(b => b.Id == dto.booth_id).FirstOrDefault();
            //if (boo.category_main.Where(cat => cat.Id == dto.category_main_id).Count() == 0)
            //{
            //    category new_cat = _db.category.Where(c => c.name != ".ingen" && c.is_parent && c.Id == dto.category_main_id).FirstOrDefault();
            //    if (new_cat.IsNull())
            //        throw new Exception("A-OK, Handled.");
            //    boo.category_main.Add(new_cat);
            //}

            int? old_cat_id = col.category_main_id != dto.category_main_id ? (int?)col.category_main_id : null;
            int? old_cat_sec_id = col.category_second_id != dto.category_second_id ? (int?)col.category_second_id : null;
            if (col.category_main_id == 0 || create)//create
            {
                category main = _db.category.Include("children").Where(c => c.name != ".ingen" && c.is_parent).FirstOrDefault();
                category sec = main.children.OrderBy(x => x.priority).FirstOrDefault();

                col.category_main_id = main.Id;
                col.category_second_id = sec.Id;
                bth.category_main.Add(main);

                biz_category.update = true;
            }
            else if (old_cat_id != null)//new cat, changing
            {
                ///JUST TEST///
                collection a = _db.collection.Where(z => z.Id == dto.id).FirstOrDefault();
                List<category> list_main = _db.category.Where(x => x.collection_main.Where(z => z.Id == a.Id).Count() > 0).ToList();
                List<category> list_sec = _db.category.Where(x => x.collection_second.Where(z => z.Id == a.Id).Count() > 0).ToList();
                ///JUST TEST///

                category old_cat = _db.category.Where(cat => cat.Id == old_cat_id).FirstOrDefault();
                
                if (old_cat.product_main.Where(cat => cat.booth_id == bth.Id).Count() + old_cat.collection_main.Where(cat => cat.booth_id == bth.Id).Count() == 0)
                    bth.category_main.Remove(old_cat);

                category main = _db.category.Where(c => c.Id == dto.category_main_id).FirstOrDefault();
                category sec = main.children.OrderBy(c => c.priority).ElementAt(main.children.Count() - 1);

                col.category_main_id = main.Id;
                col.category_second_id = sec.Id;
                bth.category_main.Add(main);

                biz_category.update = true;
            }
            else if (old_cat_sec_id != null)//cat 2 changed
            {
                col.category_main_id = dto.category_main_id;
                col.category_second_id = dto.category_second_id;

                biz_category.update = true;
            }
            else//no changes
            {
                col.category_main_id = dto.category_main_id;
                col.category_second_id = dto.category_second_id;
            }
        }

        public void ToCollection(bool new_collection, dto_collection dto, ref collection col, EbazarDB _db)
        {
            if (_db == null)
                throw new Exception("A-OK, Handled.");

            DateTime now = DateTime.Now;

            FixCats(_db, col, dto, false);

            col.foldera = !dto.foldera.IsNull() ? _db.folder.Where(l => l.Id == dto.foldera.id).FirstOrDefault() : col.foldera;
            col.folderb = !dto.folderb.IsNull() ? _db.folder.Where(l => l.Id == dto.folderb.id).FirstOrDefault() : col.folderb;
            col.active = dto.active;
            col.name = !dto.name.IsNullOrEmpty() ? dto.name : col.name;
            col.sysname = !dto.sysname.IsNullOrEmpty() ? dto.sysname : col.sysname;
            col.created_on = !new_collection && !dto.created_on.IsNull() ? dto.created_on : now;
            col.modified = now;
            col.joinedprice = !dto.price.IsNullOrEmpty() ? dto.price : col.joinedprice;
            col.status_stock = !dto.status_stock.IsNullOrEmpty() ? dto.status_stock : col.status_stock;
            col.status_condition = !dto.status_condition.IsNullOrEmpty() ? dto.status_condition : col.status_condition;
            col.description = !dto.description.IsNullOrEmpty() ? dto.description : "";
            col.note = !dto.note.IsNullOrEmpty() ? dto.note : "";

            if (dto.booth_dto != null)
            {
                col.booth = _db.booth.Where(b => b.Id == dto.booth_dto.booth_id).FirstOrDefault();
                if (new_collection)
                    col.booth.modified = now;
            }
            else
                throw new Exception("A-OK, Handled.");
        }
    }
}