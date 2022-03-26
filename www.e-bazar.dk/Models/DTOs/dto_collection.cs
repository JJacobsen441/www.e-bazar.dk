using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.e_bazar.dk.Interfaces;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_collection
    {
        public dto_collection()
        {
        }
        public dto_collection(poco_collection collection_poco, List<IBoothItem> other)
        {
            //this.salesman_poco = s_poco;
            //this.current_user = current_user;
            this.collection_poco = collection_poco;
            this.is_owner = false;
            this.other = other;
        }
        public dto_collection(poco_collection collection_poco, List<IBoothItem> other, bool is_owner)
        {
            //this.salesman_poco = s_poco;
            //this.current_user = current_user;
            this.collection_poco = collection_poco;
            this.is_owner = is_owner;
            this.other = other;
        }
        public poco_collection collection_poco { get; set; }
        public List<IBoothItem> other { get; set; }
        //public poco_person current_user { get; set; }
        public bool is_owner { get; set; }
    }
}