using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using www.e_bazar.dk.Models.DataAccess;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_story
    {
        //private EbazarDB db = new EbazarDB();
        public int Id { get; set; }

        [StringLength(50)]
        public string header1 { get; set; }

        [StringLength(150)]
        public string header2 { get; set; }

        [StringLength(500)]
        public string story { get; set; }

        /*private poco_story()
        {

        }*/
        public poco_story()
        {
            //this.db = new EbazarDB();
        }
        public List<poco_story> GetStoryPOCOs()
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            List<poco_story> stories = (from s in _db.storie
                                        select new poco_story
                                        {
                                            Id = s.Id,
                                            header1 = s.header1,
                                            header2 = s.header2,
                                            story = s.story
                                        }).ToList();
            if (stories != null)
                return stories;
            else
                throw new Exception("A-OK, handled.");
        }
    }
}