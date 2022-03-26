using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.e_bazar.dk.Interfaces;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_userprofile
    {
        public dto_userprofile()
        {
            
        }
        public dto_userprofile(/*poco_administrator a_poco, */poco_salesman s_poco, poco_customer c_poco)
        {
            //this.administrator_poco = a_poco;
            this.salesman_poco = s_poco;
            this.customer_poco = c_poco;
            //this.booth_pocos = booth_pocos;
        }

        //public poco_administrator administrator_poco { get; set; }
        public poco_salesman salesman_poco { get; set; }
        public poco_customer customer_poco { get; set; }
        public List<poco_salesman> salesmen { get; set; }
        public List<poco_customer> customers { get; set; }
        public List<poco_booth> booth_pocos { get; set; }
        public List<IBoothItem> follower_news { get; set; }
        public dto_conversations conversations_dto { get; set; }
        //public customer_dto customer_dto { get; set; }
        //public string user_type { get; set; }
    }
}