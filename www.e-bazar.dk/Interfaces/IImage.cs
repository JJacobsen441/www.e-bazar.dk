using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DTOs;

namespace www.e_bazar.dk.Interfaces
{
    public interface IImage
    {
        //EbazarDB db = new EbazarDB();

        long id { get; set; }

        string name { get; set; }

        DateTime created_on { get; set; }

        long? _id { get; set; }

        List<IImage> GetImagePOCOs(long collection_id);
        void SaveImages(long collection_id, List<string> fnames);
        void DeleteImages(long collection_id);
        
    }
}
