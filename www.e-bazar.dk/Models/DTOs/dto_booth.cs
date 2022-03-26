using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_booth
    {
        public dto_booth()
        {
            
        }
        public dto_booth(poco_booth booth_poco, dto_folders folders, List<poco_category> cats, List<poco_booth> other_booths, List<poco_booth> chance, List<poco_booth> booth_pocos, bool is_owner/*, poco_person current_user*/, int rating/*, string s, string c, int z, int f, int t, bool g*/)
        {
            //this.salesman_poco = s_poco;
            this.current_user = current_user;
            this.booth_poco = booth_poco;
            this.folders = folders;
            this.cats = cats;
            this.chance = chance;
            this.other_booths = other_booths;
            this.booth_pocos = booth_pocos;
            this.is_advanced = folders.flda_pocos.Count > 0;
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
        public poco_booth booth_poco { get; set; }
        public dto_folders folders { get; set; }
        public List<poco_category> cats { get; set; }
        public List<poco_booth> chance { get; set; }
        public List<poco_booth> other_booths { get; set; }
        public List<poco_booth> booth_pocos { get; set; }
        //public poco_salesman salesman_poco { get; set; }
        public poco_person current_user { get; set; }
        public bool is_advanced { get; set; }
        public bool is_owner { get; set; }
        public int rating { get; set; }
    }
}