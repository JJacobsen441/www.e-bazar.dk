using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Statics
{
    public struct Area
    {
        public string area { get; set; }
        public int from { get; set; }
        public int to { get; set; }
        public int alt_from { get; set; }
        public int alt_to { get; set; }
    }
    public class AreasHelper
    {
        private static bool is_setup = false;
        private static bool is_running = false;

        private static bool OK()
        {
            if (!is_setup && !is_running)
            {
                is_running = true;
                Setup();
                is_setup = true;
                is_running = false;
            }
            return is_setup && !is_running;
        }

        public static List<string> selected {
            get {
                    if (!OK())
                        return new List<string> { "dk" };
                    if (!ThisSession.Cookie)
                        return new List<string> { "dk" };
                    if (ThisSession.Area == null)
                    {
                        List<string> selected = new List<string>();
                        selected.Add("dk");
                        return selected;
                    }
                    else
                    {
                        return ThisSession.Area; 
                    }
                }

            set {
                OK();
                if (!ThisSession.Cookie)
                    return;
                if (ThisSession.Area == null)
                {
                    ThisSession.Area = new List<string>();
                }
                if(value == null || value.Count() == 0 || value[0] == "")
                {
                    List<string> tmp = new List<string>();
                    tmp.Add("dk");
                    ThisSession.Area = tmp;
                }
                else
                    ThisSession.Area = value; } 
        }
        public static List<Area> areas = new List<Area>();

        public AreasHelper()
        {
        }
        private static void Setup()
        {
            if (!is_setup)
            {
                areas = new List<Area>();
                areas.Add(new Area { area = "dk", from = 100, to = 9999 });
                areas.Add(new Area { area = "Storkøbenhavn", from = 1001, to = 2999 });
                areas.Add(new Area { area = "Nord Sjælland", from = 3000, to = 3699 });
                areas.Add(new Area { area = "Region Sjælland", from = 4000, to = 4999 });
                areas.Add(new Area { area = "Fyn", from = 5000, to = 5999 });
                areas.Add(new Area { area = "Syd Jylland", from = 6000, to = 6999 });
                areas.Add(new Area { area = "Midt Jylland", from = 7000, to = 8999 });
                areas.Add(new Area { area = "Nord Jylland", from = 9000, to = 9999 });
                areas.Add(new Area { area = "Bornholm", from = 3700, to = 3799 });
                areas.Add(new Area { area = "Færørene", from = 3800, to = 3899, alt_from = 100, alt_to = 999 });
                areas.Add(new Area { area = "Grønland", from = 3900, to = 3999 });

                is_setup = true;
            }
        }
        public static bool IsArea(string a) 
        {
            if (!OK())
                return false;
            foreach (Area ar in GetAreas())
            {
                if (ar.area.Replace(" ", "").ToLower() == a)
                    return true;
            }
            return false;
        }
        public static List<Area> GetAreas()
        {
            if (!OK())
                return new List<Area> { new Area { area = "dk", from = 100, to = 9999 } };
            Setup();
            return areas;
        }
        public static bool IsRelevant(int zip)
        {
            if (!OK())
                return false;
            bool ok = false;
            if (selected.Count() == 0)
                ok = true;
            else
            {
                foreach (string s in selected)
                {
                    Area area = areas.Where(a => a.area.Replace(" ", "").ToLower() == s).FirstOrDefault();
                    if (zip <= area.to && zip >= area.from)
                        ok = true;
                }
            }
            return ok;
        }
    }

}