using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;
using static www.e_bazar.dk.Models.DTOs.dto_booth;

namespace www.e_bazar.dk.Models.DTOs
{
    public abstract class biz_booth_item// : IBoothItem
    {
        public abstract long Save<T>(T dto) where T : dto_booth_item;
        public abstract void Update<T>(T dto) where T : dto_booth_item;
        public abstract void Delete(long id, EbazarDB db);
        public abstract bool RemoveTag<T>(T dto, long tag_id, bool is_up_dating) where T : dto_booth_item;
        public abstract bool RemoveParam<T>(T dto, int param_id) where T : dto_booth_item;
        public abstract void RemoveImage<T>(string image_name, T dto) where T : dto_booth_item;
        public abstract dto_booth GetBoothDTO<T>(T dto) where T : dto_booth_item;

        public T SetupToClient<T>(T dto) where T : dto_booth_item
        {

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                if (!dto.IsNull()/* && !dto.booth_dto.IsNull()*/)
                {
                    if (!dto.category_main_id.IsNull())
                    {
                        biz_category cat_poco = new biz_category();
                        List<dto_category> cats_top = cat_poco._GetAll(true, true);

                        dto.category_main_selectlist = new SelectList(cats_top, "category_id", "name", dto.category_main_id);

                        dto.category_second_selectlist = dto.category_second_id == 0 ?
                            new SelectList(cats_top.OrderBy(x => x.priority).FirstOrDefault().children, "category_id", "name", dto.category_second_id) :
                            new SelectList(cats_top.Where(x => x.category_id == dto.category_main_id).FirstOrDefault().children, "category_id", "name", dto.category_second_id);
                    }

                    dto_folder tmpa = dto.foldera;
                    dto_folder tmpb = dto.folderb;

                    List<folder> lista = new List<folder>() { new folder() { Id = -1, name = "ingen.." } };
                    lista = lista.Concat(_db.folder.Where(l => l.booth_id == dto.booth_id).OrderBy(l => l.priority)).ToList();

                    List<folder> listb = new List<folder>() { new folder() { Id = -1, name = "ingen.." } };
                    listb = !tmpa.IsNull() ? listb.Concat(_db.folder.Where(l => l.parent_id == tmpa.id).OrderBy(l => l.priority)).ToList() : listb;

                    dto.foldera_selectlist = !tmpa.IsNull() && lista.Count() > 0 ?
                        new SelectList(lista, "Id", "name", tmpa.id) :
                        new SelectList(lista, "Id", "name", -1);

                    dto.folderb_selectlist = !tmpa.IsNull() && !tmpb.IsNull() && listb.Count() > 0 ?
                        new SelectList(listb, "Id", "name", tmpb.id) :
                        new SelectList(listb, "Id", "name", -1);
                }

                dto.status_stock = string.IsNullOrEmpty(dto.status_stock) ? STOCK.PÅ_LAGER.ToString() : dto.status_stock;
                dto.status_stock_selectlist = EnumHelper.SelectListFor(TextHelper.GetStockEnum(dto.status_stock));

                dto.status_condition = string.IsNullOrEmpty(dto.status_condition) ? CONDITION.VELHOLDT.ToString() : dto.status_condition;
                dto.status_condition_selectlist = EnumHelper.SelectListFor(TextHelper.GetConditionEnum(dto.status_condition));

                dto.price = string.IsNullOrEmpty(dto.price) ? NOP.INGEN_PRIS.ToString() : dto.price;
                dto.note = string.IsNullOrEmpty(dto.note) ? TextHelper.GetNopValue(NOP.NO_NOTE.ToString()) : dto.note;
                dto.description = string.IsNullOrEmpty(dto.description) ? TextHelper.GetNopValue(NOP.NO_DESCRIPTION.ToString()) : dto.description;

                if (dto.tag_dtos == null || dto.tag_dtos.Count == 0)
                {
                    dto.tag_dtos = null;
                    dto.tag_dtos_nop = TextHelper.GetNopValue(NOP.NO_TAGS.ToString());
                }
            }

            return dto;
        }



