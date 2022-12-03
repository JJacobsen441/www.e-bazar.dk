namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.sale")]
    public partial class sale
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public DateTime created_on { get; set; }

        public long? product_id { get; set; }

        [StringLength(128)]
        public string person_id { get; set; }

        public virtual person person { get; set; }

        public virtual product product { get; set; }
    }
}
