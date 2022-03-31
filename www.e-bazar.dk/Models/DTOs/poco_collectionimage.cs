using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Interfaces;

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

        public void SaveImages(long o_id, List<string> fnames)
        {
            if (fnames == null)
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


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
            }
        }
    }
}