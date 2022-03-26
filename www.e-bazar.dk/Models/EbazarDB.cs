namespace www.e_bazar.dk.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class EbazarDB : DbContext
    {
        public EbazarDB()
            : base("name=EbazarDB")
        {
        }

        public virtual DbSet<C__EFMigrationsHistory> C__EFMigrationsHistory { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<booth> booth { get; set; }
        public virtual DbSet<boothrating> boothrating { get; set; }
        public virtual DbSet<category> category { get; set; }
        public virtual DbSet<collection> collection { get; set; }
        public virtual DbSet<collection_param> collection_param { get; set; }
        public virtual DbSet<comment> comment { get; set; }
        public virtual DbSet<conversation> conversation { get; set; }
        public virtual DbSet<folder> folder { get; set; }
        public virtual DbSet<image> image { get; set; }
        public virtual DbSet<param> param { get; set; }
        public virtual DbSet<person> person { get; set; }
        public virtual DbSet<product> product { get; set; }
        public virtual DbSet<product_param> product_param { get; set; }
        public virtual DbSet<region> region { get; set; }
        public virtual DbSet<sale> sale { get; set; }
        public virtual DbSet<storie> storie { get; set; }
        public virtual DbSet<tag> tag { get; set; }
        public virtual DbSet<value> value { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.Entity<booth>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<product>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<image>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<conversation>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<comment>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<collection>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<region>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<tag>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);                
            modelBuilder.Entity<folder>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<boothrating>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<category>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<param>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<value>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<product_param>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<collection_param>().Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            


            modelBuilder.Entity<AspNetRoles>()
                .HasMany(e => e.AspNetUsers)
                .WithMany(e => e.AspNetRoles)
                .Map(m => m.ToTable("AspNetUserRoles", "public").MapLeftKey("RoleId").MapRightKey("UserId"));

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.AspNetUserClaims)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.AspNetUserLogins)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<booth>()
                .HasMany(e => e.conversation)
                .WithOptional(e => e.booth)
                .HasForeignKey(e => e.booth_id);

            modelBuilder.Entity<booth>()
                .HasMany(e => e.foldera)
                .WithOptional(e => e.booth)
                .HasForeignKey(e => e.booth_id);

            modelBuilder.Entity<booth>()
                .HasMany(e => e.collection)
                .WithOptional(e => e.booth)
                .HasForeignKey(e => e.booth_id);

            modelBuilder.Entity<booth>()
                .HasMany(e => e.product)
                .WithOptional(e => e.booth)
                .HasForeignKey(e => e.booth_id);

            modelBuilder.Entity<booth>()
                .HasMany(e => e.boothrating)
                .WithOptional(e => e.booth)
                .HasForeignKey(e => e.booth_id);

            modelBuilder.Entity<booth>()
                .HasMany(e => e.category_main)
                .WithMany(e => e.booth)
                .Map(m => m.ToTable("boothcategory", "public").MapLeftKey("booth_id").MapRightKey("category_id"));

            modelBuilder.Entity<category>()
                .HasMany(e => e.collection_main)
                .WithRequired(e => e.category_main)
                .HasForeignKey(e => e.category_main_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<category>()
                .HasMany(e => e.collection_second)
                .WithRequired(e => e.category_second)
                .HasForeignKey(e => e.category_second_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<category>()
                .HasMany(e => e.param)
                .WithOptional(e => e.category)
                .HasForeignKey(e => e.category_id);

            modelBuilder.Entity<category>()
                .HasMany(e => e.children)
                .WithOptional(e => e.parent)
                .HasForeignKey(e => e.parent_id);

            modelBuilder.Entity<category>()
                .HasMany(e => e.product_main)
                .WithRequired(e => e.category_main)
                .HasForeignKey(e => e.category_main_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<category>()
                .HasMany(e => e.product_second)
                .WithRequired(e => e.category_second)
                .HasForeignKey(e => e.category_second_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<collection>()
                .HasMany(e => e.collection_param)
                .WithOptional(e => e.collection)
                .HasForeignKey(e => e.collection_id);

            modelBuilder.Entity<collection>()
                .HasMany(e => e.conversation)
                .WithOptional(e => e.collection)
                .HasForeignKey(e => e.collection_id);

            modelBuilder.Entity<collection>()
                .HasMany(e => e.image)
                .WithOptional(e => e.collection)
                .HasForeignKey(e => e.collection_id);

            modelBuilder.Entity<collection>()
                .HasMany(e => e.product)
                .WithOptional(e => e.collection)
                .HasForeignKey(e => e.collection_id);

            modelBuilder.Entity<collection>()
                .HasMany(e => e.favorites)
                .WithMany(e => e.favorites_collection)
                .Map(m => m.ToTable("favorites_collections", "public").MapLeftKey("collection_id").MapRightKey("person_id"));

            modelBuilder.Entity<conversation>()
                .HasMany(e => e.comment)
                .WithOptional(e => e.conversation)
                .HasForeignKey(e => e.conversation_id);

            modelBuilder.Entity<folder>()
                .HasMany(e => e.collection)
                .WithOptional(e => e.foldera)
                .HasForeignKey(e => e.folder_a_id);

            modelBuilder.Entity<folder>()
                .HasMany(e => e.collection1)
                .WithOptional(e => e.folderb)
                .HasForeignKey(e => e.folder_b_id);

            modelBuilder.Entity<folder>()
                .HasMany(e => e.children)
                .WithOptional(e => e.parent)
                .HasForeignKey(e => e.parent_id);

            modelBuilder.Entity<folder>()
                .HasMany(e => e.product)
                .WithOptional(e => e.foldera)
                .HasForeignKey(e => e.folder_a_id);

            modelBuilder.Entity<folder>()
                .HasMany(e => e.product1)
                .WithOptional(e => e.folderb)
                .HasForeignKey(e => e.folder_b_id);

            modelBuilder.Entity<param>()
                .HasMany(e => e.collection_param)
                .WithOptional(e => e.param)
                .HasForeignKey(e => e.param_id);

            modelBuilder.Entity<param>()
                .HasMany(e => e.product_param)
                .WithOptional(e => e.param)
                .HasForeignKey(e => e.param_id);

            modelBuilder.Entity<param>()
                .HasMany(e => e.value1)
                .WithOptional(e => e.param)
                .HasForeignKey(e => e.param_id);

            modelBuilder.Entity<person>()
                .HasMany(e => e.booth)
                .WithOptional(e => e.person)
                .HasForeignKey(e => e.person_id);

            modelBuilder.Entity<person>()
                .HasMany(e => e.boothrating)
                .WithRequired(e => e.person)
                .HasForeignKey(e => e.user_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<person>()
                .HasMany(e => e.comment)
                .WithOptional(e => e.person)
                .HasForeignKey(e => e.person_id);

            modelBuilder.Entity<person>()
                .HasMany(e => e.conversation)
                .WithOptional(e => e.person)
                .HasForeignKey(e => e.person_id);

            modelBuilder.Entity<person>()
                .HasMany(e => e.sale)
                .WithOptional(e => e.person)
                .HasForeignKey(e => e.person_id);

            modelBuilder.Entity<person>()
                .HasMany(e => e.favorites_product)
                .WithMany(e => e.favorites)
                .Map(m => m.ToTable("favorites_product", "public").MapLeftKey("person_id").MapRightKey("product_id"));

            modelBuilder.Entity<person>()
                .HasMany(e => e.following)
                .WithMany(e => e.followers)
                .Map(m => m.ToTable("following", "public").MapLeftKey("person_id").MapRightKey("booth_id"));

            modelBuilder.Entity<product>()
                .HasMany(e => e.conversation)
                .WithOptional(e => e.product)
                .HasForeignKey(e => e.product_id);

            modelBuilder.Entity<product>()
                .HasMany(e => e.image)
                .WithOptional(e => e.product)
                .HasForeignKey(e => e.product_id);

            modelBuilder.Entity<product>()
                .HasMany(e => e.product_param)
                .WithOptional(e => e.product)
                .HasForeignKey(e => e.product_id);

            modelBuilder.Entity<product>()
                .HasMany(e => e.sale)
                .WithOptional(e => e.product)
                .HasForeignKey(e => e.product_id);

            modelBuilder.Entity<product>()
                .HasMany(e => e.tag)
                .WithMany(e => e.product)
                .Map(m => m.ToTable("producttag", "public").MapLeftKey("product_id").MapRightKey("tag_id"));

            modelBuilder.Entity<region>()
                .HasMany(e => e.booth)
                .WithRequired(e => e.region)
                .HasForeignKey(e => e.region_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tag>()
                .HasMany(e => e.collection)
                .WithMany(e => e.tag)
                .Map(m => m.ToTable("collectiontag", "public").MapLeftKey("tag_id").MapRightKey("collection_id"));

            modelBuilder.Entity<value>()
                .HasMany(e => e.collection_param)
                .WithOptional(e => e.value)
                .HasForeignKey(e => e.value_id)
                .WillCascadeOnDelete();

            modelBuilder.Entity<value>()
                .HasMany(e => e.product_param)
                .WithOptional(e => e.value)
                .HasForeignKey(e => e.value_id)
                .WillCascadeOnDelete();
        }
    }
}
