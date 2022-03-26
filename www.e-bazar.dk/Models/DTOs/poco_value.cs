using System;
using System.Collections.Generic;
using System.Linq;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_value
    {
        public int Id { get; set; }
        public string value { get; set; }
        public int prio { get; set; }
        public int? params_id { get; set; }

        public virtual poco_params params_dao { get; set; }

        public List<poco_value> ToDAO_List(List<value> vals)
        {
            if (vals == null)
                throw new Exception("A-OK, Check.");

            List<poco_value> list = new List<poco_value>();
            foreach (value v in vals)
            {
                list.Add(new poco_value() { Id = v.Id, value = v.value1, prio = v.prio, params_id = v.param_id });
            }
            return list.OrderBy(x => x.prio).ToList();
        }
    }
}
