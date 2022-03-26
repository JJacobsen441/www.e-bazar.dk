using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_region
    {
        public int Id { get; set; }

        [DisplayName("PostNr.")]
        public int zip { get; set; }

        [DisplayName("By")]
        [StringLength(20)]
        public string town { get; set; }

        public void ToPOCO(region r)
        {
            if (r == null)
                throw new Exception("A-OK, Check");

            this.zip = r.zip;
            this.town = r.town;
        }

    }
}