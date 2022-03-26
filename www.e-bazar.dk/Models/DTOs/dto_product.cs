using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.e_bazar.dk.Interfaces;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_product
    {
        public dto_product()
        {

        }
        public dto_product(poco_product product_poco, List<IBoothItem> other/*, poco_person current_user*/)
        {
            //this.salesman_poco = s_poco;
            //this.current_user = current_user;
            this.product_poco = product_poco;
            this.is_owner = false;
            this.other = other;
        }

        public dto_product(poco_product product_poco, List<IBoothItem> other/*, poco_person current_user*/, bool is_owner)
        {
            //this.salesman_poco = s_poco;
            //this.current_user = current_user;
            this.product_poco = product_poco;
            this.is_owner = is_owner;
            this.other = other;
        }

        public poco_product product_poco { get; set; }
        public List<IBoothItem> other { get; set; }
        //public poco_person current_user { get; set; }
        public bool is_owner { get; set; }
    }
}