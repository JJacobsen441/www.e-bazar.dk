using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_tag
    {
        //private EbazarDB db;// = new EbazarDB();


        public long tag_id { get; set; }
        public string name { get; set; }
        public string form { get; set; }
        public int numberofhits { get; set; }
        //public List<booth> booth { get; set; }
        public List<poco_product> product { get; set; }
        public List<poco_collection> collection { get; set; }

        /*private poco_tag()
        {
        }*/
        public poco_tag()
        {
            //this.db = new EbazarDB();
        }
        /*~poco_tag() 
        {
            db?.Dispose();
        }*/
        
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

        public List<poco_tag> Get5TagPOCOs(string contains)
        {
            List<tag> tags = GetTagsStartsWith(contains);
            return this.ToPocoList(tags).OrderByDescending(t => t.numberofhits).Take(5).ToList();
           
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

                form = "primary";
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

        public List<poco_tag> ToPocoList(ICollection<tag> tags)
        {
            if (tags == null)
                throw new Exception("A-OK, Check.");

            List<poco_tag> res = new List<poco_tag>();
            foreach (tag t in tags.ToList())
            {
                poco_tag tag = new poco_tag();
                tag.ToPoco(t);
                res.Add(tag);
            }
            return res;
        }

        public void ToPoco(tag tag)
        {
            if (tag == null)
                throw new Exception("A-OK, Check.");

            //EbazarDB _db = DAL.GetInstance().GetDB();

            using (EbazarDB _db = new EbazarDB()) 
            {
                this.tag_id = tag.Id;
                this.name = tag.name;
                this.form = tag.form;

                IQueryable<tag> _t = _db.tag
                    .Include("product")
                    .Include("collection")
                .Where(t => t.Id == tag.Id);
                

                _t.ToTraceStringB();

                tag tmp = _t.AsEnumerable().FirstOrDefault();
                if(tmp.IsNotNull() && tmp.product.IsNotNull() && tmp.collection.IsNotNull())
                    numberofhits = tmp.product.Count() + tmp.collection.Count();

                this.product = new List<poco_product>();
                if (tag.product != null && tag.product.Count() > 0)
                {
                    foreach (product pro in tag.product)
                    {
                        poco_product poco = new poco_product(false);
                        poco.ToPoco(pro, null, "");
                        this.product.Add(poco);
                    }
                }
                this.collection = new List<poco_collection>();
                if (tag.collection != null && tag.collection.Count() > 0)
                {
                    foreach (collection col in tag.collection)
                    {
                        poco_collection poco = new poco_collection();
                        poco.ToPoco(col, null, "");
                        this.collection.Add(poco);
                    }
                }
            }
        }
    }
}
