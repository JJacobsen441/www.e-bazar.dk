using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_tag
    {
        public biz_tag()
        {
            //this.db = new EbazarDB();
        }
        
        public List<tag> GetTagsStartsWith(string contains)
        {
            if (string.IsNullOrEmpty(contains))
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                List<tag> tags = new List<tag>();
                tags = (from t in _db.tag
                         where t.name.StartsWith(contains)
                        select new
                        {
                            Id = t.Id,
                            name = t.name,
                            form = t.form
                            //product = t.product,
                            //collection = t.collection
                        }).AsEnumerable()
                        .Select(t => new tag
                        {
                            Id = t.Id,
                            name = t.name,
                            form = t.form
                        }).ToList();

                if (tags == null)
                    return new List<tag>();
                return tags;
            }
        }

        public List<dto_tag> Get5TagDTOs(string contains)
        {
            List<tag> tags = GetTagsStartsWith(contains);
            return this.ToDTOList(tags).OrderByDescending(t => t.numberofhits).Take(5).ToList();
           
        }

        public MESSAGE_TAG SaveTag(string tag_name, string id, TYPE type)
        {
            int _id;
            if (string.IsNullOrEmpty(id))
                throw new Exception("A-OK, Check.");
            if (!int.TryParse(id, out _id))
                throw new Exception("A-OK, Check.");
            if (string.IsNullOrEmpty(tag_name))
                return MESSAGE_TAG.EMPTYNAME;


            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                string form = "primary";
                bool add_to_tags = false;
                tag tag = _db.tag.Where(t => t.name == tag_name).Select(t => t).FirstOrDefault();
                if (tag == null)
                {
                    tag = new tag();
                    tag.name = tag_name;
                    tag.form = form;
                    add_to_tags = true;
                }

                product product = null;
                collection collection = null;
                if (type == TYPE.PRODUCT)
                {
                    product = _db.product.Where(p => p.Id == _id).Select(p => p).FirstOrDefault();
                    if (product.tag.Count >= 5)
                        return MESSAGE_TAG.MAXLIMIT;
                    product.tag.Add(tag);
                }
                else if (type == TYPE.COLLECTION)
                {
                    collection = _db.collection.Where(c => c.Id == _id).Select(c => c).FirstOrDefault();
                    if (collection.tag.Count >= 10)
                        return MESSAGE_TAG.MAXLIMIT;
                    collection.tag.Add(tag);
                }
                if (add_to_tags)
                    _db.tag.Add(tag);
                _db.SaveChanges();
                return MESSAGE_TAG.OK;

            }
        }

        private product Null(product p)
        {
            if (p.booth != null)
                p.booth = null;
            if (p.category_main != null)
                p.category_main = null;
            if (p.category_second != null)
                p.category_second = null;
            if (p.collection != null)
                p.collection = null;
            if (p.conversation != null)
                p.conversation = null;
            if (p.favorites != null)
                p.favorites = null;
            if (p.foldera != null)
                p.foldera = null;
            if (p.folderb != null)
                p.folderb = null;
            if (p.image != null)
                p.image = null;
            if (p.product_param != null)
                p.product_param = null;
            return p;
        }

        private collection Null(collection c)
        {
            if (c.booth != null)
                c.booth = null;
            if (c.category_main != null)
                c.category_main = null;
            if (c.category_second != null)
                c.category_second = null;
            if (c.product != null)
                c.product = null;
            if (c.conversation != null)
                c.conversation = null;
            if (c.favorites != null)
                c.favorites = null;
            if (c.foldera != null)
                c.foldera = null;
            if (c.folderb != null)
                c.folderb = null;
            if (c.image != null)
                c.image = null;
            if (c.collection_param != null)
                c.collection_param = null;
            return c;
        }

        public List<dto_tag> ToDTOList(ICollection<tag> tags)
        {
            if (tags == null)
                throw new Exception("A-OK, Check.");

            List<dto_tag> res = new List<dto_tag>();
            biz_tag biz = new biz_tag();
            foreach (tag t in tags.ToList())
            {
                dto_tag tag = new dto_tag();
                tag = biz.ToDTO(t);
                res.Add(tag);
            }
            return res;
        }

        public dto_tag ToDTO(tag tag)
        {
            if (tag == null)
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetDB();
            dto_tag dto = new dto_tag();
            using (EbazarDB _db = new EbazarDB()) 
            {
                dto.tag_id = tag.Id;
                dto.name = tag.name;
                dto.form = tag.form;

                IQueryable<tag> _t = _db.tag
                    .Include("product")
                    .Include("collection")
                .Where(t => t.Id == tag.Id);
                

                _t.ToTraceStringD();

                tag tmp = _t.AsEnumerable().FirstOrDefault();
                if(tmp.IsNotNull() && tmp.product.IsNotNull() && tmp.collection.IsNotNull())
                    dto.numberofhits = tmp.product.Count() + tmp.collection.Count();

                dto.product = new List<dto_product>();
                if (tag.product != null && tag.product.Count() > 0)
                {
                    foreach (product pro in tag.product)
                    {
                        biz_product biz = new biz_product(false);
                        dto_product _dto = new dto_product();
                        _dto = biz.ToDTO(pro, null, "");
                        dto.product.Add(_dto);
                    }
                }
                dto.collection = new List<dto_collection>();
                if (tag.collection != null && tag.collection.Count() > 0)
                {
                    foreach (collection col in tag.collection)
                    {
                        biz_collection biz = new biz_collection();
                        dto_collection _dto = new dto_collection();
                        _dto = biz.ToDTO(col, null, "");
                        dto.collection.Add(_dto);
                    }
                }
            }

            return dto;
        }
    }
}
