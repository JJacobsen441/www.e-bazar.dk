using System;
using System.Collections.Generic;
using System.Linq;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_image
    {
        public List<dto_image> ToDTOList(ICollection<image> images)
        {
            if (images == null)
                throw new Exception("A-OK, Check.");

            List<dto_image> res = new List<dto_image>();
            foreach (image i in images.ToList())
            {
                biz_image biz = new biz_image();
                dto_image dto = new dto_image();
                dto = biz.ToDTO(i);
                res.Add(dto);
            }
            return res;
        }

        public dto_image ToDTO(image i)
        {
            if (i == null)
                throw new Exception("A-OK, Check.");

            dto_image dto = new dto_image();

            dto.id = i.Id;
            dto.name = i.name;
            dto.created_on = i.created_on;
            dto._id = i.collection_id;

            return dto;
        }

        public void SaveImages(bool is_pro, long o_id, List<string> fnames)
        {
            if (fnames == null)
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                if (is_pro)
                {
                    product pro = _db.product.Where(c => c.Id == o_id).FirstOrDefault();
                    if (pro == null)
                        throw new Exception("A-OK, Handled.");

                    foreach (string fname in fnames)
                    {
                        image c = new image();
                        c.name = string.IsNullOrEmpty(fname) ? "" : fname;
                        c.created_on = DateTime.Now;
                        c.product_id = pro.Id;
                        c.product = pro;
                        c.is_product = true;
                        _db.image.Add(c);
                    }
                }
                else
                {
                    collection collection = _db.collection.Where(c => c.Id == o_id).FirstOrDefault();
                    if (collection == null)
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
                }
                _db.SaveChanges();
            }
        }
    }
}