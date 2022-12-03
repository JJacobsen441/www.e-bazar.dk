using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DataAccess
{
    /*
     * DataAccessLayer
     * */

    public class DAL
    {
        private DAL()
        {
            
        }

        private static DAL d = null;
        public static DAL GetInstance() 
        {
            if (d == null)
                d = new DAL();
            return d;
        }

        public long GetTagIdByName_FORTEST(string name)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                return _db.tag.Where(t => t.name == name).FirstOrDefault().Id;
            }
        }

        public int? GetLevelPriority_FORTEST(string type, string name)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                int? priority = -1;
            if (type == "lev_a")
            {
                List<folder> list = _db.folder.ToList();
                folder a = list.Where(t => t.name == name && t.is_parent == true).SingleOrDefault();
                priority = a.priority;
            }
            else
            {
                folder b = _db.folder.Where(t => t.name == name && t.is_parent == false).SingleOrDefault();
                priority = b.priority;
            }
            return priority;
            }
        }

        public int GetLevelId_FORTEST(string type, string name)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                if (type == "lev_a")
                return (int)_db.folder.Where(t => t.name == name && t.is_parent == true).FirstOrDefault().Id;
            else
                return (int)_db.folder.Where(t => t.name == name && t.is_parent == false).FirstOrDefault().Id;
            }
        }

        public int GetBoothsCount()
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                return _db.booth.Count();
            } 
        }

        public int GetItemsCount()
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                return _db.product.Count() + _db.collection.Count();
            }
        }        

        public dto_category GetChildInCategory(int cat_id, int nr)
        {
            List<dto_category> cat = CategorysHelper.CatsYesYes;
            
            if (cat == null)
                throw new Exception("A-OK, Handled.");

            dto_category cat_dto = cat.Where(x=>x.category_id == cat_id).FirstOrDefault();
            int elem = nr == -1 ? cat_dto.children.Count() - 1 : nr;
            
            return cat_dto.children.ElementAt(elem);
        }

        public List<dto_booth> GetNewestBoothDTOs(int skip, int take)
        {
            biz_booth booth_poco = new biz_booth();
            List<dto_booth> booth_dtos;
            
            booth_dtos = booth_poco.GetNewestBoothDTOs(skip, take);//categorys eller begge
            
            return booth_dtos;
        }

        public List<dto_booth> GetBoothDTOs(int skip, int take, out int count)
        {
            biz_booth booth_poco = new biz_booth();
            List<dto_booth> booth_pocos;
            
            booth_pocos = booth_poco.GetBoothDTOs(skip, take, out count);//categorys eller begge
            
            return booth_pocos;
        }

        public List<dto_booth> GetBoothDTOs(string salesman_id, bool withsalesman)
        {
            biz_booth biz = new biz_booth();
            List<dto_booth> booths = biz.GetBoothDTOs(salesman_id, withsalesman);

            return booths;
        }

        public dto_booth GetBoothDTO(int? booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withproducts, bool withcollections, bool overrideonlycollection, bool withlevela, bool withconversations, bool withdefault)
        {
            biz_booth booth_biz = new biz_booth();
            dto_booth booth_dto = booth_biz.GetBoothDTO(booth_id, lev_a_search, lev_b_search, select_inactive, withproducts, withcollections, overrideonlycollection, withlevela, withconversations, withdefault);

            return booth_dto;
        }

        public List<dto_booth> GetBoothsByPersonDTOs(string salesman_id)
        {
            biz_booth booth_biz = new biz_booth();
            return booth_biz.GetBoothsByPersonDTO(salesman_id);
        }        

        public dto_product GetProductDTO(long product_id, bool withbooth, bool withproducts, bool withcollection, bool withconversation, bool withtags)
        {
            biz_product pr_biz = new biz_product(withcollection);
            dto_product product = pr_biz.GetProductDTO(product_id, withbooth, withproducts, withconversation, withtags);

            return product;
        }

        public List<dto_product> GetProductDTOs(int booth_id, bool select_inactive, bool withbooth, bool withcollection, bool overrideonlycollection, bool withtags, bool withfoldera, bool withdefault)
        {
            biz_product product_poco = new biz_product(withcollection);
            List<dto_product> list = product_poco.GetProductDTOs(booth_id, "", "", select_inactive, withbooth, overrideonlycollection, withtags, withfoldera, withdefault);
            return list;
        }

        public dto_collection GetCollectionDTO(int collection_id, bool withproducts, bool withbooth, bool withconversation, bool withboothsalesman, bool withtags)
        {
            biz_collection col_poco = new biz_collection();
            dto_collection collection = col_poco.GetCollectionDTO(collection_id, withproducts, withbooth, withconversation, withboothsalesman, withtags);

            return collection;
        }

        public List<dto_collection> GetCollectionDTOs(int booth_id, bool select_inactive, bool withbooth, bool withtags, bool withfoldera)
        {
            biz_collection col_poco = new biz_collection();
            List<dto_collection> pocos = col_poco.GetCollectionDTOs(booth_id, "", "", select_inactive, withbooth, withtags, withfoldera);

            return pocos;
        }

        public List<dto_params> GetParams(int cat_main, int cat_sec)
        {
            biz_params pa = new biz_params();
            //EbazarDB _db = GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                List<param> list = (from p in _db.param
                                    //.Include(x=>x.ValueNavigation)
                                    //.Include(x=>x.Category)
                                    //.Where(pa => (pa.CategoryId == cat_main && pa.Category.IsParent) || (pa.CategoryId == cat_sec && !pa.Category.IsParent)).ToList();
                                join v in _db.value on p.Id equals v.param_id into va
                                from v in va.DefaultIfEmpty()
                                where (p.category_id == cat_main && p.category.is_parent) || (p.category_id == cat_sec && !p.category.is_parent)
                                select new
                                {
                                    Id = p.Id,
                                    name = p.name,
                                    prio = p.prio,
                                    category = p.category,
                                    cat_id = p.category_id,
                                    type = p.type,
                                    val = p.value,
                                    valn = p.value1
                                })
                               .AsEnumerable()
                               .Select(x => new param()
                               {
                                   Id = x.Id,
                                   name = x.name,
                                   prio = x.prio,
                                   category = x.category,
                                   category_id = x.cat_id,
                                   type = x.type,
                                   value = x.val,
                                   value1 = x.valn
                               })
                               .GroupBy(x => x.Id)
                               .Select(grp => grp.First())
                               .ToList();
                if (list == null)
                    throw new Exception("A-OK, Check.");

                return pa.ToDTO_List(list);
            }
        }

        public List<dto_booth_item> GetItemDTOs(int booth_id)
        {
            List<dto_product> pro = this.GetProductDTOs(booth_id, false, true, false, false, false, false, true);
            if (pro == null)
                return null;
            List<dto_collection> col = this.GetCollectionDTOs(booth_id, false, true, false, false);
            if (col == null)
                return null;
            List<dto_booth_item> items = new List<dto_booth_item>();
            items = items.Concat(pro).ToList();
            items = items.Concat(col).ToList();
            items = items.OrderByDescending(i => i.created_on).ToList();
            items = items.OrderByDescending(i => i.relevant).ToList();
            return items;
        }

        public dto_person GetPersonDTO<T>(string person_id, bool withbooth, bool withfavorites, bool withfollowing) where T : dto_person, new()
        {
            biz_person biz = null;
            if (typeof(dto_salesman) == typeof(T))
                biz = new biz_salesman();
            else 
                biz = new biz_customer();

            T dto = null;
            
            string _d = "Salesman";
            if (typeof(T) == typeof(dto_customer))
                _d = "Customer";
            
            dto = biz.GetPersonDTO<T>(person_id, withbooth, withfavorites, withfollowing);

            //if (dto == null || dto.nator != _d)
            //    throw new Exception("A-OK, Handled.");

            return dto;
        }

        public int SaveBooth(dto_booth booth_dto)
        {
            //EbazarDB _db = GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                biz_booth biz = new biz_booth();
                booth booth = biz.ToBooth(true, booth_dto, _db);
                _db.booth.Add(booth);
                _db.SaveChanges();
                _db.Dispose();
                return booth.Id;
            }
        }

        public void UpdateBooth(dto_booth booth)
        {
            //EbazarDB db = GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                biz_booth biz = new biz_booth();
                biz.ToBooth(false, booth, _db);
                _db.SaveChanges();
                _db.Dispose();
            }
        }

        public void DeleteBooth(int id)
        {
            //EbazarDB db = GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                biz_booth poco = new biz_booth();
                poco.DeleteBooth(id, _db);
                _db.SaveChanges();
                _db.Dispose();
            }
        }

        public long SaveProduct(dto_product product_dto, List<string> uploaded_fnames)
        {
            //product_poco.db = this.db;
            biz_product biz = new biz_product();
            long product_id = biz.Save<dto_product>(product_dto);
            biz_image productimage_dto = new biz_image();
            if (uploaded_fnames != null && uploaded_fnames.Count > 0)
                productimage_dto.SaveImages(true, product_id, uploaded_fnames);
            return product_id;
        }

        public void UpdateProduct(dto_product product_dto, List<string> uploaded_fnames)
        {
            //product_poco.db = this.db;
            biz_product biz = new biz_product();
            biz.Update(product_dto);
            biz_image productimage_dto = new biz_image();
            if (uploaded_fnames != null && uploaded_fnames.Count > 0)
                productimage_dto.SaveImages(true, product_dto.id, uploaded_fnames);
        }

        public void DeleteProduct(long id)
        {
            //productimage_dto productimage_dto = new productimage_dto();
            //productimage_dto.DeleteProductImages(id);
            //EbazarDB db = GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                biz_product poco = new biz_product(false);
                poco.Delete(id, _db);
                _db.SaveChanges();
                _db.Dispose();
            }
        }

        public int SaveCollection(dto_collection collection_dto, List<string> uploaded_fnames)
        {
            //collection_poco.db = this.db;
            biz_collection biz = new biz_collection();
            int collection_id = (int)biz.Save(collection_dto);
            biz_image collectionimage_dto = new biz_image();
            if (uploaded_fnames != null && uploaded_fnames.Count > 0)
                collectionimage_dto.SaveImages(false, collection_id, uploaded_fnames);
            return collection_id;
        }

        public void UpdateCollection(dto_collection collection_dto, List<string> uploaded_fnames)
        {
            //collection_poco.db = this.db;
            biz_collection biz = new biz_collection();
            biz.Update<dto_collection>(collection_dto);
            biz_image collectionimage_poco = new biz_image();
            if (uploaded_fnames != null && uploaded_fnames.Count > 0)
                collectionimage_poco.SaveImages(false, (int)collection_dto.id, uploaded_fnames);
        }

        public void DeleteCollection(long id)
        {
            //productimage_dto productimage_dto = new productimage_dto();
            //productimage_dto.DeleteProductImages(id);
            //EbazarDB db = GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                biz_collection poco = new biz_collection();
                poco.Delete(id, _db);
                _db.SaveChanges();
                _db.Dispose();
            }
        }

        public void AddProductToCollection(int collection_id, long product_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            //using (EbazarDB _db = new EbazarDB())
            {

                dto_collection dto = this.GetCollectionDTO(collection_id, false, false, false, false, false);

                biz_collection biz= new biz_collection();
                biz.AddProduct(dto, product_id);
                //_db.SaveChanges();
            }
        }

        public void RemoveProductFromCollection(int collection_id, long product_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            //using (EbazarDB _db = new EbazarDB())
            {

                dto_collection dto = this.GetCollectionDTO(collection_id, false, false, false, false, false);

                biz_collection biz = new biz_collection();
                biz.RemoveProduct(dto, product_id);

                //_db.SaveChanges();
            }
        }

        public List<dto_tag> Get5TagDTOs(string contains)
        {
            biz_tag poco = new biz_tag();
            List<dto_tag> tag_pocos = poco.Get5TagDTOs(contains);

            return tag_pocos;
        }

        public tag GetTag(string name)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                tag tag = _db.tag.Where(t => t.name == name).FirstOrDefault();
                if (tag != null)
                    return tag;
                else
                    return null;
            }
        }
        
        public string AddCategory(int cat_id, int booth_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                category cat = _db.category.Where(c => c.Id == cat_id).FirstOrDefault();
                booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
                booth.category_main.Add(cat);
                _db.SaveChanges();
                return cat.name;
            }
        }

        public bool RemoveCategory(int cat_id, int booth_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                category cat = _db.category.Where(c => c.Id == cat_id).FirstOrDefault();
                booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
                booth.category_main.Remove(cat);
                _db.SaveChanges();
                return true;
            }
        }

        public MESSAGE_TAG SaveTag(string tag_name, string id, TYPE type)
        {
            biz_tag dto = new biz_tag();
            return dto.SaveTag(tag_name, id, type);
        }

        public bool RemoveTag(long tag_id, TYPE type, string id, bool is_updating)
        {
            biz_product biz_product = new biz_product(false);
            biz_collection biz_collection = new biz_collection();

            if (type == TYPE.PRODUCT)
            {
                dto_product dto = biz_product.GetProductDTO(long.Parse(id), false, false, false, false);
                
                return biz_product.RemoveTag<dto_product>(dto, tag_id, is_updating);
            }
            else
            {
                dto_collection dto = biz_collection.GetCollectionDTO(int.Parse(id), false, false, false, false, false);
                
                return biz_collection.RemoveTag<dto_collection>(dto, tag_id, is_updating);
            }
        }



        public bool SaveParam(long id, int param_id, int val_id, TYPE type)
        {
            biz_params dto = new biz_params();
            return dto.SaveParam(id, param_id, val_id, type);
        }

        public bool RemoveParam(int param_id, TYPE type, long id)
        {
            biz_product biz_product = new biz_product();
            biz_collection biz_collection = new biz_collection();

            if (type == TYPE.PRODUCT)
            {
                dto_product dto = biz_product.GetProductDTO(id, false, false, false, false);
                
                return biz_product.RemoveParam<dto_product>(dto, param_id);
            }
            else
            {
                dto_collection dto = biz_collection.GetCollectionDTO(id, false, false, false, false, false);
                
                return biz_collection.RemoveParam<dto_collection>(dto, param_id);
            }
        }



        public void SavePerson<T>(T dto) where T : dto_person, new()
        {
            biz_person biz = new biz_customer();
            if (dto.GetType() == typeof(dto_salesman))
                biz = new biz_salesman();
            biz.SavePerson<T>(dto);
        }

        public void UpdatePerson<T>(T dto) where T: dto_person, new()
        {
            biz_person biz = new biz_customer();
            if (dto.GetType() == typeof(dto_salesman))
                biz = new biz_salesman();
            biz.UpdatePerson<T>(dto);
        }

        public dto_conversation GetConversation(long id, string person_id, TYPE type)
        {
            biz_conversation biz = new biz_conversation();
            if(type == TYPE.PRODUCT)
                return biz.GetConversationDTO(id, -1, -1, person_id, type);
            else if (type == TYPE.COLLECTION)
                return biz.GetConversationDTO(-1, (int)id, -1, person_id, type);
            else 
                return biz.GetConversationDTO(-1, -1, (int)id, person_id, type);
        }

        public string SaveMessage(long? conversation_id, long product_id, string person_id, string message, TYPE type)
        {
            biz_conversation conversation_poco = new biz_conversation();
            return conversation_poco.SaveMessage(conversation_id, product_id, person_id, message, type);
        }

        public void AddFavorite(long product_id, int collection_id)
        {
            dto_person current_user = CurrentUser.Inst().GetDTO(SETUP.FFF);
            biz_person biz = new biz_customer();
            if (current_user.nator == "Salesman")
                biz = new biz_salesman();
            if(current_user != null)
                biz.AddFavorite(current_user.person_id, product_id, collection_id);
        }

        public void RemoveFavorite(long product_id, int collection_id)
        {
            dto_person current_user = CurrentUser.Inst().GetDTO(SETUP.FFF);
            biz_person biz = new biz_customer();
            if (current_user.nator == "Salesman")
                biz = new biz_salesman();
            if (current_user != null)
                biz.RemoveFavorite(current_user.person_id, product_id, collection_id);
        }

        public void AddFollowing(int booth_id)
        {
            dto_person current_user = CurrentUser.Inst().GetDTO(SETUP.FFF);
            biz_person biz = new biz_customer();
            if (current_user.nator == "Salesman")
                biz = new biz_salesman();
            if (current_user != null)
                biz.AddFollowing(current_user.person_id, booth_id);
        }

        public void RemoveFollowing(int booth_id)
        {
            dto_person current_user = CurrentUser.Inst().GetDTO(SETUP.FFF);
            biz_person biz = new biz_customer();
            if (current_user.nator == "Salesman")
                biz = new biz_salesman();
            if (current_user != null)
                biz.RemoveFollowing(current_user.person_id, booth_id);            
        }

        public void DeleteConversation(long con_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                conversation con = _db.conversation.Where(c => c.Id == con_id).FirstOrDefault();
                if (con != null)
                {
                    foreach (comment com in con.comment.ToList())
                        con.comment.Remove(com);
                    _db.conversation.Remove(con);
                    _db.SaveChanges();
                }
            }
        }

        public List<dto_folder> GetFolderTree(int booth_id, bool withbooth)
        {
            biz_folder fa = new biz_folder();
            List<dto_folder> fld_a_dtos = new List<dto_folder>();
            fld_a_dtos = fa.GetFolderADTOs(booth_id, withbooth);
            
            return fld_a_dtos.OrderBy(l => l.priority).ToList();
        }

        public void CreateFolder(string fld_name, int id, TYPE type)
        {
            biz_folder a = new biz_folder();
            biz_folder b = new biz_folder();
            if (type == TYPE.FOLDER_A)
                a.CreateFolderForA(fld_name, id);
            else
                b.CreateFolderForB(fld_name, id);
        }

        public void MoveFolder(int fld_id, string direction, int id, TYPE type)
        {
            biz_folder a = new biz_folder();
            biz_folder b = new biz_folder();
            if (type == TYPE.FOLDER_A)
                a.MoveFolderForA(fld_id, direction, id);
            else
                b.MoveFolderForB(fld_id, direction, id);
        }

        public void DeleteFolder(int fld_id, int id, TYPE type)
        {
            biz_folder a = new biz_folder();
            biz_folder b = new biz_folder();
            if (type == TYPE.FOLDER_A)
                a.DeleteFolderForA(fld_id, id);
            else
                b.DeleteFolderForB(fld_id, id);
        }

        public void SetFolder(int fld_id, string id, TYPE type, bool is_product)
        {
            biz_product p = new biz_product(false);
            biz_collection c = new biz_collection();
            if (is_product)
                p.SetFolder(fld_id, long.Parse(id), type);
            else
                c.SetFolder(fld_id, int.Parse(id), type);
        }

        public bool AddRating(int booth_id, string person_id, short rating)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
                if (booth != null)
                {
                    person person = _db.person.Where(p => p.Id == person_id).FirstOrDefault();
                    if (person != null)
                    {
                        boothrating br = _db.boothrating.Where(brat => brat.booth_id == booth_id && brat.person.Id == person_id).FirstOrDefault();
                        if (br == null)
                            booth.boothrating.Add(new boothrating() { booth = booth, person = person, rating = rating });
                        else
                            br.rating = rating;
                        _db.SaveChanges();
                        return true;
                    }
                }
                return false;
            }
        }

        public List<dto_conversation> GetConversationsPerson(string person_id, bool is_salesman, bool withboothsalesman)
        {
            biz_conversation c = new biz_conversation();
            return c.GetConversationsPersonDTO(person_id, is_salesman, withboothsalesman);

        }

        public string SetActive(long id, bool value, string type)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                bool res = false;
                if (type == "product")
                {
                    product pro = _db.product
                        .Include("category_main")
                        .Include("category_second")
                        .Where(p => p.Id == id).FirstOrDefault();
                    if (pro != null)
                    {
                        if (pro.category_main.name == ".ingen" || pro.category_second.name == "..ingen")
                            return "err";
                        pro.active = value;
                        res = value;
                    }
                    else
                        throw new Exception("A-OK, handled.");
                }
                else if (type == "collection")
                {
                    collection col = _db.collection
                        .Include("category_main")
                        .Include("category_second")
                        .Where(c => c.Id == id).FirstOrDefault();
                    if (col != null)
                    {
                        if (col.category_main.name == ".ingen" || col.category_second.name == "..ingen")
                            return "err";
                        col.active = value;
                        res = value;
                    }
                    else
                        throw new Exception("A-OK, handled.");
                }
                _db.SaveChanges();
                return res == true ? "true" : "false";
            }
        }

        public bool ChangeBoothId(int BoothId, long ProductId)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                dto_person currentuser = CurrentUser.Inst().GetDTO(SETUP.FFF);
                if (currentuser == null)
                    throw new Exception("A-OK, handled.");

                dto_product product_poco = new dto_product();
                dto_booth booth_poco = new dto_booth();
                biz_product biz_product = new biz_product();
                biz_booth biz_booth = new biz_booth();
                product_poco = biz_product.GetProductDTO(ProductId, true, false, false, false);
                    
                booth_poco = biz_booth.GetBoothDTO(BoothId, "", "", true, true, false, false, false, false, true);
                    
                if (product_poco.collection_id == null)
                {
                    product pro = _db.product.Where(p => p.Id == ProductId).FirstOrDefault();
                    booth new_booth = _db.booth.Where(b => b.Id == BoothId).FirstOrDefault();
                    booth old_booth = _db.booth.Where(b => b.Id == pro.booth_id).FirstOrDefault();

                    if (pro == null)
                        throw new Exception("dal.ChangeShopId > pro NULL");
                    if (new_booth == null)
                        throw new Exception("dal.ChangeShopId > new_shop NULL");
                    if (old_booth == null)
                        throw new Exception("dal.ChangeShopId > old_shop NULL");

                    Dictionary<string, string> dirs_product = SetupHelper.SetupProductDirs(product_poco, currentuser.sysname);
                    Dictionary<string, string> dirs_booth = SetupHelper.SetupBoothDirs(ref booth_poco, currentuser.sysname);
                    string nd = Path.DirectorySeparatorChar.ToString();
                    string old_path = PathHelper.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs_product, true);// + product_poco.sysname + nd;
                    string new_path = PathHelper.GetPath(PATH.BOOTH_DIRECTORY, dirs_booth, true) + booth_poco.sysname + nd + "products" + nd + product_poco.sysname + nd;
                    foreach (dto_image im in product_poco.image_dtos)
                    {
                        try { PathHelper.MoveFile(old_path, im.name, new_path, im.name, true, false, false, false); } catch (Exception _e) { ; }
                        try { PathHelper.MoveFile(old_path, "t_" + im.name, new_path, "t_" + im.name, true, false, false, false); } catch (Exception _e) { ; }
                    }
                    PathHelper.ClearFolder(old_path, true, true);
                    //pro.booth_id = BoothId;



                    try
                    {
                        category top = _db.category.Where(x=>x.is_parent).OrderBy(x=>x.priority).FirstOrDefault();
                        category sec = top.children.OrderBy(c => c.priority).ElementAt(top.children.Count() - 1);

                        pro.folder_a_id = null;
                        pro.folder_b_id = null;

                        if (pro.category_main != null)
                            pro.category_main = top;
                        if (pro.category_second != null)
                            pro.category_second = sec;
                        pro.category_main_id = top.Id;
                        pro.category_second_id = sec.Id;


                    

                        new_booth.product.Add(pro);
                        if (!new_booth.category_main.Contains(top))
                            new_booth.category_main.Add(top);
                        old_booth.product.Remove(pro);

                        _db.SaveChanges();

                        //category old_cat = db.category.Where(c => c.Id == pro.category_main_id).FirstOrDefault();
                        //category old_cat_sec = db.category.Where(c => c.Id == pro.category_second_id).FirstOrDefault();
                        //old_cat_sec.product_second.Remove(pro);
                        //old_cat.product_main.Remove(pro);
                        //if (old_cat.product_main.Where(c => c.booth_id == old_booth.Id).Count() + old_cat.collection_main.Where(c => c.booth_id == old_booth.Id).Count() == 0)
                        //    old_booth.category_main.Remove(old_cat);

                        //db.SaveChanges();
                        //dbContextTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        ;//Log, handle or absorbe I don't care ^_^
                    }
                    return true;
                }
                return false;
            }
        }

        public string GetAddressTown(string zip)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                int zipno;
                bool ok = int.TryParse(zip, out zipno) && zipno >= 1001 && zipno <= 9999;
                if (ok)
                {
                    region region = _db.region.Where(r => r.zip == zipno).FirstOrDefault();
                    if (region != null)
                    {
                        if (zipno >= 1001 && zipno <= 1499)
                            return "københavn k";
                        else if (zipno >= 1500 && zipno <= 1799)
                            return "københavn v";
                        else if (zipno >= 1800 && zipno <= 2000)
                            return "frederiksberg";
                        else
                            return region.town;
                    }
                }
                return "(ugyldig!)";
            }
        }
    }
}