        public bool IsRelevant<T>(T dto, product pro, collection col, bool counting, string b_name, RelevantHelper helper) where T: dto_booth_item
        {
            if (pro.IsNull() && col.IsNull())
                throw new Exception("A-OK, Check.");

            if (!pro.IsNull() && !col.IsNull())
                throw new Exception("A-OK, Check.");

            if (b_name.IsNull())
                throw new Exception("A-OK, Check.");

            if (helper.IsNull())
                throw new Exception("A-OK, Check.");

            bool is_pro = !pro.IsNull();

            string[] opt, cats;
            string op1, op2, op3, op4, op5, op6, cat;
            int fra, til, zip;
            //RelevantHelper helper = RelevantHelper._Create(false);
            helper.GetVals(out opt, out op1, out op2, out op3, out op4, out op5, out op6, out cats, out cat, out fra, out til, out zip);

            string cat_main = is_pro ? pro.category_main.name : col.category_main.name;
            string cat_second = is_pro ? (pro.category_second != null ? pro.category_second.name : null) : (col.category_second != null ? col.category_second.name : null);

            bool ok;
            string desc = is_pro ?  (StringHelper.OnlyAlphanumeric(pro.description.ToLower().Trim(), false, false, "notag", CharacterHelper.Space(), out ok)) :
                                    (StringHelper.OnlyAlphanumeric(col.description.ToLower().Trim(), false, false, "notag", CharacterHelper.Space(), out ok));

            bool relevantA = is_pro ? (pro.price == NOP.INGEN_PRIS.ToString() ?
                            pro.active && !string.IsNullOrEmpty(pro.price) && true :
                            (pro.active && !string.IsNullOrEmpty(pro.price) && int.Parse(pro.price) >= fra && int.Parse(pro.price) <= til)) :
                            (col.joinedprice == NOP.INGEN_PRIS.ToString() ?
                            col.active && !string.IsNullOrEmpty(col.joinedprice) && true :
                            (col.active && !string.IsNullOrEmpty(col.joinedprice) && int.Parse(col.joinedprice) >= fra && int.Parse(col.joinedprice) <= til));

            bool relevantB = is_pro ? ((opt[0] == "") ?
                            true :
                            ((opt.Where(x => x != null && x != "" && pro.name.ToLower().Trim().Contains(x))).Count() > 0 && pro.active ||
                            (pro.tag != null && opt.Where(x => x != null && x != "" && pro.tag.Where(t => t.name == x).Count() > 0).Count() > 0 && pro.active) ||
                            (opt.Where(x => x != null && x != "" && desc.Contains(x)).Count() > 0 && pro.active))) :
                            ((opt[0] == "") ?
                            true :
                            ((opt.Where(x => x != null && x != "" && col.name.ToLower().Trim().Contains(x))).Count() > 0 && col.active ||
                            (col.tag != null && opt.Where(x => x != null && x != "" && col.tag.Where(t => t.name == x).Count() > 0).Count() > 0 && col.active) ||
                            (opt.Where(x => x != null && x != "" && desc.Contains(x)).Count() > 0 && col.active)));

            bool relevantC = is_pro ? ((
                            (cat == "alle") ?
                            pro.active &&
                            true :

                            (!counting && cat != "alle" && cats.Count() == 1) ?
                            pro.active &&
                            cat == cat_main :

                            (!counting && cat != "alle" && cats.Count() > 1) ?
                            pro.active &&
                            cat == cat_main &&
                            (pro.category_second != null && cats.Contains(cat_second)) :

                            (counting && cat != "alle") ?
                            pro.active &&
                            cat == cat_main :

                            false)) :

                            ((
                            (cat == "alle") ?
                            col.active &&
                            true :

                            (!counting && cat != "alle" && cats.Count() == 1) ?
                            col.active &&
                            cat == cat_main :

                            (!counting && cat != "alle" && cats.Count() > 1) ?
                            col.active &&
                            cat == cat_main &&
                            (col.category_second != null && cats.Contains(cat_second)) :

                            (counting && cat != "alle") ?
                            col.active &&
                            cat == cat_main :

                            false))
                            ;

            bool relevantD = is_pro ? (pro.booth != null && pro.booth.region != null ? (zip != 0 ? pro.booth.region.zip == zip : true) &&
                AreasHelper.IsRelevant(pro.booth.region.zip) : true) :
                (col.booth != null && col.booth.region != null ? (zip != 0 ? col.booth.region.zip == zip : true) &&
                AreasHelper.IsRelevant(col.booth.region.zip) : true);

            bool relevantF = CheckParam(pro, col, false, helper);

            bool relevant = relevantA && relevantB && relevantC && relevantD && relevantF;
            if(dto != null)
            {
                if (is_pro && relevant)
                    dto.relevant_hits.Add(new Hit() { booth = b_name, product = pro.name });
                if (!is_pro && relevant)
                    dto.relevant_hits.Add(new Hit() { booth = b_name, product = col.name });
                dto.relevant = relevant;
            }
            return relevant;
        }

