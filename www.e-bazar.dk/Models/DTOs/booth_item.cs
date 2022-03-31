using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.SharedClasses;
using static www.e_bazar.dk.Models.DTOs.poco_booth;

namespace www.e_bazar.dk.Models.DTOs
{
    public abstract class booth_item : IBoothItem
    {
        public long id { get; set; }
        public int category_second_id { get; set; }
        public int category_main_id { get; set; }
        public SelectList category_main_selectlist { get; set; }
        public SelectList category_second_selectlist { get; set; }

        public poco_folder foldera { get; set; }
        public SelectList foldera_selectlist { get; set; }
        public poco_folder folderb { get; set; }
        public SelectList folderb_selectlist { get; set; }

        [DisplayName("Parametre")]
        public List<poco_params> param_daos { get; set; }
        public DateTime modified { get; set; }
        [Required]
        [StringLength(50)]
        [DisplayName("Vare")]
        public string name { get; set; }
        [Required]
        [StringLength(100)]
        public string sysname { get; set; }
        [DisplayName("Oprettet")]
        public DateTime created_on { get; set; }
        [Required]
        [StringLength(20)]
        [DisplayName("Pris")]
        public string price { get; set; }
        public string status_stock { get; set; }
        [DisplayName("Lagerbeholdning")]
        public SelectList status_stock_selectlist { get; set; }
        [StringLength(20)]
        public string status_condition { get; set; }
        [DisplayName("Stand")]
        public SelectList status_condition_selectlist { get; set; }

        [StringLength(500)]
        [DisplayName("Beskrivelse")]
        public string description { get; set; }
        public string description_limit
        {
            get
            {
                if (string.IsNullOrEmpty(description))
                    return "";
                int len = description.Length < 230 ? description.Length : 230;
                return len < 230 ? description : (description.Substring(0, len) + "...");
            }
            set
            {
                description = value;
            }
        }
        [StringLength(50)]
        [DisplayName("Note")]
        public string note { get; set; }
        public bool active { get; set; }

        public int? booth_id { get; set; }
        public poco_booth booth_poco { get; set; }
        [DisplayName("Billeder")]
        public List<IImage> image_pocos { get; set; }
        [DisplayName("Søgeord")]
        public List<poco_tag> tag_pocos { get; set; }
        [DisplayName("Søgeord")]
        public string tag_pocos_nop { get; set; }
        public List<poco_conversation> conversations { get; set; }

        public abstract long Save();
        public abstract void Update();
        public abstract void Delete(long id, EbazarDB db);
        public abstract bool RemoveTag(long tag_id, bool is_up_dating);
        public abstract void RemoveImage(string image_name);
        public abstract poco_booth GetBoothPOCO();

        public bool relevant { get; set; }
        public List<Hit> relevant_hits = new List<Hit>();

        public string cat_main = "";
        public string cat_second = "";



