using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Statics
{
    public class IP_Logs
    {
        //public static List<string> IPs = new List<string>();
        public static bool Logged(string ip)
        {
            var xdoc = XElement.Load(StaticsHelper.Root + "App_Stat\\IP_LOG.xml");
            var ips = xdoc.Elements("ips");

            foreach (XElement elem in ips.Descendants())
            {
                if (elem.Attribute("val").Value == ip)
                    return true;
            }
            return false;
        }

        public static void Add(string ip)
        {
            var xdoc = XElement.Load(StaticsHelper.Root + "App_Stat\\IP_LOG.xml");
            var ips = xdoc.Element("ips");
            XElement e = new XElement("ip");
            e.Add(new XAttribute("val", ip));
            //e.Add(new XAttribute("cats", cats));
            //e.Add(new XAttribute("date", date.Ticks));
            ips.Add(e);

            xdoc.Save(StaticsHelper.Root + "App_Stat\\IP_LOG.xml");

        }

        public static void Reset()
        {
            var xdoc = XElement.Load(StaticsHelper.Root + "App_Stat\\IP_LOG.xml");
            var ips = xdoc.Elements("ips");

            foreach (XElement elem in ips.Descendants().ToList())
                elem.Remove();


            xdoc.Save(StaticsHelper.Root + "App_Stat\\IP_LOG.xml");
        }

        public static List<Log> Get(string ip)
        {
            List<Log> logs = new List<Log>();
            var xdoc = XElement.Load(StaticsHelper.Root + "App_Stat\\IP_LOG.xml");
            var ips = xdoc.Elements("ips");

            foreach (XElement elem in ips.Descendants().ToList())
            {
                if(elem.Attribute("val").Value == ip)
                {
                    if (elem.Attribute("cats") != null && elem.Attribute("date") != null)
                        logs.Add(new Log() { ip = elem.Attribute("val").Value, cats = elem.Attribute("cats").Value, date = new DateTime(long.Parse(elem.Attribute("date").Value)) });
                }
            }

            return logs;
        }

        public static int CountDistinct()
        {
            var xdoc = XElement.Load(StaticsHelper.Root + "App_Stat\\IP_LOG.xml");
            var ips = xdoc.Element("ips");
            return ips.Descendants().GroupBy(x => x.Attribute("val").Value).Select(x => x.First()).Count();
        }

        public class Log
        {
            public string ip { get; set; }
            public string cats { get; set; }
            public DateTime date { get; set; }
        }
    }
    /*public class IP_Stats
    {
        public static List<Elems> list = new List<Elems>();
        public static bool Logged(Elems elem) 
        {
            return list.Where(e => e.Equals(elem)).Count() > 0;
        }
        public class Elems 
        {
            public string ip { get; set; }
            public string cats { get; set; }
            public DateTime date { get; set; }
            /*public static bool operator == (Elems a, Elems b)
            {
                return a.ip == b.ip;
            }
            public static bool operator != (Elems a, Elems b)
            {
                return a.ip != b.ip;
            }
            public bool Equals(Elems b)
            {
                return this.ip == b.ip;
            }
        }
    }*/

    public class Stats
    {
        public bool ok { get; set; }
        public string ip { get; set; }
        public bool first { get; set; }
        public int users_per_day { get; set; }
        public int users_per_month { get; set; }
        public int max_users_per_month { get; set; }
        public int booths_count { get; set; }
        public int items_count { get; set; }
    }

    public class StatisticsHelper
    {
        public StatisticsHelper() 
        {
        }
        
        private static void SetUserPerMonth(Stats stat, string path, int count)
        {
            var xdoc = XElement.Load(path);
            int res = 0;
            var users_per_month = xdoc.Element("userspermonth");
            DateTime now = DateTime.Now;
            int old_count = int.Parse(users_per_month.Attribute("count").Value);
            string old_date = users_per_month.Attribute("date").Value;
            if (now.ToString("MM-yyyy") == old_date)
            {
                int new_count = old_count + count;
                users_per_month.Attribute("count").SetValue(new_count);
                res = new_count;
            }
            else
            {
                users_per_month.Attribute("count").SetValue(0);
                users_per_month.Attribute("date").SetValue(now.ToString("MM-yyyy"));
            }
            xdoc.Save(path);
            stat.users_per_month = res;
        }
        
        private static void SetUsersPerDay(string ip, string path, out bool first, out int users_per_day, out int max_users_per_month)
        {
            DateTime now = DateTime.Now;

            var xdoc = XElement.Load(path);
            var elem_per_day = xdoc.Element("usersperday");
            var elem_per_month = xdoc.Element("maxuserspermonth");
            //int old_count = int.Parse(elem.Attribute("count").Value);
            string old_date = elem_per_day.Attribute("date").Value;
            first = false;
            if (now.ToString("dd-MM-yyyy") == old_date)
            {
                elem_per_day.Attribute("date").SetValue(now.ToString("dd-MM-yyyy"));

                if (!IP_Logs.Logged(ip))
                    first = true;
                IP_Logs.Add(ip);
                int count = IP_Logs.CountDistinct();
                
                users_per_day = count;
            }
            else
            {
                elem_per_day.Attribute("date").SetValue(now.ToString("dd-MM-yyyy"));

                first = true;

                IP_Logs.Reset();
                IP_Logs.Add(ip);
                int count = IP_Logs.CountDistinct();
                                
                users_per_day = count;
            }

            string _d = elem_per_month.Attribute("date").Value.ToString();
            DateTime d = DateTime.ParseExact(_d, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            if (d.Month != now.Month)
                elem_per_month.Attribute("count").SetValue(0);

            if (int.Parse(elem_per_month.Attribute("count").Value.ToString()) < users_per_day)
            {
                elem_per_month.Attribute("count").SetValue(users_per_day);
                elem_per_month.Attribute("date").SetValue(now.ToString("dd-MM-yyyy"));
            }
            max_users_per_month = int.Parse(elem_per_month.Attribute("count").Value.ToString());

            xdoc.Save(path);
        }

        public static Stats GetStatistics()
        {
            Stats stats = new Stats();

            HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
            string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);

            string path = StaticsHelper.Root + "App_Stat\\Statistics.xml";
            
            if (CheckHelper.Generel.IsAdmin(ip))
                SetUserPerMonth(stats, path, 0);
            else
                SetUserPerMonth(stats, path, 1);

            bool first;
            //bool ok;
            int users_per_day;
            int max_users_per_month;
            if (CheckHelper.Generel.IsAdmin(ip))
                SetUsersPerDay(ip, path, out first, out users_per_day, out max_users_per_month);
            else
                SetUsersPerDay(ip, path, out first, out users_per_day, out max_users_per_month);

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {

                //this.stats.ok = ok;
                stats.ip = ip;
                stats.first = first;
                stats.users_per_day = users_per_day;
                stats.max_users_per_month = max_users_per_month;
                stats.booths_count = _db.booth.Where(b => b.product.Where(p => p.active).Count() > 0 || b.collection.Where(co => co.active).Count() > 0).Count();
                stats.items_count = _db.product.Where(p => p.active).Count() + _db.collection.Where(co => co.active).Count();

                return stats;
            }
        }
    }
}