        public bool IsRelevant<T>(T dto, product pro, collection col, string b_name, bool is_param, RelevantHelper helper) where T : dto_booth_item
        {
            if (pro.IsNull() && col.IsNull())
                throw new Exception("A-OK, Check.");

            if (!pro.IsNull() && !col.IsNull())
                throw new Exception("A-OK, Check.");

            if (b_name.IsNull())
                throw new Exception("A-OK, Check.");

            if (helper.IsNull())
                throw new Exception("A-OK, Check.");

            bool is_pro = !pro.IsNull();

            if(ThisSession.Params.Count == 0 && !is_param)
                return true;

            bool relevant = CheckParam(pro, col, is_param, helper);
            if(dto != null)
            {
                if (is_pro && relevant)
                    dto.relevant_hits.Add(new Hit() { booth = b_name, product = pro.name });
                if (!is_pro && relevant)
                    dto.relevant_hits.Add(new Hit() { booth = b_name, product = col.name });
                dto.relevant = relevant;
            }
            return relevant;
        }

        public bool CheckParam(product pro, collection col, bool is_param, RelevantHelper helper)
        {
            if (pro.IsNull() && col.IsNull())
                throw new Exception("A-OK, Check.");

            if (!pro.IsNull() && !col.IsNull())
                throw new Exception("A-OK, Check.");

            if (helper.IsNull())
                throw new Exception("A-OK, Check.");

            bool is_pro = !pro.IsNull();


            string[] opt, cats;
            string op1, op2, op3, op4, op5, op6, cat;
            int fra, til, zip;
            //RelevantHelper helper = RelevantHelper.Create(false);
            helper.GetVals(out opt, out op1, out op2, out op3, out op4, out op5, out op6, out cats, out cat, out fra, out til, out zip);

            /*
             * HACK - just a precaution
             * */
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                if (is_pro && (pro.product_param == null || !pro.product_param.Any()))
                    pro.product_param = _db.product_param.Where(x => x.product_id == pro.Id).ToList();

                if (!is_pro && (col.collection_param == null || !col.collection_param.Any()))
                    col.collection_param = _db.collection_param.Where(x => x.collection_id == col.Id).ToList();

                bool relevantF = false;

                if (ThisSession.Params.Count == 0)
                    relevantF = is_param ? false : true;

                List<param> l1 = null;
                List<value> l2 = null;

                /*
                 * denne del af algoritmen kører de valgte params og values igennem
                 * da der kan være valgt mange values + produktet kan have mange values, er der tale om en mange til mange søgning
                 * M: mange values
                 * MS: mange values, kun en kan vælges
                 * S: ingen values, kun param
                 * */

                //her tjekkes for params der passer overens med en text fra søgeboxen
                if (!relevantF || is_param)
                {
                    l1 = is_pro ? (pro.product_param.Select(x => x.param).ToList()) : (col.collection_param.Select(x => x.param).ToList());
                    l2 = is_pro ? (pro.product_param.Select(x => x.value).ToList()) : (col.collection_param.Select(x => x.value).ToList());

                    foreach (string s in opt)
                    {
                        if (s == "")
                            continue;
                        if (Params(s, "S", l1, l2) || Params(s, "MS", l1, l2) || Params(s, "M", l1, l2))
                            relevantF = true;
                    }
                }

                //her tjekkes for params der kommer fra param chooseren
                if (!relevantF)
                {
                    foreach (biz_params pa in ThisSession.Params)
                    {
                        if (pa.type == "MS" || pa.type == "M")
                        {
                            foreach (biz_value val in pa.values_daos)
                            {
                                if (Params(val.value, pa.type, l1, l2))
                                    relevantF = true;
                            }
                        }
                        else
                        {
                            if (Params(pa.name, pa.type, l1, l2))
                                relevantF = true;
                        }
                    }
                }

                return relevantF;
            }
        }

        private bool Params(string chooser, string type, List<param> l1, List<value> l2)
        {
            if (string.IsNullOrEmpty(chooser))
                throw new Exception("A-OK, Check.");
            
            if (string.IsNullOrEmpty(type))
                throw new Exception("A-OK, Check.");

            if (l1.IsNull())
                throw new Exception("A-OK, Check.");
            
            if (l2.IsNull())
                throw new Exception("A-OK, Check.");

            /*
             * denne del af algoritmen kører det give products params og values igennem
             * returnerer true hvis der er match
             * */
            if (type == "S")
            {
                foreach (param pa in l1)
                {
                    if (pa.type == "S" && pa.name == chooser)
                        return true;
                }
            }
            else if (type == "MS" || type == "M")
            {
                foreach (param pa in l1)
                {
                    foreach (value val in l2)
                    {
                        if (val.IsNull())
                            continue;
                        if (val.param == pa && val.value1 == chooser)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}