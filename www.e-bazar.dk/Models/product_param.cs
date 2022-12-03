namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.product_param")]
    public partial class product_param
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public int? value_id { get; set; }

        public long? product_id { get; set; }

        public int? param_id { get; set; }

        public virtual param param { get; set; }

        public virtual product product { get; set; }

        public virtual value value { get; set; }
    }
}
