using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_folders
    {
        public List<poco_folder> flda_pocos { get; set; }
        public string selected { get; set; }
        public dto_folders(List<poco_folder> flda_pocos, string selected)
        {
            this.flda_pocos = flda_pocos;
            this.selected = selected;
        }
        /*public string Catelog_Str
        {
            get
            {
                string res = "";
                foreach (string s in catelog)
                    res += s;
                return res;
            }
        }*/
    }
}