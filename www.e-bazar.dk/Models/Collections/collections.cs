using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class col_userprofile
    {
        public col_userprofile()
        {
        }

        public col_userprofile(dto_salesman s_dto, dto_customer c_dto)
        {
            this.salesman_dto = s_dto;
            this.customer_dto = c_dto;           
        }

        public dto_salesman salesman_dto { get; set; }
        public dto_customer customer_dto { get; set; }
        public List<dto_salesman> salesmen { get; set; }
        public List<dto_customer> customers { get; set; }
        public List<dto_booth> booth_dtos { get; set; }
        public List<dto_booth_item> follower_news { get; set; }
        public col_conversations conversations_dto { get; set; }        
    }

    public class col_categories
    {
        public string isset_top { get; set; }
        public string isset_bottom { get; set; }
        public List<dto_category> cats { get; set; }
    }

    public class col_marketplace
    {
        private col_marketplace()
        {
        }

        public col_marketplace(List<dto_booth> booth_list, int number_booths, int number_booths_page, List<dto_booth> booth_newest, List<string> area_selected, List<dto_category> cats, string c_search, int z, int f, int t, bool g, Stats stats, bool fromsearch = false)
        {
            this.booths_list = booth_list;
            this.booths_newest = booth_newest;
            this.number_booths = number_booths;
            this.number_booths_page = number_booths_page;

            this.fromsearch = fromsearch;

            this.z = "" + z;
            this.area_selected = area_selected;
            this.f = "" + f;
            this.t = "" + t;
            this.g = g;
            this.c = c_search;
            this.stats = stats;

            dto_category alle = new dto_category() { name = "alle", children = new List<dto_category>() };
            List<dto_category> cats_list = new List<dto_category>();
            cats_list.Add(alle);
            foreach (dto_category cat in cats)
                cats_list.Add(cat);

            string[] set = c_search.Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa").Replace(" ", "_").Split('-').Where(s => s != "").ToArray();

            string isset_top = "";
            if (set.Count() > 0)
                isset_top = c_search.Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa").Replace(" ", "_").Split('-').Where(s => s != "").ToArray()[0];
            string isset_bottom = "";
            if (set.Count() > 1)
                isset_bottom = c_search.Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa").Replace(" ", "_").Split('-').Where(s => s != "").ToArray()[1];

            this.categories = new col_categories() { isset_top = isset_top, isset_bottom = isset_bottom, cats = cats_list };
        }

        private string ListToString(List<string> list, char split)
        {
            string s = "";
            if (list != null)
            {
                foreach (string str in list)
                    s += str + split;
            }
            return s;
        }

        public List<dto_booth> booths_list { get; set; }
        public List<dto_booth> booths_newest { get; set; }
        public Stats stats { get; set; }

        public string s { get; set; }
        public string c { get; set; }
        public string z { get; set; }
        public List<string> area_selected { get; set; }
        public string area_checked { get; set; }
        public string f { get; set; }
        public string t { get; set; }
        public bool g { get; set; }
        public col_categories categories { get; set; }
        public bool fromsearch { get; set; }
        public int? number_booths { get; set; }
        public int number_booths_page { get; set; }
        public int users_per_month { get; set; }
        public int booths_count { get; set; }
        public int items_count { get; set; }

    }

    public class col_booth
    {
        public col_booth()
        {

        }
        public col_booth(dto_booth booth_dto, col_folders folders, List<dto_category> cats, List<dto_booth> other_booths, List<dto_booth> chance, List<dto_booth> booth_dtos, bool is_owner/*, biz_person current_user*/, int rating/*, string s, string c, int z, int f, int t, bool g*/)
        {
            //this.salesman_poco = s_poco;
            this.current_user = current_user;
            this.booth_dto = booth_dto;
            this.folders = folders;
            this.cats = cats;
            this.chance = chance;
            this.other_booths = other_booths;
            this.booth_dtos = booth_dtos;
            this.is_advanced = folders.flda_dtos.Count > 0;
            this.is_owner = is_owner;
            this.rating = rating;
            /*this.s = s;
            this.c = c;
            this.z = z;
            this.t = t;
            this.f = f;
            this.g = g;*/
        }
        public string s { get; set; }
        public string c { get; set; }
        public int z { get; set; }
        public int f { get; set; }
        public int t { get; set; }
        public bool g { get; set; }
        public dto_booth booth_dto { get; set; }
        public col_folders folders { get; set; }
        public List<dto_category> cats { get; set; }
        public List<dto_booth> chance { get; set; }
        public List<dto_booth> other_booths { get; set; }
        public List<dto_booth> booth_dtos { get; set; }
        //public biz_salesman salesman_poco { get; set; }
        public dto_person current_user { get; set; }
        public bool is_advanced { get; set; }
        public bool is_owner { get; set; }
        public int rating { get; set; }
    }

    public class col_product
    {
        public col_product()
        {
        }

        public col_product(dto_product product_dto, List<dto_booth_item> other)
        {
            this.product_dto = product_dto;
            this.is_owner = false;
            this.other = other;
        }

        public col_product(dto_product product_dto, List<dto_booth_item> other, bool is_owner)
        {
            this.product_dto = product_dto;
            this.is_owner = is_owner;
            this.other = other;
        }

        public dto_product product_dto { get; set; }
        public List<dto_booth_item> other { get; set; }
        public bool is_owner { get; set; }
    }

    public class col_collection
    {
        public col_collection()
        {
        }
        
        public col_collection(dto_collection collection_dto, List<dto_booth_item> other)
        {
            this.collection_dto = collection_dto;
            this.is_owner = false;
            this.other = other;
        }
        
        public col_collection(dto_collection collection_dto, List<dto_booth_item> other, bool is_owner)
        {
            this.collection_dto = collection_dto;
            this.is_owner = is_owner;
            this.other = other;
        }
        
        public dto_collection collection_dto { get; set; }
        public List<dto_booth_item> other { get; set; }
        public bool is_owner { get; set; }
    }

    public class col_conversations
    {
        public List<dto_conversation> booths { get; set; }
        public List<dto_conversation> items { get; set; }
        public List<dto_conversation> own { get; set; }
        public bool is_salesman { get; set; }

        public col_conversations(List<dto_conversation> own, List<dto_conversation> booths, List<dto_conversation> items, bool is_salesman)
        {
            this.own = own;
            this.booths = booths;
            this.items = items;
            this.is_salesman = is_salesman;
        }
    }

    public class col_email
    {
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class col_folders
    {
        public List<dto_folder> flda_dtos { get; set; }
        public string selected { get; set; }
        public col_folders(List<dto_folder> flda_dtos, string selected)
        {
            this.flda_dtos = flda_dtos;
            this.selected = selected;
        }
    }

    public class col_message
    {
        public string message { get; set; }
        public long id { get; set; }
        public TYPE type { get; set; }
        public long? conversation_id { get; set; }
        public dto_conversation conversation { get; set; }
        public dto_salesman product_owner { get; set; }
        public dto_person other { get; set; }
        public string conn_owner_id { get; set; }
        public string product_owner_id { get; set; }
        public string other_id { get; set; }
        public string product_owner_email { get; set; }
        public string other_email { get; set; }
        public string product_owner_firstname { get; set; }
        public string other_firstname { get; set; }
    }
}