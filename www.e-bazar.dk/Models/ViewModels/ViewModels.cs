using System.Collections.Generic;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.ViewModels
{
    public class ViewModels
    {
        //[HttpPost]
        public class AdminPostViewModel 
        {
            public string pwd;
            public string cmd;
            public string value1;
            public string bool1;
            public string bool2;
        }

        public class MarketplaceViewModel
        {
            public List<string> area_selected;
            public string a { get; set; } = "";
            public string s { get; set; } = "";
            public string c { get; set; } = "";
            public string p { get; set; } = "";
            public string z { get; set; } = "0";
            public string f { get; set; } = "0";
            public string t { get; set; } = "999999";
            public string gra { get; set; } = "true";
            public int page { get; set; } = 1;
        }

        //[HttpPost]
        public class GetCatsViewModel
        {
            public string ok { get; set; }
        }

        public class BoothViewModel
        {
            public int id { get; set; }
            public string a_sub { get; set; } = "";
            public string b_sub { get; set; } = "";
        }

        public class SelectCatelogViewModel
        {
            public int id { get; set; }
            public string a_sub { get; set; }
            public string b_sub { get; set; }
            public string catelog { get; set; }
        }

        public class ProductViewModel
        {
            public long id { get; set; }
        }

        public class CollectionViewModel
        {
            public int id { get; set; }
        }

        //[HttpGet]
        public class MessageAViewModel
        {
            public long id { get; set; }
            public string type { get; set; }
        }

        //[HttpGet]
        public class MessageBViewModel
        {
            public long id { get; set; }
            public string owner { get; set; }
            public string type { get; set; }
        }

        //[HttpPost]
        public class MessageViewModel
        {
            public string message { get; set; }
            public long id { get; set; }
            public TYPE type { get; set; }
            public long? conversation_id { get; set; }
            public string conn_owner_id { get; set; }
            public string other_email { get; set; }
            public string other_firstname { get; set; }
        }

        public class AddFavoriteViewModel
        {
            public long product_id { get; set; }
            public int collection_id { get; set; }
        }

        public class RemoveFavoriteViewModel
        {
            public long product_id { get; set; }
            public int collection_id { get; set; }
        }

        public class AddFollowingViewModel
        {
            public int booth_id { get; set; }
        }

        public class RemoveFollowingViewModel
        {
            public int booth_id { get; set; }
        }

        //[HttpPost]
        public class AddRatingViewModel
        {
            public string booth_id { get; set; }
            public string person_id { get; set; }
            public string rating { get; set; }
        }

        public class NotFoundViewModel
        {
            public TYPE type { get; set; }
            public long id { get; set; } = -1;
            public string a { get; set; } = "";
            public string b { get; set; } = "";
        }        
    }
}