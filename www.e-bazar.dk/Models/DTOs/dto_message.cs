using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public class dto_message
    {
        public string message { get; set; }
        //public bool owner { get; set; }
        public long id { get; set; }
        public TYPE type { get; set; }
        public long? conversation_id { get; set; }
        public poco_conversation conversation { get; set; }
        public poco_salesman product_owner { get; set; }
        public poco_person other { get; set; }
        public string conn_owner_id { get; set; }
        public string product_owner_id { get; set; }
        public string other_id { get; set; }
        public string product_owner_email { get; set; }
        public string other_email { get; set; }
        public string product_owner_firstname { get; set; }
        public string other_firstname { get; set; }
    }
}