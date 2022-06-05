using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_region
    {
        public int Id { get; set; }

        [DisplayName("PostNr.")]
        public int zip { get; set; }

        [DisplayName("By")]
        [StringLength(20)]
        public string town { get; set; }

        public dto_region ToDTO(region r)
        {
            if (r == null)
                throw new Exception("A-OK, Check");

            dto_region dto = new dto_region();
            dto.zip = r.zip;
            dto.town = r.town;

            return dto;
        }
    }
}