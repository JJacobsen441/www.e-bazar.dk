namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.storie")]
    public partial class storie
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [StringLength(50)]
        public string header1 { get; set; }

        [StringLength(150)]
        public string header2 { get; set; }

        [StringLength(500)]
        public string story { get; set; }
    }
}
