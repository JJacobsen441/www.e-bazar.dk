using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models.DataAccess;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_collectionimage : IImage
    {
        //private EbazarDB db;// = new EbazarDB();

        /*private poco_collectionimage()
        {

        }*/
        public poco_collectionimage()
        {
            //this.db = new EbazarDB();
        }
        /*~poco_collectionimage()
        {
            db?.Dispose();
        }*/

        public long id { get; set; }
        
        public string name { get; set; }

        public DateTime created_on { get; set; }
        
        public long? _id { get; set; }

        public List<image> GetImages(long o_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            List<image> images = (from pi in _db.image
                                  where pi.is_product == false && pi.collection_id == o_id
                                  select new 
                                  {
                                      Id = pi.Id,
                                      name = pi.name,
                                      created_on = pi.created_on,
                                      collection_id = pi.collection_id
                                      //collection_id = pi.collection_id
                                  }).AsEnumerable()
                                  .Select(pi=>new image {
                                      Id = pi.Id,
                                      name = pi.name,
                                      created_on = pi.created_on,
                                      collection_id = pi.collection_id
                                      //collection_id = pi.collection_id
                                  }).ToList<image>();
            if (images == null)
                throw new Exception("A-OK, Handled.");
            return images;
        }

        public List<IImage> GetCollectionImagePOCOs(long collection_id) { return GetImagePOCOs(collection_id); }
        public List<IImage> GetImagePOCOs(long o_id)
        {
            List<image> images = GetImages(o_id).ToList();
            List<IImage> list = this.ToPocoList(images);
            
            return list;
        }

        public List<IImage> ToPocoList(ICollection<image> images)
        {
            if (images == null)
                throw new Exception("A-OK, Check.");

            List<IImage> res = new List<IImage>();
            foreach (image i in images.ToList())
            {
                poco_collectionimage pi = new poco_collectionimage();
                pi.ToPoco(i);
                res.Add(pi);
            }
            return res;
        }
        public void ToPoco(image i)
        {
            if (i == null)
                throw new Exception("A-OK, Check.");

            this.id = i.Id;
            name = i.name;
            created_on = i.created_on;
            _id = i.collection_id;
        }

        public void SaveCollectionImages(long collection_id, List<string> fnames) { SaveImages(collection_id, fnames); }
        public void SaveImages(long o_id, List<string> fnames)
        {
            if (fnames == null)
                throw new Exception("A-OK, Check.");

            EbazarDB _db = DAL.GetInstance().GetContext();

            collection collection = _db.collection.Where(c => c.Id == o_id).FirstOrDefault();
            if(collection == null)
                throw new Exception("A-OK, Handled.");
            foreach (string fname in fnames)
            {
                image c = new image();
                c.name = string.IsNullOrEmpty(fname) ? "" : fname;
                c.created_on = DateTime.Now;
                c.collection_id = collection.Id;
                c.collection = collection;
                c.is_product = false;
                _db.image.Add(c);
            }
            _db.SaveChanges();
            _db.Dispose();
        }
        public void DeleteCollectionImages(long collection_id) { DeleteImages(collection_id); }
        public void DeleteImages(long o_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            List<image> collectionimages = _db.image.Where(c => c.collection_id == o_id).ToList();
            if (collectionimages == null)
                throw new Exception("A-OK, Check.");

            foreach (image c in collectionimages)
                _db.image.Remove(c);
            _db.SaveChanges();
        }
        private void Sanitize(ref poco_productimage dto)
        {
            ;
        }
    }
}