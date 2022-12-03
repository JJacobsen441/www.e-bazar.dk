using System;
using System.Collections.Generic;
using System.Linq;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_folder
    {
        public folder GetFolderA(long fol_a_id, bool withbooth, bool withproducts)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                folder foldera = (from l in _db.folder
                              where l.Id == fol_a_id
                            select new 
                            {
                                Id = l.Id,
                                name = l.name,
                                priority = l.priority,
                                booth_id = l.booth_id,
                                is_parent = l.is_parent,
                                product = l.product,
                                collection = l.collection,
                                product1 = l.product1,
                                collection1 = l.collection1
                            }).AsEnumerable()
                            .Select(l=> new folder
                            {
                                Id = l.Id,
                                name = l.name,
                                priority = l.priority,
                                booth_id = l.booth_id,
                                is_parent = l.is_parent,
                                product = l.product,
                                collection = l.collection,
                                product1 = l.product1,
                                collection1 = l.collection1
                            }).FirstOrDefault(); 
                if(foldera == null)
                    throw new Exception("A-OK, Handled.");
                return foldera;
            }
        }

        public dto_folder GetFolderADTO(long? fld_a_id, bool withbooth, bool withproducts)
        {
            if (fld_a_id == null)
                return new dto_folder();
            biz_folder biz = new biz_folder();
            dto_folder dto = new dto_folder();
            folder foldera = GetFolderA((long)fld_a_id, withbooth, withproducts);
            dto = biz.ToDTO(foldera);
            return dto;            
        }

        public List<folder> GetFolderAs(int booth_id, bool withbooth)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                biz_folder folderb = new biz_folder();
                biz_booth booth = new biz_booth();
                List<folder> folders = (from l in _db.folder
                                        where l.booth_id == booth_id
                                       select new 
                                       {
                                           Id = l.Id,
                                           name = l.name,
                                           priority = l.priority,
                                           booth_id = l.booth_id,
                                           is_parent = l.is_parent,
                                           booth = withbooth ? l.booth : null,
                                           product = l.product,
                                           collection = l.collection,
                                           product1 = l.product1,
                                           collection1 = l.collection1
                                       }).AsEnumerable()
                                       .Select(l=>new folder
                                       {
                                           Id = l.Id,
                                           name = l.name,
                                           priority = l.priority,
                                           booth_id = l.booth_id,
                                           is_parent = l.is_parent,
                                           booth = withbooth ? l.booth : null,
                                           children = folderb.GetFolderBs(l.Id),
                                           product = l.product,
                                           collection = l.collection,
                                           product1 = l.product1,
                                           collection1 = l.collection1
                                       }).ToList();
                if(folders == null)
                    throw new Exception("A-OK, Handled.");
                return folders;
            }
        }

        public List<dto_folder> GetFolderADTOs(int booth_id, bool withbooth)
        {
            List<dto_folder> list = new List<dto_folder>();
            List<folder> folderas = GetFolderAs(booth_id, withbooth);
            folderas = folderas.OrderBy(l => l.priority).ToList();
            return this.ToDTOList(folderas);
        }

        public folder GetFolderB(long fld_b_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                folder folderb = (from l in _db.folder
                              where l.Id == fld_b_id
                             select new
                             {
                                 Id = l.Id,
                                 name = l.name,
                                 priority = l.priority,
                                 parent_id = l.parent_id,
                                 is_parent = l.is_parent,
                                 product = l.product,
                                 collection = l.collection,
                                 product1 = l.product1,
                                 collection1 = l.collection1
                             }).AsEnumerable()
                            .Select(l => new folder
                            {
                                Id = l.Id,
                                name = l.name,
                                priority = l.priority,
                                parent_id = l.parent_id,
                                is_parent = l.is_parent,
                                product = l.product,
                                collection = l.collection,
                                product1 = l.product1,
                                collection1 = l.collection1
                            }).FirstOrDefault();
                if(folderb == null)
                    throw new Exception("A-OK, Handled.");
                return folderb;
            }
        }

        public dto_folder GetFolderBDTO(long? fld_b_id)
        {
            if (fld_b_id == null)
                return new dto_folder();

            biz_folder biz = new biz_folder();
            dto_folder dto = new dto_folder();
            folder folderb = GetFolderB((long)fld_b_id);
            dto = biz.ToDTO(folderb);
            return dto;
            
            //return new biz_folder();
        }

        public List<folder> GetFolderBs(long foldera_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                List<folder> folders = (from l in _db.folder
                                    where l.parent.Id == foldera_id
                                    select new
                                   {
                                       Id = l.Id,
                                       name = l.name,
                                       priority = l.priority,
                                        parent_id = l.parent_id,
                                        is_parent = l.is_parent,
                                        product = l.product,
                                        collection = l.collection,
                                        product1 = l.product1,
                                        collection1 = l.collection1
                                    }).AsEnumerable()
                                  .Select(l => new folder
                                  {
                                      Id = l.Id,
                                      name = l.name,
                                      priority = l.priority,
                                      parent_id = l.parent_id,
                                      is_parent = l.is_parent,
                                      product = l.product,
                                      collection = l.collection,
                                      product1 = l.product1,
                                      collection1 = l.collection1
                                  }).ToList();
                if(folders == null)
                    throw new Exception("A-OK, Handled.");
                return folders;
            }
        }

        public List<dto_folder> GetFolderBDTOs(long? foldera_id)
        {
            if (foldera_id == null)
                return new List<dto_folder>();

            List<folder> folders = GetFolderBs((long)foldera_id);
            folders = folders.OrderBy(l => l.priority).ToList();
            return this.ToDTOList(folders);
        }

        private void swap(folder current, folder other)
        {
            int priority_current = (int)current.priority;
            current.priority = other.priority;
            other.priority = priority_current;
        }

        public void CreateFolderForA(string fld_name, int booth_id)
        {
            if (string.IsNullOrEmpty(fld_name))
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                List<folder> list = _db.folder.Where(l => l.booth_id == booth_id).OrderBy(l => l.priority).ToList();
                if (list == null)
                    throw new Exception("A-OK, Handled.");
            
                int index = 0;

                folder foldera = new folder();
                foldera.name = fld_name;
                foldera.priority = index;
                foldera.is_parent = true;
                foldera.booth = _db.booth.Where(b => b.Id == booth_id).FirstOrDefault();
                for (int i = index; i < list.Count(); i++)
                    list.ElementAt(i).priority = i + 1;
                _db.folder.Add(foldera);
                _db.SaveChanges();
            }
        }

        public void MoveFolderForA(int fld_id, string direction, int booth_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                List<folder> list = _db.folder.Where(l => l.booth_id == booth_id).OrderBy(l => l.priority).ToList();
                if (list == null)
                    throw new Exception("A-OK, Handled.");
            
                folder fld = list.Where(l => l.Id == fld_id).FirstOrDefault();
                if (fld.priority == 0 && direction == "up")
                    return;
                if (fld.priority == list.Count() - 1 && direction == "down")
                    return;

                if (direction == "up")
                {
                    swap(fld, list.Where(l => l.priority == fld.priority - 1).FirstOrDefault());
                }
                if (direction == "down")
                {
                    swap(fld, list.Where(l => l.priority == fld.priority + 1).FirstOrDefault());
                }
                _db.SaveChanges();
            }
        }

        public void DeleteFolderForA(int fld_id, int booth_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                List<folder> list = _db.folder.Where(l => l.booth_id == booth_id).OrderBy(l => l.priority).ToList();
                if (list == null)
                    throw new Exception("A-OK, Handled.");
            
                int index = (int)list.Where(l => l.Id == fld_id).FirstOrDefault().priority + 1;

                folder fld = _db.folder.Where(l => l.Id == fld_id).FirstOrDefault();
                for (int i = index; i < list.Count(); i++)
                    list.ElementAt(i).priority = i - 1;
                int j = fld.children.Count() - 1;
                for (; j >= 0; j--)
                {
                    folder b = fld.children.ElementAt(j);
                    _db.folder.Remove(b);
                }
                _db.folder.Remove(fld);
                _db.SaveChanges();
            }
        }

        public void CreateFolderForB(string fld_name, int parent_id)
        {
            if (string.IsNullOrEmpty(fld_name))
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                folder parent = _db.folder.Where(l => l.Id == parent_id && l.is_parent == true).FirstOrDefault();
                if (parent == null)
                    throw new Exception("A-OK, Handled.");
            
                int index = 0;

                folder folderb = new folder();
                folderb.name = fld_name;
                folderb.priority = index;
                folderb.is_parent = false;
                folderb.parent = parent;
                for (int i = index; i < parent.children.Count(); i++)
                    parent.children.ElementAt(i).priority = i + 1;
                parent.children.Add(folderb);
                _db.folder.Add(folderb);
                _db.SaveChanges();
            }
        }
        
        public void MoveFolderForB(int fld_id, string direction, int parent_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                List<folder> list = _db.folder.Where(l => l.parent_id == parent_id && l.is_parent == false).OrderBy(l => l.priority).ToList();
                if (list != null)
                    throw new Exception("A-OK, Handled.");
            
                folder fld = list.Where(l => l.Id == fld_id).FirstOrDefault();
                if (fld.priority == 0 && direction == "up")
                    return;
                if (fld.priority == list.Count() - 1 && direction == "down")
                    return;

                if (direction == "up")
                {
                    swap(fld, list.Where(l => l.priority == fld.priority - 1).FirstOrDefault());
                }
                if (direction == "down")
                {
                    swap(fld, list.Where(l => l.priority == fld.priority + 1).FirstOrDefault());
                }
                _db.SaveChanges();
            }
            
        }

        public void DeleteFolderForB(int fld_id, int parent_id)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                folder parent = _db.folder.Where(l => l.Id == parent_id && l.is_parent == true).FirstOrDefault();
                if (parent == null)
                    throw new Exception("A-OK, handled.");
            
                List<folder> children = parent.children.OrderBy(c => c.priority).ToList();
                folder fld = children.Where(l => l.Id == fld_id).FirstOrDefault();
                int index = (int)fld.priority + 1;

                for (int i = index; i < children.Count(); i++)
                    children.ElementAt(i).priority = i - 1;
                _db.folder.Remove(fld);
                _db.SaveChanges();
            }
        }

        public void ToFolder(EbazarDB db, dto_folder dto, ref folder f)
        {
            if (!string.IsNullOrEmpty(dto.name))
                f.name = dto.name;
            else
                throw new Exception("A-OK, Handled.");

            if (dto.priority != null)
                f.priority = dto.priority;
            else
                throw new Exception("A-OK, Handled.");

            if (dto.children != null)
                f.children = db.folder.Where(l => l.Id == dto.parent_id).ToList();
            else
                throw new Exception("A-OK, Handled.");

            if (dto.booth != null)
                f.booth = db.booth.Where(b => b.Id == dto.booth.booth_id).FirstOrDefault();
            else
                throw new Exception("A-OK, Handled.");
        }

        public List<dto_folder> ToDTOList(ICollection<folder> folderas)
        {
            if (folderas == null)
                throw new Exception("A-OK, Check");

            List<dto_folder> list = new List<dto_folder>();
            foreach (folder b in folderas.ToList())
            {
                dto_folder folder_dto = new dto_folder();
                folder_dto = this.ToDTO(b);
                list.Add(folder_dto);
            }
            return list;
        }

        public dto_folder ToDTO(folder f)
        {
            if (f == null)
                throw new Exception("A-OK, Check");

            dto_folder dto = new dto_folder();

            if (f.is_parent == true)
                dto.count = (f.product != null ? f.product.Count() : 0) + (f.collection != null ? f.collection.Count() : 0);
            else
                dto.count = (f.product1 != null ? f.product1.Count() : 0) + (f.collection1 != null ? f.collection1.Count() : 0);

            dto.id = f.Id;
            
            if (!string.IsNullOrEmpty(f.name))
                dto.name = f.name;
            else
                throw new Exception("A-OK, Handled.");

            if (f.priority != null)
                dto.priority = f.priority;
            else
                throw new Exception("A-OK, Handled.");

            if (f.booth_id != null)
                dto.booth_id = (int)f.booth_id;

            dto.children = new List<dto_folder>();
            if (f.children != null)
            {
                foreach (folder b in f.children)
                {
                    biz_folder fb = new biz_folder();
                    dto_folder _dto = new dto_folder();
                    _dto = fb.ToDTO(b);
                    dto.children.Add(_dto);
                }
                dto.children = dto.children.OrderBy(l => l.priority).ToList();
            }

            return dto;
        }
    }
}