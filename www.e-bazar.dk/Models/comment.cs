namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.comment")]
    public partial class comment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public DateTime created_on { get; set; }

        [StringLength(250)]
        public string text { get; set; }

        public float? bid { get; set; }

        [StringLength(15)]
        public string type { get; set; }

        public long? conversation_id { get; set; }

        [StringLength(128)]
        public string person_id { get; set; }

        public bool viewed_owner { get; set; }

        public bool viewed_other { get; set; }

        public virtual conversation conversation { get; set; }

        public virtual person person { get; set; }
    }
}
