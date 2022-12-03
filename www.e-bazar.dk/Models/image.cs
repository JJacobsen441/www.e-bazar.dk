namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.image")]
    public partial class image
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string name { get; set; }

        public DateTime created_on { get; set; }

        public long? product_id { get; set; }

        public long? collection_id { get; set; }

        public bool? is_product { get; set; }

        public virtual collection collection { get; set; }

        public virtual product product { get; set; }
    }
}
