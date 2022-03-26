namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.collection_param")]
    public partial class collection_param
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public int? value_id { get; set; }

        public long? collection_id { get; set; }

        public int? param_id { get; set; }

        public virtual collection collection { get; set; }

        public virtual param param { get; set; }

        public virtual value value { get; set; }
    }
}
