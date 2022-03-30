using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DataAccess
{
    /*
     * DataAccessLayer
     * */
    public class DAL
    {
        private List<EbazarDB> _dbs = null;

        public EbazarDB GetContext()
        {
            
            if (_dbs == null)
                _dbs = new List<EbazarDB>();
            EbazarDB _db = new EbazarDB();

            
            /*try
            {
                EbazarDB db = new EbazarDB();
                foreach (category cat in db.category)
                    _db.Entry(cat).Reload();
            }
            catch (Exception e)
            {
                ;
            }/**/

            _dbs.Add(_db);
            return _db;
        }

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

        public void Dispose()
        {
            if (_dbs == null)
                return;

            foreach (EbazarDB d in _dbs)
            {
                try
                {
                    d?.Dispose();
                }
                catch (Exception e) 
                {
                    ErrorHandler err = new ErrorHandler();
                    string subject = "Fejl i DataAccessLayer -> Dispose!";
                    string body = err.FormatError(e);
                    Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                }
            }
            _dbs = null;
        }

        /*public List<poco_booth> GetBoothPOCOs()
        {
            poco_booth booth_dto = new poco_booth() { db = this.db };
            List<poco_booth> booth_dtos = booth_dto.GetBoothPOCOs();

            return booth_dtos;
        }*/

        public long GetTagIdByName_FORTEST(string name)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            return _db.tag.Where(t => t.name == name).FirstOrDefault().Id;
        }

        public int? GetLevelPriority_FORTEST(string type, string name)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
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

        public int GetLevelId_FORTEST(string type, string name)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            if (type == "lev_a")
                return (int)_db.folder.Where(t => t.name == name && t.is_parent == true).FirstOrDefault().Id;
            else
                return (int)_db.folder.Where(t => t.name == name && t.is_parent == false).FirstOrDefault().Id;
        }

        public int GetBoothsCount()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            return _db.booth.Count();
        }

        public int GetItemsCount()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            return _db.product.Count() + _db.collection.Count();
        }

        public poco_story GetRandomStoriePOCO()
        {
            poco_story story_poco = new poco_story();
            List<poco_story> story_pocos;
            
            story_pocos = story_poco.GetStoryPOCOs();

            Random random = new Random();
            int rand = random.Next(0, story_pocos.Count() - 1);

            return story_pocos.ElementAt(rand);
        }

        public poco_category GetChildInCategory(int cat_id, int nr)
        {
            /*category cat = db.category
                //.Include("parent")
                //.Include("children")//.ThenInclude(x => x.Param).ThenInclude(x => x.ValueNavigation)
                //.Include("product_main")
                //.Include("product_second")
                //.Include("booth_category")
                //.Include(x => x.Param).ThenInclude(x => x.ValueNavigation)
                .Where(c => c.Id == cat_id).FirstOrDefault();

            //cat.param = IncludeParam(cat.Id);

            //foreach (category c in cat.children)
            //{
            //    c.param = IncludeParam(c.Id);
            //}*/
            List<poco_category> cat = Categorys.CatsYesYes;
            
            if (cat == null)
                throw new Exception("A-OK, Handled.");

            poco_category cat_poco = cat.Where(x=>x.category_id == cat_id).FirstOrDefault();
            //cat_poco.ToPOCO(cat/*, cat_id*/, true, false);
            int elem = nr == -1 ? cat_poco.children.Count() - 1 : nr;
            return cat_poco.children.ElementAt(elem);
        }

        public List<poco_booth> GetNewestBoothPOCOs(int skip, int take)
        {
            poco_booth booth_poco = new poco_booth();
            List<poco_booth> booth_pocos;
            //if (string.IsNullOrEmpty(search) && string.IsNullOrEmpty(categorys))
            //    booth_dtos = booth_dto.GetBoothPOCOs();//ingen
            //else
            //{
            booth_pocos = booth_poco.GetNewestBoothPOCOs(skip, take);//categorys eller begge
            //}

            return booth_pocos;
        }

        public List<poco_booth> GetBoothPOCOs(int skip, int take, out int count)
        {
            poco_booth booth_poco = new poco_booth();
            List<poco_booth> booth_pocos;
            //if (string.IsNullOrEmpty(search) && string.IsNullOrEmpty(categorys))
            //    booth_dtos = booth_dto.GetBoothPOCOs();//ingen
            //else
            //{
            booth_pocos = booth_poco.GetBoothPOCOs(skip, take, out count);//categorys eller begge
            //}

            return booth_pocos;
        }

        public List<poco_booth> GetBoothPOCOs(string salesman_id, bool withsalesman)
        {
            poco_booth booth_poco = new poco_booth();
            List<poco_booth> booths = booth_poco.GetBoothPOCOs(salesman_id, withsalesman);

            return booths;
        }

        public poco_booth GetBoothPOCO(int? booth_id, string lev_a_search, string lev_b_search, bool select_inactive, bool withproducts, bool withcollections, bool overrideonlycollection, bool withlevela, bool withconversations, bool withdefault)
        {
            poco_booth booth_poco = new poco_booth();
            booth_poco = booth_poco.GetBoothPOCO(booth_id, lev_a_search, lev_b_search, select_inactive, withproducts, withcollections, overrideonlycollection, withlevela, withconversations, withdefault);

            return booth_poco;
        }

        public List<poco_booth> GetBoothsByPersonPOCOs(string salesman_id)
        {
            poco_booth booth_poco = new poco_booth();
            return booth_poco.GetBoothsByPersonPOCO(salesman_id);
        }

        //public poco_booth GetBoothPOCOByProductId(long product_id)
        //{
        //    poco_booth booth_poco = new poco_booth();
        //    poco_booth booth = booth_poco.GetBoothPOCOByProductId(product_id);

        //    return booth;
        //}

        //public poco_booth _GetBoothPOCOByCollectionId(long collection_id, bool withperson)
        //{
        //    poco_booth booth_poco = new poco_booth();
        //    poco_booth booth = booth_poco.GetBoothPOCOByCollectionId(collection_id, withperson);

        //    return booth;
        //}

        public poco_product GetProductPOCO(long product_id, bool withbooth, bool withproducts, bool withcollection, bool withconversation, bool withtags)
        {
            poco_product pr_poco = new poco_product(withcollection);
            poco_product product = pr_poco.GetProductPOCO(product_id, withbooth, withproducts, withconversation, withtags);

            return product;
        }

        public List<poco_product> GetProductPOCOs(int booth_id, bool select_inactive, bool withbooth, bool withcollection, bool overrideonlycollection, bool withtags, bool withfoldera, bool withdefault)
        {
            poco_product product_poco = new poco_product(withcollection);
            List<poco_product> list = product_poco.GetProductPOCOs(booth_id, "", "", select_inactive, withbooth, overrideonlycollection, withtags, withfoldera, withdefault);
            return list;
        }

        public poco_collection GetCollectionPOCO(int collection_id, bool withproducts, bool withbooth, bool withconversation, bool withboothsalesman, bool withtags)
        {
            poco_collection col_poco = new poco_collection();
            poco_collection collection = col_poco.GetCollectionPOCO(collection_id, withproducts, withbooth, withconversation, withboothsalesman, withtags);

            return collection;
        }

        public List<poco_collection> GetCollectionPOCOs(int booth_id, bool select_inactive, bool withbooth, bool withtags, bool withfoldera)
        {
            poco_collection col_poco = new poco_collection();
            List<poco_collection> pocos = col_poco.GetCollectionPOCOs(booth_id, "", "", select_inactive, withbooth, withtags, withfoldera);

            return pocos;
        }

        public List<poco_params> GetParams(/*long product_id, */int cat_main, int cat_sec)
        {
            poco_params pa = new poco_params();
            EbazarDB _db = GetContext();
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

            return pa.ToPOCO_List(list);
        }

        public List<IBoothItem> GetItemPOCOs(int booth_id)
        {
            List<poco_product> pro = this.GetProductPOCOs(booth_id, false, true, false, false, false, false, true);
            if (pro == null)
                return null;
            List<poco_collection> col = this.GetCollectionPOCOs(booth_id, false, true, false, false);
            if (col == null)
                return null;
            List<IBoothItem> items = new List<IBoothItem>();
            items = items.Concat(pro).ToList();
            items = items.Concat(col).ToList();
            items = items.OrderByDescending(i => i.created_on).ToList();
            items = items.OrderByDescending(i => i.relevant).ToList();
            return items;
        }

        public poco_person GetPersonPOCO<T>(string person_id, bool withbooth, bool withfavorites, bool withfollowing) where T : poco_person, new()
        {
            poco_person poco;
            string d = "";
            if (typeof(T) == typeof(poco_salesman))
            {
                d = "Salesman";
                poco = new poco_salesman(); 
            }
            else
            {
                d = "Customer";
                poco = new poco_customer();
            }
            poco = poco.GetPersonPOCO<T>(person_id, d, withbooth, withfavorites, withfollowing);

            return poco;
        }

        /*public List<T> _GetPersonsPOCO<T>(bool withfavorites, bool withfollowing) where T : poco_person, new()
        {
            poco_person poco = new T();
            List<T> pocos = new List<T>();
            pocos = poco._GetPersonsPOCO<T>(withfavorites, withfollowing);

            return pocos;
        }*/

        /*public customer_dto GetCustomerDTO(string customer_id)
        {
            customer_dto dto = new customer_dto();
            dto = dto.GetCustomerDTO(customer_id);
            return dto;
        }*/

        public int SaveBooth(poco_booth booth_poco)
        {
            EbazarDB db = GetContext();
            booth booth = booth_poco.ToBooth(true, db);
            db.booth.Add(booth);
            db.SaveChanges();
            db.Dispose();
            return booth.Id;
        }

        public void UpdateBooth(poco_booth booth)
        {
            EbazarDB db = GetContext();
            booth.ToBooth(false, db);
            db.SaveChanges();
            db.Dispose();
        }

        public void DeleteBooth(int id)
        {
            EbazarDB db = GetContext();
            poco_booth poco = new poco_booth();
            poco.DeleteBooth(id, db);
            db.SaveChanges();
            db.Dispose();
        }

        public long SaveProduct(poco_product product_poco, List<string> uploaded_fnames)
        {
            //product_poco.db = this.db;
            long product_id = product_poco.Save();
            poco_productimage productimage_dto = new poco_productimage();
            if (uploaded_fnames != null && uploaded_fnames.Count > 0)
                productimage_dto.SaveProductImages(product_id, uploaded_fnames);
            return product_id;
        }

        public void UpdateProduct(poco_product product_poco, List<string> uploaded_fnames)
        {
            //product_poco.db = this.db;
            product_poco.Update();
            poco_productimage productimage_dto = new poco_productimage();
            if (uploaded_fnames != null && uploaded_fnames.Count > 0)
                productimage_dto.SaveProductImages(product_poco.id, uploaded_fnames);
        }

        public void DeleteProduct(long id)
        {
            //productimage_dto productimage_dto = new productimage_dto();
            //productimage_dto.DeleteProductImages(id);
            EbazarDB db = GetContext();
            poco_product poco = new poco_product(false);
            poco.Delete(id, db);
            db.SaveChanges();
            db.Dispose();
        }

        public int SaveCollection(poco_collection collection_poco, List<string> uploaded_fnames)
        {
            //collection_poco.db = this.db;
            int collection_id = (int)collection_poco.Save();
            poco_collectionimage collectionimage_dto = new poco_collectionimage();
            if (uploaded_fnames != null && uploaded_fnames.Count > 0)
                collectionimage_dto.SaveCollectionImages(collection_id, uploaded_fnames);
            return collection_id;
        }

        public void UpdateCollection(poco_collection collection_poco, List<string> uploaded_fnames)
        {
            //collection_poco.db = this.db;
            collection_poco.Update();
            poco_collectionimage collectionimage_poco = new poco_collectionimage();
            if (uploaded_fnames != null && uploaded_fnames.Count > 0)
                collectionimage_poco.SaveCollectionImages((int)collection_poco.id, uploaded_fnames);
        }

        public void DeleteCollection(long id)
        {
            //productimage_dto productimage_dto = new productimage_dto();
            //productimage_dto.DeleteProductImages(id);
            EbazarDB db = GetContext();
            poco_collection poco = new poco_collection();
            poco.Delete(id, db);
            db.SaveChanges();
            db.Dispose();
        }

        public void AddProductToCollection(int collection_id, long product_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            poco_collection col_poco = this.GetCollectionPOCO(collection_id, false, false, false, false, false);
            //if (col_poco != null)
            {
                //col_poco.db = this.db;
                col_poco.AddProduct(product_id);
                _db.SaveChanges();
            }
            //else
            //    throw new Exception("A-OK, handled.");
        }

        public void RemoveProductFromCollection(int collection_id, long product_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            poco_collection col_poco = this.GetCollectionPOCO(collection_id, false, false, false, false, false);
            //if (col_poco == null)
            //    throw new Exception("A-OK, handled.");
            //col_poco.db = this.db;
            col_poco.RemoveProduct(product_id);

            _db.SaveChanges();
        }

        public List<poco_tag> Get5TagPOCOs(string contains)
        {
            poco_tag poco = new poco_tag();
            List<poco_tag> tag_pocos = poco.Get5TagPOCOs(contains);

            return tag_pocos;
        }

        public tag GetTag(string name)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            tag tag = _db.tag.Where(t => t.name == name).FirstOrDefault();
            if (tag != null)
                return tag;
            else
                return null;
        }
        
        public string AddCategory(int cat_id, int booth_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            category cat = _db.category.Where(c => c.Id == cat_id).FirstOrDefault();
            booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
            booth.category_main.Add(cat);
            _db.SaveChanges();
            return cat.name;
        }

        public bool RemoveCategory(int cat_id, int booth_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            category cat = _db.category.Where(c => c.Id == cat_id).FirstOrDefault();
            booth booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
            booth.category_main.Remove(cat);
            _db.SaveChanges();
            return true;
        }

        public MESSAGE_TAG SaveTag(string tag_name, string id, TYPE type)
        {
            poco_tag dto = new poco_tag();
            return dto.SaveTag(tag_name, id, type);
        }

        public bool RemoveTag(long tag_id, TYPE type, string id, bool is_updating)
        {
            poco_booth booth_poco = new poco_booth();
            poco_product product_poco = new poco_product(false);
            poco_collection collection_poco = new poco_collection();

            if (type == TYPE.PRODUCT)
            {
                product_poco = product_poco.GetProductPOCO(long.Parse(id), false, false, false, false);
                //if (product_poco == null)
                //    throw new Exception("A-OK, handled.");
                //product_poco.db = this.db;
                return product_poco.RemoveTag(tag_id, is_updating);
            }
            else
            {
                collection_poco = collection_poco.GetCollectionPOCO(int.Parse(id), false, false, false, false, false);
                //if (collection_poco == null)
                //    throw new Exception("A-OK, handled.");
                //collection_poco.db = this.db;
                return collection_poco.RemoveTag(tag_id, is_updating);
            }
        }



        public bool SaveParam(long id, int param_id, int val_id, TYPE type)
        {
            poco_params dto = new poco_params();
            return dto.SaveParam(id, param_id, val_id, type);
        }

        public bool RemoveParam(int param_id, TYPE type, long id)
        {
            poco_product product_dao = new poco_product();
            poco_collection collection_dao = new poco_collection();

            if (type == TYPE.PRODUCT)
            {
                product_dao = product_dao.GetProductPOCO(id, false, false, false, false);
                //if (product_dao == null)
                //    throw new Exception("A-OK Handled.");
                //product_dao.SetupObject(db, httpcon);
                return product_dao.RemoveParam(param_id);
            }
            else
            {
                collection_dao = collection_dao.GetCollectionPOCO(id, false, false, false, false, false);
                //if (collection_dao == null)
                //    throw new Exception("A-OK Handled.");
                //collection_dao.SetupObject(/*db, */httpcon);
                return collection_dao.RemoveParam(param_id);
            }
        }



        public void SavePerson<T>(T dto) where T:poco_person, new()
        {
            dto.SavePerson<T>();
        }

        public void UpdatePerson<T>(T dto) where T: poco_person, new()
        {
            dto.UpdatePerson<T>();
        }

        public poco_conversation GetConversation(long id, string person_id, TYPE type)
        {
            poco_conversation conversation_poco = new poco_conversation();
            if(type == TYPE.PRODUCT)
                return conversation_poco.GetConversationPOCO(id, -1, -1, person_id, type);
            else if (type == TYPE.COLLECTION)
                return conversation_poco.GetConversationPOCO(-1, (int)id, -1, person_id, type);
            else 
                return conversation_poco.GetConversationPOCO(-1, -1, (int)id, person_id, type);
        }

        public string SaveMessage(long? conversation_id, long product_id, string person_id, string message, TYPE type)
        {
            poco_conversation conversation_poco = new poco_conversation();
            return conversation_poco.SaveMessage(conversation_id, product_id, person_id, message, type);
        }

        public void AddFavorite(long product_id, int collection_id)
        {
            poco_person current_user = CurrentUser.GetInstance().GetCurrentUser(false, false, false);
            if(current_user != null)
                current_user.AddFavorite(product_id, collection_id);
        }

        public void RemoveFavorite(long product_id, int collection_id)
        {
            poco_person current_user = CurrentUser.GetInstance().GetCurrentUser(false, false, false);
            if (current_user != null)
                current_user.RemoveFavorite(product_id, collection_id);
        }

        public void AddFollowing(int booth_id)
        {
            poco_person current_user = CurrentUser.GetInstance().GetCurrentUser(false, false, false);
            if (current_user != null)
                current_user.AddFollowing(booth_id);
        }

        public void RemoveFollowing(int booth_id)
        {
            poco_person current_user = CurrentUser.GetInstance().GetCurrentUser(false, false, false);
            if (current_user != null)
                current_user.RemoveFollowing(booth_id);            
        }

        public void DeleteConversation(long con_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            conversation con = _db.conversation.Where(c => c.Id == con_id).FirstOrDefault();
            if (con != null)
            {
                foreach (comment com in con.comment.ToList())
                    con.comment.Remove(com);
                _db.conversation.Remove(con);
                _db.SaveChanges();
            }
        }

        public List<poco_folder> GetFolderTree(int booth_id, bool withbooth)
        {
            poco_folder fa = new poco_folder();
            List<poco_folder> fld_a_pocos = new List<poco_folder>();
            fld_a_pocos = fa.GetFolderAPOCOs(booth_id, withbooth);
            
            return fld_a_pocos.OrderBy(l => l.priority).ToList();
        }

        public void CreateFolder(string fld_name, int id, TYPE type)
        {
            poco_folder a = new poco_folder();
            poco_folder b = new poco_folder();
            if (type == TYPE.FOLDER_A)
                a.CreateFolderForA(fld_name, id);
            else
                b.CreateFolderForB(fld_name, id);
        }

        public void MoveFolder(int fld_id, string direction, int id, TYPE type)
        {
            poco_folder a = new poco_folder();
            poco_folder b = new poco_folder();
            if (type == TYPE.FOLDER_A)
                a.MoveFolderForA(fld_id, direction, id);
            else
                b.MoveFolderForB(fld_id, direction, id);
        }

        public void DeleteFolder(int fld_id, int id, TYPE type)
        {
            poco_folder a = new poco_folder();
            poco_folder b = new poco_folder();
            if (type == TYPE.FOLDER_A)
                a.DeleteFolderForA(fld_id, id);
            else
                b.DeleteFolderForB(fld_id, id);
        }

        public void SetFolder(int fld_id, string id, TYPE type, bool is_product)
        {
            poco_product p = new poco_product(false);
            poco_collection c = new poco_collection();
            if (is_product)
                p.SetFolder(fld_id, long.Parse(id), type);
            else
                c.SetFolder(fld_id, int.Parse(id), type);
        }

        public bool AddRating(int booth_id, string person_id, short rating)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
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

        public List<poco_conversation> GetConversationsPerson(string person_id, bool is_salesman, bool withboothsalesman)
        {
            poco_conversation c = new poco_conversation();
            return c.GetConversationsPersonPOCO(person_id, is_salesman, withboothsalesman);

        }

        public string SetActive(long id, bool value, string type)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
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

        public bool ChangeBoothId(int BoothId, long ProductId)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
            //SetupCurrentUser();
            poco_person currentuser = CurrentUser.GetInstance().GetCurrentUser(false, false, false);
            if (currentuser == null)
                throw new Exception("A-OK, handled.");
            
                poco_product product_poco = new poco_product(false);
                poco_booth booth_poco = new poco_booth();
                product_poco = product_poco.GetProductPOCO(ProductId, true, false, false, false);
                //if (product_poco == null)
                //    throw new Exception("A-OK, handled.");
                booth_poco = booth_poco.GetBoothPOCO(BoothId, "", "", true, true, false, false, false, false, true);
                //if (booth_poco == null)
                //    throw new Exception("A-OK, handled.");
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

                Dictionary<string, string> dirs_product = Setup.SetupProductDirs(product_poco, currentuser.sysname);
                Dictionary<string, string> dirs_booth = Setup.SetupBoothDirs(ref booth_poco, currentuser.sysname);
                string nd = Path.DirectorySeparatorChar.ToString();
                string old_path = Paths.GetPath(PATH.PRODUCT_DIRECTORY_NAME, dirs_product, true);// + product_poco.sysname + nd;
                string new_path = Paths.GetPath(PATH.BOOTH_DIRECTORY, dirs_booth, true) + booth_poco.sysname + nd + "products" + nd + product_poco.sysname + nd;
                foreach (IImage im in product_poco.image_pocos)
                {
                    Paths.MoveFile(old_path, im.name, new_path, im.name, true, false, false, false);
                    Paths.MoveFile(old_path, "t_" + im.name, new_path, "t_" + im.name, true, false, false, false);
                }
                Paths.ClearFolder(old_path, true, true);
                //pro.booth_id = BoothId;



                EbazarDB db = GetContext();
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        //db.SaveChanges();
                        category top = db.category.Where(x=>x.is_parent).OrderBy(x=>x.priority).FirstOrDefault();
                        category sec = top.children.OrderBy(c => c.priority).ElementAt(top.children.Count() - 1);

                        pro.folder_a_id = null;
                        pro.folder_b_id = null;

                        if (pro.category_main != null)
                            pro.category_main = top;
                        if (pro.category_second != null)
                            pro.category_second = sec;
                        pro.category_main_id = top.Id;
                        pro.category_second_id = sec.Id;


                        db.SaveChanges();

                        new_booth.product.Add(pro);
                        if (!new_booth.category_main.Contains(top))
                            new_booth.category_main.Add(top);
                        old_booth.product.Remove(pro);

                        db.SaveChanges();

                        //category old_cat = db.category.Where(c => c.Id == pro.category_main_id).FirstOrDefault();
                        //category old_cat_sec = db.category.Where(c => c.Id == pro.category_second_id).FirstOrDefault();
                        //old_cat_sec.product_second.Remove(pro);
                        //old_cat.product_main.Remove(pro);
                        //if (old_cat.product_main.Where(c => c.booth_id == old_booth.Id).Count() + old_cat.collection_main.Where(c => c.booth_id == old_booth.Id).Count() == 0)
                        //    old_booth.category_main.Remove(old_cat);

                        db.SaveChanges();
                        dbContextTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        //Log, handle or absorbe I don't care ^_^
                    }
                }
                return true;
            }
            return false;

        }

        public string GetAddressTown(string zip)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();
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