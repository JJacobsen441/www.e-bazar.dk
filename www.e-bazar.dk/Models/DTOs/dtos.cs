using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using static www.e_bazar.dk.Models.DTOs.dto_booth;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_booth
    {
        public int booth_id { get; set; }

        [DisplayName("Stand Navn")]
        [Required]
        [StringLength(50)]
        public string name { get; set; }

        [Required]
        [StringLength(100)]
        public string sysname { get; set; }

        [DisplayName("Oprettet")]
        public DateTime created_on { get; set; }

        [DisplayName("Oprettet")]
        public DateTime modified { get; set; }

        [DisplayName("Stand Beskrivelse")]
        [StringLength(500)]
        public string description { get; set; }

        [DisplayName("Stand Beskrivelse")]
        public string description_limit
        {
            get
            {
                if (string.IsNullOrEmpty(description))
                    return "";
                int len = description.Length < 230 ? description.Length : 230;

                return len < 230 ? description : (description.Substring(0, len) + "...");
            }
            set
            {
                description = value;
            }
        }

        [DisplayName("Stand Logo")]
        [StringLength(80)]
        public string frontimage { get; set; }

        [DisplayName("Antal vurderinger")]
        public int? numberofratings { get; set; }

        [DisplayName("Stand vurdering")]
        public double? boothrating { get; set; }

        [DisplayName("Stand vurdering")]
        public string boothrating_nop { get; set; }

        [StringLength(128)]
        public string salesman_id { get; set; }


        [Required]
        [DisplayName("Gade navn")]
        [StringLength(50)]
        public string street_address { get; set; }

        [Required]
        [DisplayName("Land")]
        [StringLength(20)]
        public string country { get; set; }

        public bool fulladdress { get; set; }

        public string fulladdress_str { get; set; }

        public int region_id { get; set; }
        
        public bool relevant { get; set; }

        public dto_salesman salesman_dto { get; set; }

        public List<dto_product> product_dtos { get; set; }

        public List<dto_collection> collection_dtos { get; set; }

        public virtual dto_region region_dto { get; set; }

        public virtual List<dto_conversation> conversation_dtos { get; set; }

        public virtual List<dto_folder> foldera_dtos { get; set; }

        public virtual List<dto_category> category_main { get; set; }
        
        public int hits_items { get; set; }

        public class Hit { public string booth; public string product; }

        public List<dto_booth_item> items = new List<dto_booth_item>();

    }

    public class dto_booth_item
    {
        public long id { get; set; }
        public int category_second_id { get; set; }
        public int category_main_id { get; set; }
        public SelectList category_main_selectlist { get; set; }
        public SelectList category_second_selectlist { get; set; }

        public dto_folder foldera { get; set; }
        public SelectList foldera_selectlist { get; set; }
        public dto_folder folderb { get; set; }
        public SelectList folderb_selectlist { get; set; }

        [DisplayName("Parametre")]
        public List<dto_params> param_dtos { get; set; }

        public DateTime modified { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Vare")]
        public string name { get; set; }

        [Required]
        [StringLength(100)]
        public string sysname { get; set; }

        [DisplayName("Oprettet")]
        public DateTime created_on { get; set; }

        [Required]
        [StringLength(20)]
        [DisplayName("Pris")]
        public string price { get; set; }

        public string status_stock { get; set; }

        [DisplayName("Lagerbeholdning")]
        public SelectList status_stock_selectlist { get; set; }

        [StringLength(20)]
        public string status_condition { get; set; }

        [DisplayName("Stand")]
        public SelectList status_condition_selectlist { get; set; }

        [StringLength(50)]
        [DisplayName("Note")]
        public string note { get; set; }

        [StringLength(500)]
        [DisplayName("Beskrivelse")]
        public string description { get; set; }

        public string description_limit
        {
            get
            {
                if (string.IsNullOrEmpty(description))
                    return "";
                int len = description.Length < 230 ? description.Length : 230;
                return len < 230 ? description : (description.Substring(0, len) + "...");
            }
            set
            {
                description = value;
            }
        }

        public bool active { get; set; }

        public int? booth_id { get; set; }

        public dto_booth booth_dto { get; set; }

        [DisplayName("Billeder")]
        public List<dto_image> image_dtos { get; set; }

        [DisplayName("Søgeord")]
        public List<dto_tag> tag_dtos { get; set; }

        [DisplayName("Søgeord")]
        public string tag_dtos_nop { get; set; }

        public List<dto_conversation> conversations { get; set; }

        public bool relevant { get; set; }

        public List<Hit> relevant_hits = new List<Hit>();
    }

    public class dto_product : dto_booth_item
    {

        [DisplayName("Antal enheder")]
        public int no_of_units { get; set; }

        public bool only_collection { get; set; }

        public long? collection_id { get; set; }

        public virtual dto_collection collection { get; set; }
    }

    public class dto_collection : dto_booth_item
    {
        public virtual List<dto_product> product_dtos { get; set; }
    }

    public abstract class dto_person
    {
        public string person_id { get; set; }

        public bool iscreated = true;

        [DisplayName("Fornavn")]
        public string firstname { get; set; }

        [DisplayName("Efternavn")]
        public string lastname { get; set; }

        [DisplayName("Oprettet")]
        public DateTime created_on { get; set; }

        [DisplayName("Kontakt email")]
        public string email { get; set; }
        public bool request_email { get; set; }

        [StringLength(128)]
        public string sysname { get; set; }

        [DisplayName("Profil Billed")]
        public string profileimage { get; set; }

        public string nator { get; set; }

        public virtual List<dto_product> favorites_product { get; set; }
        public virtual List<dto_collection> favorites_collection { get; set; }
        public virtual List<dto_booth> following { get; set; }
        public virtual List<boothrating> boothrating { get; set; }
    }

    public class dto_customer : dto_person
    {

    }

    public class dto_salesman : dto_person
    {
        [DisplayName("Tlf. nr.")]
        public int? phonenumber { get; set; }

        public bool show_phone { get; set; }

        [DisplayName("Tlf. Nr.")]
        public string phonenumber_nop { get; set; }

        [DisplayName("Beskrivelse")]
        public virtual string description { get; set; }

        public virtual List<dto_booth> booth_dtos { get; set; }
    }

    public class dto_image// : IImage
    {
        public long id { get; set; }

        public string name { get; set; }

        public DateTime created_on { get; set; }

        public long? _id { get; set; }
    }

    public class dto_comment
    {
        public DateTime created_on { get; set; }
        public string text { get; set; }
        public float? bid { get; set; }
        public string type { get; set; }
        public long? conversation_id { get; set; }
        public string person_id { get; set; }
        public bool product_viewed_owner { get; set; }
        public bool viewed_other { get; set; }

        public virtual dto_conversation conversation_dto { get; set; }
        public dto_salesman dto_salesman { get; set; }
        public dto_customer dto_customer { get; set; }
    }

    public class dto_conversation
    {
        public long? conversation_id { get; set; } = null;
        public DateTime created_on { get; set; }
        public DateTime modified { get; set; }

        public long? product_id { get; set; }
        public long? collection_id { get; set; }
        public int? booth_id { get; set; }
        public string person_id { get; set; }

        public virtual List<dto_comment> comment_dtos { get; set; }
        public virtual dto_product product_dto { get; set; }
        public virtual dto_collection collection_dto { get; set; }
        public virtual dto_booth booth_dto { get; set; }
    }

    public class dto_params
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public int prio { get; set; }
        public int? category_id { get; set; }

        public dto_category category_dao { get; set; }
        public List<dto_value> values_daos { get; set; }
    }

    public class dto_value
    {
        public int Id { get; set; }
        public string value { get; set; }
        public int prio { get; set; }
        public int? params_id { get; set; }

        public virtual dto_params params_dao { get; set; }
    }

    public class dto_category
    {
        public int category_id { get; set; }
        [Required]
        [StringLength(50)]
        public string name { get; set; }
        public bool is_parent { get; set; }
        public int priority { get; set; }
        [StringLength(100)]
        public string description { get; set; }
        public int? parent_id { get; set; }
        public int booths_with_category_count { get; set; }
        public virtual List<dto_category> children { get; set; }
        public virtual dto_category parent { get; set; }
        public virtual List<dto_booth> booth { get; set; }
        public virtual List<dto_params> params_dto { get; set; }
    }

    public class dto_folder
    {
        public long id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string name { get; set; }
        public int? priority { get; set; }
        public int? booth_id { get; set; }
        public int? parent_id { get; set; }
        public bool is_parent { get; set; }
        public int count { get; set; }
        public virtual dto_booth booth { get; set; }
        public virtual List<dto_folder> children { get; set; }
    }

    public class dto_region
    {
        public int Id { get; set; }

        [DisplayName("PostNr.")]
        public int zip { get; set; }

        [DisplayName("By")]
        [StringLength(20)]
        public string town { get; set; }
    }

    public class dto_tag
    {
        public long tag_id { get; set; }
        public string name { get; set; }
        public string form { get; set; }
        public int numberofhits { get; set; }
        
        public List<dto_product> product { get; set; }
        public List<dto_collection> collection { get; set; }
    }
}