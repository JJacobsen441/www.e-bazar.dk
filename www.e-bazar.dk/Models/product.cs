namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.product")]
    public partial class product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public product()
        {
            conversation = new HashSet<conversation>();
            image = new HashSet<image>();
            product_param = new HashSet<product_param>();
            sale = new HashSet<sale>();
            favorites = new HashSet<person>();
            tag = new HashSet<tag>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string name { get; set; }

        [Required]
        [StringLength(100)]
        public string sysname { get; set; }

        public DateTime created_on { get; set; }

        [Required]
        [StringLength(20)]
        public string price { get; set; }

        [StringLength(20)]
        public string status_delivery { get; set; }

        [StringLength(20)]
        public string status_stock { get; set; }

        [StringLength(20)]
        public string status_condition { get; set; }

        [StringLength(500)]
        public string description { get; set; }

        [StringLength(50)]
        public string note { get; set; }

        public int? booth_id { get; set; }

        public long? collection_id { get; set; }

        public bool only_collection { get; set; }

        public int no_of_units { get; set; }

        public long? folder_a_id { get; set; }

        public long? folder_b_id { get; set; }

        public bool active { get; set; }

        public int category_second_id { get; set; }

        public int category_main_id { get; set; }

        public DateTime modified { get; set; }

        public virtual booth booth { get; set; }

        public virtual category category_main { get; set; }

        public virtual category category_second { get; set; }

        public virtual collection collection { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<conversation> conversation { get; set; }

        public virtual folder foldera { get; set; }

        public virtual folder folderb { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<image> image { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<product_param> product_param { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<sale> sale { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<person> favorites { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tag> tag { get; set; }
    }
}
