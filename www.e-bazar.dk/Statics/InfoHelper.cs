using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace www.e_bazar.dk.Statics
{
    public class pageinfo
    {
        public string date;
        public string type;
        public string text = "";
    }

    public class InfoHelper
    {
        public static List<pageinfo> GetInfo(int count) 
        {
            List<pageinfo> infos = new List<pageinfo>();
            
            string path = StaticsHelper.Root + "App_Info\\Info.xml";
            
            var xdoc = XElement.Load(path);
            foreach (XElement elem in xdoc.Elements())
            {
                infos.Add(new pageinfo()
                {
                    date = elem.Attribute("date").Value,
                    type = elem.Attribute("type").Value,
                    text = elem.Value
                });
            }
            if (count == -1)
                return infos;
            return infos.Take(count).ToList();
        }
    }
}