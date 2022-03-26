using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.e_bazar.dk.Interfaces;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_conversations
    {
        public List<poco_conversation> booths { get; set; }
        public List<poco_conversation> items { get; set; }

        public List<poco_conversation> own { get; set; }
        public bool is_salesman { get; set; }
        public dto_conversations(List<poco_conversation> own, List<poco_conversation> booths, List<poco_conversation> items, bool is_salesman)
        {
            this.own = own;
            this.booths = booths;
            this.items = items;
            this.is_salesman = is_salesman;
        }
    }
}