        public void SetupToClient<T>() where T : booth_item
        {

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {


                if (!this.IsNull() && !this.booth_poco.IsNull())
                {
                    if (!this.category_main_id.IsNull())
                    {
                        poco_category cat_poco = new poco_category();
                        List<poco_category> cats_top = cat_poco._GetAll(true, true);

                        this.category_main_selectlist = new SelectList(cats_top, "category_id", "name", this.category_main_id);

                        this.category_second_selectlist = this.category_second_id == 0 ?
                            new SelectList(cats_top.OrderBy(x => x.priority).FirstOrDefault().children, "category_id", "name", this.category_second_id) :
                            new SelectList(cats_top.Where(x => x.category_id == this.category_main_id).FirstOrDefault().children, "category_id", "name", this.category_second_id);
                    }

                    poco_folder tmpa = this.foldera;
                    poco_folder tmpb = this.folderb;

                    List<folder> lista = new List<folder>() { new folder() { Id = -1, name = "ingen.." } };
                    lista = lista.Concat(_db.folder.Where(l => l.booth_id == this.booth_poco.booth_id).OrderBy(l => l.priority)).ToList();

                    List<folder> listb = new List<folder>() { new folder() { Id = -1, name = "ingen.." } };
                    listb = !tmpa.IsNull() ? listb.Concat(_db.folder.Where(l => l.parent_id == tmpa.id).OrderBy(l => l.priority)).ToList() : listb;

                    this.foldera_selectlist = !tmpa.IsNull() && lista.Count() > 0 ?
                        new SelectList(lista, "Id", "name", tmpa.id) :
                        new SelectList(lista, "Id", "name", -1);

                    this.folderb_selectlist = !tmpa.IsNull() && !tmpb.IsNull() && listb.Count() > 0 ?
                        new SelectList(listb, "Id", "name", tmpb.id) :
                        new SelectList(listb, "Id", "name", -1);
                }

                this.status_stock = string.IsNullOrEmpty(this.status_stock) ? STOCK.PÅ_LAGER.ToString() : this.status_stock;
                this.status_stock_selectlist = EnumHelper.SelectListFor(Texts.GetStockEnum(this.status_stock));

                this.status_condition = string.IsNullOrEmpty(this.status_condition) ? CONDITION.VELHOLDT.ToString() : this.status_condition;
                this.status_condition_selectlist = EnumHelper.SelectListFor(Texts.GetConditionEnum(this.status_condition));

                this.price = string.IsNullOrEmpty(this.price) ? NOP.INGEN_PRIS.ToString() : this.price;
                this.note = string.IsNullOrEmpty(this.note) ? Texts.GetNopValue(NOP.NO_NOTE.ToString()) : this.note;
                this.description = string.IsNullOrEmpty(this.description) ? Texts.GetNopValue(NOP.NO_DESCRIPTION.ToString()) : this.description;

                if (this.tag_pocos == null || this.tag_pocos.Count == 0)
                {
                    this.tag_pocos = null;
                    this.tag_pocos_nop = Texts.GetNopValue(NOP.NO_TAGS.ToString());
                }
            }
        }



        public bool IsRelevant(product pro, collection col, bool counting, string b_name, RelevantHelper helper)
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

            cat_main = is_pro ? pro.category_main.name : col.category_main.name;
            cat_second = is_pro ? (pro.category_second != null ? pro.category_second.name : null) : (col.category_second != null ? col.category_second.name : null);

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
                            ((opt.Where(x => x != "" && pro.name.ToLower().Trim().Contains(x))).Count() > 0 && pro.active ||
                            (pro.tag != null && opt.Where(x => pro.tag.Where(t => t.name == x).Count() > 0).Count() > 0 && pro.active) ||
                            (opt.Where(x => x != "" && desc.Contains(x)).Count() > 0 && pro.active))) :
                            ((opt[0] == "") ?
                            true :
                            ((opt.Where(x => x != "" && col.name.ToLower().Trim().Contains(x))).Count() > 0 && col.active ||
                            (col.tag != null && opt.Where(x => col.tag.Where(t => t.name == x).Count() > 0).Count() > 0 && col.active) ||
                            (opt.Where(x => x != "" && desc.Contains(x)).Count() > 0 && col.active)));

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
                Areas.IsRelevant(pro.booth.region.zip) : true) :
                (col.booth != null && col.booth.region != null ? (zip != 0 ? col.booth.region.zip == zip : true) &&
                Areas.IsRelevant(col.booth.region.zip) : true);

            bool relevantF = CheckParam(pro, col, false, helper);

            this.relevant = relevantA && relevantB && relevantC && relevantD && relevantF;
            if (is_pro && this.relevant)
                this.relevant_hits.Add(new Hit() { booth = b_name, product = pro.name });
            if (!is_pro && this.relevant)
                this.relevant_hits.Add(new Hit() { booth = b_name, product = col.name });
            return this.relevant;
        }

        public bool IsRelevant(product pro, collection col, string b_name, bool is_param, RelevantHelper helper)
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
            if (is_pro && relevant)
                this.relevant_hits.Add(new Hit() { booth = b_name, product = pro.name });
            if (!is_pro && relevant)
                this.relevant_hits.Add(new Hit() { booth = b_name, product = col.name });
            this.relevant = relevant;
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
                    foreach (poco_params pa in ThisSession.Params)
                    {
                        if (pa.type == "MS" || pa.type == "M")
                        {
                            foreach (poco_value val in pa.values_daos)
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