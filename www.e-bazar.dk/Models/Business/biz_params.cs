using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_params
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public int prio { get; set; }
        public int? category_id { get; set; }

        public biz_category category_dao { get; set; }
        public List<biz_value> values_daos { get; set; }

        public biz_params()
        {           
        }
        
        public bool SaveParam(long _id, int param_id, int val_id, TYPE type)
        {
            using (EbazarDB _db = new EbazarDB())
            {


                param param = _db.param.Where(t => t.Id == param_id).Select(t => t).Include("value1").FirstOrDefault();
                if (param == null)
                    return false;

                product product = null;
                collection collection = null;
                if (type == TYPE.PRODUCT)
                {
                    product = _db.product
                        .Where(p => p.Id == _id).Select(p => p).FirstOrDefault();

                    if (product == null)
                        return false;

                    if (product.product_param.Select(x=>x.param).Contains(param) && param.type == "S")
                        return true;

                    if (product.product_param.Select(x => x.param).Contains(param) && param.type == "MS" && param.value.Count() > 0)
                        return false;

                    List<value> chosen = product.product_param.Where(x => x.param_id == param_id && x.value_id == val_id).Select(x => x.value).ToList();
                    if (product.product_param.Select(x => x.param).Contains(param) && param.type == "M" && chosen.Count() > 0)//skulle jo ikke ske
                        return true;

                    int? v = val_id;
                    if (v == -1)
                        v = null;

                    product.product_param.Add(new product_param { product_id = product.Id, param_id = param_id, value_id = v });
                }
                else if (type == TYPE.COLLECTION)
                {
                    collection = _db.collection
                        .Where(c => c.Id == _id).Select(c => c).FirstOrDefault();
                    if (collection == null)
                        return false;

                    if (collection.collection_param.Select(x => x.param).Contains(param) && param.type == "S")
                        return true;

                    if (collection.collection_param.Select(x => x.param).Contains(param) && param.type == "MS" && param.value1.Count() > 0)
                        return false;

                    List<value> chosen = collection.collection_param.Where(x => x.param_id == param_id && x.value_id == val_id).Select(x => x.value).ToList();
                    if (collection.collection_param.Select(x => x.param).Contains(param) && param.type == "M" && chosen.Count() > 0)//skulle jo ikke ske
                        return true;

                    int? v = val_id;
                    if (v == -1)
                        v = null;
                    collection.collection_param.Add(new collection_param { collection_id = collection.Id, param_id = param_id, value_id = v });
                }

                _db.SaveChanges();
                _db.Dispose();
                return true;
            }
        }

        public List<dto_params> ToDTO_List(List<param> param)
        {
            if (param== null)
                throw new Exception("A-OK, Check");

            List<dto_params> list = new List<dto_params>();
            foreach (param par in param)
            {
                biz_params biz = new biz_params();
                dto_params dto = new dto_params();
                dto = biz.ToDTO(par);
                list.Add(dto);
            }
            return list.OrderBy(x => x.prio).ToList();
        }

        public dto_params ToDTO(param par)
        {
            if (par == null)
                throw new Exception("A-OK, Check");

            dto_params dto = new dto_params();

            dto.Id = par.Id;
            dto.name = par.name;
            dto.type = par.type;
            dto.value = par.value;
            dto.prio = par.prio;
            dto.category_id = par.category_id;

            biz_category cat_dao = new biz_category();
            biz_collection col_dao = new biz_collection();
            biz_product pro_dao = new biz_product();
            biz_value val_dao = new biz_value();

            if (par.category != null)
            {
                biz_category biz = new biz_category();
                dto.category_dao = biz.ToDTO(par.category, false, false);
            }

            if (par.value1 != null)
            {
                dto.values_daos = val_dao.ToDTO_List(par.value1.ToList());
            }

            return dto;
        }
    }
}
