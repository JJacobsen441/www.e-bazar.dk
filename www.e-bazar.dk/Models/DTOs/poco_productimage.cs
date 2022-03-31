using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using www.e_bazar.dk.Interfaces;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_productimage : IImage
    {
        //private EbazarDB db;// = new EbazarDB();
        /*private poco_productimage()
        {

        }*/
        public poco_productimage()
        {
            //this.db = new EbazarDB();
        }
        /*~poco_productimage()
        {
            //db?.Dispose();
        }*/

        public long id { get; set; }
        [DisplayName("")]
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
                poco_productimage pi = new poco_productimage();
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
            _id = i.product_id;
        }
        
        public void SaveImages(long o_id, List<string> fnames)
        {
            if (fnames == null)
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                product product = _db.product.Where(p => p.Id == o_id).FirstOrDefault();
                if (product == null)
                    throw new Exception("A-OK, Handled.");
                foreach (string fname in fnames)
                {
                    image pi = new image();
                    pi.name = string.IsNullOrEmpty(fname) ? "" : fname;
                    pi.created_on = DateTime.Now;
                    pi.product_id = product.Id;
                    pi.product = product;
                    pi.is_product = true;
                    _db.image.Add(pi);
                }
                _db.SaveChanges();
                _db.Dispose();
            }
        }
    }
}