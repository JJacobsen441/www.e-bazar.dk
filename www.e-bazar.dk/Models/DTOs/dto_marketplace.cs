using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_categories
    {
        public string isset_top { get; set; }
        public string isset_bottom { get; set; }
        public List<poco_category> cats { get; set; }
    }
    public class dto_marketplace
    {
        private dto_marketplace()
        {
            ;
        }
        public dto_marketplace(List<poco_booth> booth_list, int number_booths, int number_booths_page, List<poco_booth> booth_newest/*, poco_person current_user*/, List<string> area_selected, List<poco_category> cats, string c_search, int z, int f, int t, bool g, Stats stats, bool fromsearch=false)
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

            poco_category alle = new poco_category() { name = "alle", children = new List<poco_category>() };
            List<poco_category> cats_list = new List<poco_category>();
            cats_list.Add(alle);
            foreach (poco_category cat in cats)
                cats_list.Add(cat);

            string[] set = c_search.Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa").Replace(" ", "_").Split('-').Where(s => s != "").ToArray();

            string isset_top = "";
            if(set.Count() > 0)
                isset_top = c_search.Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa").Replace(" ", "_").Split('-').Where(s => s != "").ToArray()[0];
            string isset_bottom = "";
            if(set.Count() > 1)
                isset_bottom = c_search.Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa").Replace(" ", "_").Split('-').Where(s => s != "").ToArray()[1];

            this.categories = new dto_categories() { isset_top = isset_top, isset_bottom = isset_bottom, cats = cats_list };
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

        public List<poco_booth> booths_list { get; set; }
        public List<poco_booth> booths_newest { get; set; }
        public Stats stats { get; set; }
        
        public string s { get; set; }
        public string c { get; set; }
        public string z { get; set; }
        public List<string> area_selected { get; set; }
        public string area_checked { get; set; }
        public string f { get; set; }
        public string t { get; set; }
        public bool g { get; set; }
        public dto_categories categories { get; set; }
        public bool fromsearch { get; set; }
        public int? number_booths { get; set; }
        public int number_booths_page { get; set; }
        public int users_per_month { get; set; }
        public int booths_count { get; set; }
        public int items_count { get; set; }

    }
}