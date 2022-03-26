namespace www.e_bazar.dk.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("public.AspNetUserClaims")]
    public partial class AspNetUserClaims
    {
        public int Id { get; set; }

        [StringLength(256)]
        public string ClaimType { get; set; }

        [StringLength(256)]
        public string ClaimValue { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        public virtual AspNetUsers AspNetUsers { get; set; }
    }
}
