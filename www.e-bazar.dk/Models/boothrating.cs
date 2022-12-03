namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.boothrating")]
    public partial class boothrating
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public short? rating { get; set; }

        public int? booth_id { get; set; }

        [Required]
        [StringLength(128)]
        public string user_id { get; set; }

        public virtual booth booth { get; set; }

        public virtual person person { get; set; }
    }
}
