using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models.DataAccess;

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
        

        public List<image> GetImages(long o_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            List<image> images = (from pi in _db.image
                                  where pi.is_product == true && pi.product_id == o_id
                                  select new 
                                  {
                                      Id = pi.Id,
                                      name = pi.name,
                                      created_on = pi.created_on,
                                      product_id = pi.product_id
                                      //collection_id = pi.collection_id
                                  }).AsEnumerable()
                                  .Select(pi => new image {
                                      Id = pi.Id,
                                      name = pi.name,
                                      created_on = pi.created_on,
                                      product_id = pi.product_id
                                  }).ToList<image>();
            if (images == null)
                throw new Exception("A-OK, Handled.");
            return images;
        }

        public List<IImage> GetProductImagePOCOs(long product_id) { return GetImagePOCOs(product_id); }
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
        public void SaveProductImages(long product_id, List<string> fnames) { SaveImages(product_id, fnames); }
        public void SaveImages(long o_id, List<string> fnames)
        {
            if (fnames == null)
                throw new Exception("A-OK, Check.");

            EbazarDB _db = DAL.GetInstance().GetContext();

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
        public void DeleteProductImages(long product_id) { DeleteImages(product_id); }
        public void DeleteImages(long o_id)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            List<image> productimages = _db.image.Where(pi => pi.product_id == o_id).ToList();
            if (productimages == null)
                throw new Exception("A-OK, Check.");

            foreach (image pi in productimages)
                _db.image.Remove(pi);
            _db.SaveChanges();
        }
        
    }
}