namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.conversation")]
    public partial class conversation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public conversation()
        {
            comment = new HashSet<comment>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public DateTime created_on { get; set; }

        public long? product_id { get; set; }

        [StringLength(128)]
        public string person_id { get; set; }

        public long? collection_id { get; set; }

        public int? booth_id { get; set; }

        public DateTime modified { get; set; }

        public virtual booth booth { get; set; }

        public virtual collection collection { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<comment> comment { get; set; }

        public virtual person person { get; set; }

        public virtual product product { get; set; }
    }
}
