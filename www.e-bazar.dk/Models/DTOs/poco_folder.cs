using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using www.e_bazar.dk.Models.DataAccess;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_folder
    {
        //private EbazarDB db;
        /*private poco_folder()
        {

        }*/
        public poco_folder()
        {
            //this.db = new EbazarDB();
        }
        /*~poco_folder()
        {
            db?.Dispose();
        }*/
        public long id { get; set; }
        [Required]
        [StringLength(50)]
        public string name { get; set; }

        public int? priority { get; set; }
        public int? booth_id { get; set; }
        public int? parent_id { get; set; }
        public bool is_parent { get; set; }
        public int count { get; set; }
        public virtual poco_booth booth { get; set; }
        public virtual List<poco_folder> children { get; set; }

        public folder GetFolderA(long fol_a_id, bool withbooth, bool withproducts)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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
                                //booth = withbooth ? booth.GetBoothPOCO(l.booth_id, "", "", false, false, false, false) : null,
                                //levelb = levelb.GetLevelBPOCOs(l.Id, false, withproducts)
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
                                //booth = withbooth ? booth.GetBoothPOCO(l.booth_id, "", "", false, false, false, false) : null,
                                //levelb = levelb.GetLevelBPOCOs(l.Id, false, withproducts)
                            }).FirstOrDefault(); 
            if(foldera == null)
                throw new Exception("A-OK, Handled.");
            return foldera;
        }

        public poco_folder GetFolderAPOCO(long? fld_a_id, bool withbooth, bool withproducts)
        {
            if (fld_a_id == null)
                return new poco_folder();
            poco_folder poco = new poco_folder();
            folder foldera = GetFolderA((long)fld_a_id, withbooth, withproducts);
            poco.ToPoco(foldera);
            return poco;            
        }



        public List<folder> GetFolderAs(int booth_id, bool withbooth)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            poco_folder folderb = new poco_folder();
            poco_booth booth = new poco_booth();
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
        public List<poco_folder> GetFolderAPOCOs(int booth_id, bool withbooth)
        {
            List<poco_folder> list = new List<poco_folder>();
            List<folder> folderas = GetFolderAs(booth_id, withbooth);
            folderas = folderas.OrderBy(l => l.priority).ToList();
            return this.ToPocoList(folderas);
            
            //return new List<poco_folder>();
        }

        public folder GetFolderB(long fld_b_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            folder folderb = (from l in _db.folder
                              where l.Id == fld_b_id
                             select new
                             {
                                 Id = l.Id,
                                 name = l.name,
                                 priority = l.priority,
                                 parent_id = parent_id,
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
                                parent_id = parent_id,
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
        public poco_folder GetFolderBPOCO(long? fld_b_id)
        {
            if (fld_b_id == null)
                return new poco_folder();

            poco_folder poco = new poco_folder();
            folder folderb = GetFolderB((long)fld_b_id);
            poco.ToPoco(folderb);
            return poco;
            
            //return new poco_folder();
        }
        public List<folder> GetFolderBs(long foldera_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            //poco_product pro_poco = new poco_product();
            //poco_collection col_poco = new poco_collection();
            List<folder> folders = (from l in _db.folder
                                    where l.parent.Id == foldera_id
                                    select new
                                   {
                                       Id = l.Id,
                                       name = l.name,
                                       priority = l.priority,
                                        parent_id = parent_id,
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
                                      parent_id = parent_id,
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
        public List<poco_folder> GetFolderBPOCOs(long? foldera_id)
        {
            if (foldera_id == null)
                return new List<poco_folder>();

            List<folder> folders = GetFolderBs((long)foldera_id);
            folders = folders.OrderBy(l => l.priority).ToList();
            return this.ToPocoList(folders);
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

            EbazarDB _db = DAL.GetInstance().GetContext();

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
        public void MoveFolderForA(int fld_id, string direction, int booth_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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
        public void DeleteFolderForA(int fld_id, int booth_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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
                //lev.levelb.Remove(b);
                _db.folder.Remove(b);
            }
            _db.folder.Remove(fld);
            _db.SaveChanges();
        }
        public void CreateFolderForB(string fld_name, int parent_id)
        {
            if (string.IsNullOrEmpty(fld_name))
                throw new Exception("A-OK, Check");

            EbazarDB _db = DAL.GetInstance().GetContext();

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
        
        public void MoveFolderForB(int fld_id, string direction, int parent_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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
        public void DeleteFolderForB(int fld_id, int parent_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

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
        public void ToFolder(EbazarDB db, ref folder f)
        {
            //bool new_product = string.IsNullOrEmpty(this.name) ? true : false;

            if (!string.IsNullOrEmpty(this.name))
                f.name = this.name;
            else
                throw new Exception("A-OK, Handled.");
            if (this.priority != null)
                f.priority = this.priority;
            else
                throw new Exception("A-OK, Handled.");

            if (this.children != null)
                f.children = db.folder.Where(l => l.Id == this.parent_id).ToList();
            else
                throw new Exception("A-OK, Handled.");

            if (this.booth != null)
                f.booth = db.booth.Where(b => b.Id == this.booth.booth_id).FirstOrDefault();
            else
                throw new Exception("A-OK, Handled.");
        }
        public List<poco_folder> ToPocoList(ICollection<folder> folderas)
        {
            if (folderas == null)
                throw new Exception("A-OK, Check");

            List<poco_folder> list = new List<poco_folder>();
            foreach (folder b in folderas.ToList())
            {
                poco_folder folder_poco = new poco_folder();
                folder_poco.ToPoco(b);
                list.Add(folder_poco);
            }
            return list;
        }
        public void ToPoco(folder f)
        {
            if (f == null)
                throw new Exception("A-OK, Check");

            if (f.is_parent == true)
                this.count = (f.product != null ? f.product.Count() : 0) + (f.collection != null ? f.collection.Count() : 0);
            else
                this.count = (f.product1 != null ? f.product1.Count() : 0) + (f.collection1 != null ? f.collection1.Count() : 0);

            this.id = f.Id;
            
            if (!string.IsNullOrEmpty(f.name))
                this.name = f.name;
            else
                throw new Exception("A-OK, Handled.");

            if (f.priority != null)
                this.priority = f.priority;
            else
                throw new Exception("A-OK, Handled.");

            if (f.booth_id != null)
                this.booth_id = (int)f.booth_id;
            //else
            //    throw new Exception();

            //List<levelb> levb = db.levelb.Where(l => l.levela.Id == this.level_a_id).ToList();
            this.children = new List<poco_folder>();
            if (f.children != null)
            {
                foreach (folder b in f.children)
                {
                    poco_folder fb = new poco_folder();
                    fb.ToPoco(b);
                    this.children.Add(fb);
                }
                this.children = this.children.OrderBy(l => l.priority).ToList();
            }
            
            /*if (this.booth != null)
                la.booth = db.booth.Where(b => b.Id == this.booth.booth_id).FirstOrDefault();
            else
                throw new Exception();*/
        }
    }
}