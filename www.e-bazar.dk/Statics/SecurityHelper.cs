using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Statics
{
    public class SecurityHelper
    {
        //private static EbazarDB db = new EbazarDB();
        private static int groups = int.Parse(SettingsHelper.Security.GROUPS());
        private static List<dto_category> special_ae;
        private static List<dto_category> special_æ;


        private static dto_category GetTop(string top)
        {
            using (EbazarDB db = new EbazarDB())
            {
                List<dto_category> cats = CategorysHelper.CatsYesYes;

                if (cats.IsNullOrEmpty() || string.IsNullOrEmpty(top))
                    throw new Exception("A-OK, Check.");
                foreach (dto_category c in cats)
                {
                    if (SecurityHelper.Encode(SecurityHelper.Format(c.name, "_", true)) == top)
                        return c;
                }
            }
            throw new Exception("A-OK, Check.");
        }

        public static string Format(string str, string replace, bool tolower)
        {
            string s = str.Replace("ø", "oe").Replace("æ", "ae").Replace("å", "aa").Replace(" ", replace);
            return tolower ? s.ToLower() : s;
        }
        
        public static string OETOrig(string areas, char split)
        {
            List<Area> special_ae = AreasHelper.GetAreas().Where(c => c.area.Contains("ae")).ToList();
            List<Area> special_æ = AreasHelper.GetAreas().Where(c => c.area.Contains("æ") || c.area.Contains("ø") || c.area.Contains("å")).ToList();
            List<string> request_areas = areas.Replace("_", " ").Split(split).Where(c => c != "").ToList();
            string res = "";
            if (request_areas.Count() > 0)
            {
                foreach (string area in request_areas)
                {
                    if (special_ae.Where(c => c.area == area).Count() > 0)
                        res += area + split;
                    else if (special_æ.Where(c => c.area.Replace(" ", "").ToLower() == area.Replace("ae", "æ").Replace("oe", "ø").Replace("aa", "å")).Count() > 0)
                        res += area.Replace("ae", "æ").Replace("oe", "ø").Replace("aa", "å") + split;
                    else
                        res += area + split;
                }
            }
            else
                res = areas;
            return res.Replace(" ", "").ToLower();
        }

        public static string ListToString(List<string> list, char split)
        {
            string s = "";
            if (list != null)
            {
                foreach (string str in list)
                    s += str + split;
            }
            return s;
        }

        public static List<string> StringReplaceList(List<string> list, char split)
        {
            List<string> res = new List<string>();
            foreach (string s in list)
                res.Add(OETOrig(s, split).Replace(" ", "").Replace("" + split, "").ToLower());
            return res;
        }
        
        public static string GenerateHashSHA(string name)
        {
            SHA256 al = new SHA256Managed();
            byte[] shaDigest = al.ComputeHash(ASCIIEncoding.ASCII.GetBytes(name));
            return BitConverter.ToString(shaDigest);
        }

        public static string GenerateHashMD5_A(string name, int len)
        {
            MD5 al = MD5.Create();
            byte[] digest = al.ComputeHash(Encoding.UTF8.GetBytes(name));
            byte[] res = new byte[len];
            for (int i = 0; i < len; i++)
                res[i] = digest[i];
            return BitConverter.ToString(res, 0);
        }

        public static string GenerateHashMD5_B(string hash)
        {
            HashAlgorithm al = MD5.Create();
            byte[] res = al.ComputeHash(Encoding.UTF8.GetBytes(hash));
            return BitConverter.ToString(res, 0);
        }

        private static string Encode(string text)
        {
            return GenerateHashMD5_A(text, 2).Replace("-", "");
        }
        
        private static List<dto_category> Dec_FromBitsToList(dto_category top, string bits, int index)
        {
            List<dto_category> children = CategorysHelper.CatsYesYes.Where(c => c.parent != null && c.parent.name == top.name).OrderBy(c => c.priority).ToList();

            List<dto_category> res_list = new List<dto_category>();
            res_list.Add(top);
            if (bits != "")
            {
                string[] bits_arr = bits.Split(':').ToArray();
                for (int i = 0; i < 4; i += 1)
                {
                    if (bits_arr[i] == "1")
                    {
                        if ((index * 4 + i) < (children.Count()))
                            res_list.Add(children.ElementAt(index * 4 + i));
                    }
                }
            }
            return res_list;
        }

        private static List<dto_category> Dec_FromHexToList(string top, char hex, int index)
        {
            List<dto_category> all = CategorysHelper.CatsYesYes;
            List<dto_category> children = all.Where(c => SecurityHelper.Format(c.name, "_", true) == SecurityHelper.Format(top, "_", true)).FirstOrDefault().children;
            children = children.OrderBy(c => c.priority).ToList();
            //count = 0;

            List<dto_category> res_list = new List<dto_category>();
            if ((int)hex != 0)
            {
                for (int i = 0; i < 4; i += 1)
                {
                    if (((byte)((byte)hex << i) & (byte)0x08) == (byte)0x08)
                    {
                        if ((index * 4 + i) < (children.Count()))
                        {
                            res_list.Add(children.ElementAt(index * 4 + i));
                            //count++;
                        }
                    }
                }
            }
            return res_list;
        }
        
        private static byte conv(string s)
        {
            byte res = 0x00;
            switch (s)
            {
                case "0:0:0:1":
                    res = 0x01;
                    break;
                case "0:0:1:0":
                    res = 0x02;
                    break;
                case "0:1:0:0":
                    res = 0x04;
                    break;
                case "1:0:0:0":
                    res = 0x08;
                    break;
            }
            return res;
        }

        private static string Dec_FromBitArrayToChar(string hash)
        {
            //string c_arr = "";
            int count = int.Parse(SettingsHelper.Security.MD5_COUNT());
            byte b = 0x00;
            if (hash.Count() == count)
            {
                for (int i = 0; i < 16; i++)
                {
                    string str_b = "" + (byte)b;
                    string str_h = "" + (byte)conv(hash);
                    if (str_b == str_h)
                        return "" + b;
                    b = (byte)(b + 0x01);
                }
            }
            return "none";
        }

        private static string Dec_FromHashToChar(string hash)
        {
            byte b = 0x00;
            if (hash.Count() == 4)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (SecurityHelper.Encode("" + (char)b) == hash)
                        return "" + b;
                    b = (byte)(b + 0x01);
                }
            }
            return "none";
        }
        
        private static char Enc_ToHexFromString(string top, int group_curr, string route, EbazarDB db)
        {
            string[] isset = SecurityHelper.Format(route, "_", true).Split('-').Where(s => s != "").OrderBy(r => r.Substring(0, 1)).ToArray();
            top = SecurityHelper.Format(top, "_", false);
            dto_category cat = CategorysHelper.CatsYesNo.ToList().Where(c => SecurityHelper.Format(c.name, "_", true) == top.ToLower()).FirstOrDefault();
            if(cat != null)
            {
                List<dto_category> children = cat.children.OrderBy(c => c.priority).ToList();
                byte res = 0x00;
                for (int i = group_curr * 4; i < group_curr * 4 + 4; i++)
                {
                    if (i < children.Count() && isset.Contains(SecurityHelper.Format(children.ElementAt(i).name, "_", true)))
                        res = (byte)(res | 0x01);
                    if (i < group_curr * 4 + 3)
                        res = (byte)(res << 1);
                }
                return (char)res;
            }
            throw new Exception("A-OK, handled");
        }

        private static string Dec_FromArrayToString(List<dto_category> list)
        {
            string res = "";
            foreach (dto_category cat in list)
                res += cat.name + "-";
            return res;
        }
        
        private static bool IsSecond(string[] cats) 
        {
            for (int i = 1; i < cats.Length; i++)
                if (cats[i] != "93B8")
                    return true;
            return false;
        }
        
        public static string DecodeCats_MD5(string c, bool to_ae)
        {
            using (EbazarDB db = new EbazarDB())
            {

                string res = "";
                string[] cats_arr = c.Split('-');
                cats_arr = cats_arr.Where(s => s != "").ToArray();
                res = cats_arr[0] != "alle" ? GetTop(cats_arr[0]).name + "-" : cats_arr[0];
                string save = res.Split('-').ToArray()[0];
                if (res == "alle")
                    return res.Replace("_", " ");
                
                if (!IsSecond(cats_arr))
                    return res.Replace("_", " ");

                for (int i = 1; i < cats_arr.Count(); i++)
                {
                    string s = Dec_FromHashToChar(cats_arr[i]);
                    if (s == "none")
                        res += s;
                    else
                    {
                        char ch = (char)int.Parse(s);
                        res += Dec_FromArrayToString(Dec_FromHexToList(GetTop(cats_arr[0]).name, ch, i - 1)/*, out count*//*, to_ae*/);
                    }
                }
                //string[] source = res.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                // Create the query.  Use ToLowerInvariant to match "data" and "Data"
                //  var matchQuery = from word in source
                //                     where word.ToLowerInvariant() == save.ToLowerInvariant()
                //                     select word;
                // Count the matches, which executes the query.  
                //int wordCount = matchQuery.Count();
                //if (wordCount > 1 && cats_arr.Count() == 1)
                //    res = res.Substring(save.Count() + 1, save.Count() + 1);
                
                //return FromOEToOrig(res, '-').Replace("_", " ");
                return res.Replace("_", " ");
            }
        }
        
        public static string EncodeCats_MD5(bool is_top, string name1, string route)
        {
            using (EbazarDB db = new EbazarDB())
            {
                //StaticsHelper.Log("groups > " + groups);
                string res = "";
                for (int i = 0; i < groups; i++)
                {
                    char c = SecurityHelper.Enc_ToHexFromString(name1, i, route, db);
                    string s = c == (char)0x00 ? "" + (char)0x00 : "" + c;
                    res += SecurityHelper.Encode(s) + (i < (groups - 1) ? "-" : "");
                }

                if(is_top)
                    return SecurityHelper.Encode(SecurityHelper.Format(name1, "_", true)) + "-93B8-93B8";
                else
                    return SecurityHelper.Encode(SecurityHelper.Format(name1, "_", true)) + "-" + res;
            }
        }

        public static string EncodeCats_MD5(bool is_top, string route)
        {
            using (EbazarDB db = new EbazarDB())
            {

                if (route == "alle")
                    return route;
                string[] tmp = route.Split('-').Where(s => s != "").ToArray();
                string name1 = tmp[0];
                //string[] tmp_isset = new string[tmp.Count() - 1];// route.Split('-').Where(s => s != "").ToArray();
                //for (int i = 1; i < tmp.Count(); i++)
                //    tmp_isset[i - 1] = tmp[i];
                route = route.Replace(name1 + "-", "");

                string res = "";
                for (int i = 0; i < groups; i++)
                {
                    char c = SecurityHelper.Enc_ToHexFromString(name1, i, route, db);
                    string s = c == (char)0x00 ? "" + (char)0x00 : "" + c;
                    res += SecurityHelper.Encode(s) + (i < (groups - 1) ? "-" : "");
                }

                //return name1 + "-" + res;
                if (is_top)
                    return SecurityHelper.Encode(SecurityHelper.Format(name1, "_", true)) + "-93B8-93B8";
                else
                    return SecurityHelper.Encode(SecurityHelper.Format(name1, "_", true)) + "-" + res;
            }
        }

        private static int PreliminaryCheck(string c, EbazarDB db)
        {
            int no_groups = 0;
            if (c == "alle")
                return no_groups;
            int count = int.Parse(SettingsHelper.Security.MD5_COUNT());

            string[] cats_arr = c.Split('-');
            cats_arr = cats_arr.Where(s => s != "").ToArray();

            if (cats_arr.Count() == 1 || cats_arr.Count() == groups + 1)
            {
                List<dto_category> cats = CategorysHelper.CatsNoNo;
                cats = cats.Where(x => SecurityHelper.Encode(SecurityHelper.Format(x.name, "_", true)) == cats_arr[0]).ToList();
                if (cats.IsNullOrEmpty())//kun parent?
                    return -1;
                no_groups++;
            }
            else
                return -1;
            for (int i = 1; i < cats_arr.Count(); i++)
            {
                if (cats_arr[i].Count() != count)
                    return -1;

                no_groups++;
            }
            return no_groups;
        }

        private static bool _GetParamsSelected(string c, string p, int no_groups, out List<biz_params> _params)
        {
            _params = new List<biz_params>();

            if (p == "")
                return true;
            if (no_groups == 0 || no_groups == 1)
                return true;
            if (no_groups != int.Parse(SettingsHelper.Security.GROUPS()) + 1)
                return false;

            string[] pa_str = p.Split('_').ToArray();

            bool ok;
            StringHelper.Only(p, CharacterHelper.Param(), out ok);
            if (!ok)
                return false;
            foreach (string s1 in pa_str)
            {
                string[] arr = s1.Split(':');
                foreach (string s2 in arr)
                {
                    if (s2.Count() != 1)
                        return false;
                    if (s2 != "0" && s2 != "1")
                        return false;
                }
            }

            special_ae = CategorysHelper.CatsYesYes.Where(_c => _c.name.Contains("ae")).ToList();
            special_æ = CategorysHelper.CatsYesYes.Where(_c => _c.name.Contains("æ") || _c.name.Contains("ø") || _c.name.Contains("å")).ToList();

            dto_category c_top = GetTop(c.Split('-')[0]);
            string c_top_name = c_top.name;
            string[] groups = { c.Split('-')[1], c.Split('-')[2] };
            List<dto_category> subs_tmp = new List<dto_category>();
            List<dto_category> subs;
            if (SettingsHelper.Security.MD5() != "true")
                subs = (subs_tmp = Dec_FromBitsToList(c_top, groups[0], 0)).Count > 0 ? subs_tmp : Dec_FromBitsToList(c_top, groups[1], 1);
            else
            {
                char c1 = (char)int.Parse(Dec_FromHashToChar(groups[0]));
                char c2 = (char)int.Parse(Dec_FromHashToChar(groups[1]));
                subs = (subs_tmp = Dec_FromHexToList(c_top_name, c1, 0)).Count > 0 ? subs_tmp : Dec_FromHexToList(c_top.name, c2, 1);
            }

            //c_top_name = Replace(c_top.Name, '-').Replace("_", " ").Replace("-", "");
            c_top_name = c_top.name.Replace("_", " ").Replace("-", "");

            List<dto_params> parms = (CategorysHelper.s_Params()[c_top_name])[subs[0].name].OrderBy(x => x.prio).OrderByDescending(x => x.type).ToList();
            int no_m_ms = parms.Where(x => x.type == "M" || x.type == "MS").Count();
            int no_s = parms.Where(x => x.type == "S").Count();

            if (pa_str.Where(x => !x.Contains(":")).Count() != no_s)
                return false;
            if (pa_str.Where(x => x.Contains(":")).Count() != no_m_ms)
                return false;

            int par_counter = 0;
            foreach (dto_params par in parms)
            {
                if (par.type == "S" && !pa_str[par_counter].Contains(':'))
                {
                    if (pa_str[par_counter].Substring(0, 1) == "1")
                        _params.Add(new biz_params() { name = par.name, type = par.type });
                    par_counter++;
                }
                else if (par.type == "M" || par.type == "MS")
                {
                    string[] va_str = pa_str[par_counter].Split(':').ToArray();
                    if (va_str.Count() != par.values_daos.Count())
                        return false;
                    biz_params p_val = new biz_params() { name = par.name, type = par.type };
                    int val_counter = 0;
                    bool add_param = false;
                    foreach (dto_value val in par.values_daos)
                    {
                        if (pa_str[par_counter].Contains(':') && va_str[val_counter].Substring(0, 1) == "1")
                        {
                            if (p_val.values_daos == null)
                                p_val.values_daos = new List<biz_value>();
                            p_val.values_daos.Add(new biz_value() { value = val.value, Id = val.Id });
                            add_param = true;
                        }
                        val_counter++;
                    }
                    if (add_param)
                        _params.Add(p_val);
                    par_counter++;
                }
            }

            return true;
        }

        public static bool First(List<string> area_selected, string area_check, string c, string p, out string c_url, out string c_search, out List<biz_params> _params)
        {
            using (EbazarDB db = new EbazarDB())
            {


                _params = new List<biz_params>();
                c_search = "none";
                c_url = "none";

                if (string.IsNullOrEmpty(c))
                {
                    //string subject = "Preliminary A";
                    //string body = "Cat (c): " + c;

                    //Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                    return false;
                }
                //if (p == null)
                //{
                //    //string subject = "Preliminary B";
                //    //string body = "Cat (c): " + c;
                //
                //    //Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                //    return false;
                //}

                int no_groups = PreliminaryCheck(c, db);
                if (no_groups < 0)
                {
                    //string subject = "Preliminary C";
                    //string body = "Cat (c): " + c;

                    //Admin.Notification.Run(Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), Settings.Basic.EMAIL_MAIL(), subject, body);
                    return false;
                }

                bool ok;
                c_search = StringHelper.OnlyAlphanumeric(SecurityHelper.DecodeCats_MD5(c, false), false, true, "notag", CharacterHelper.Category(), out ok);
                c_url = StringHelper.OnlyAlphanumeric(SecurityHelper.DecodeCats_MD5(c, true), false, true, "notag", CharacterHelper.Category(), out ok);

                area_selected = area_selected != null ? SecurityHelper.StringReplaceList(area_selected, '-') : null;
                area_check = !string.IsNullOrEmpty(area_check) ? SecurityHelper.OETOrig(area_check, '-') : "";
                List<string> areas_list = area_check.Split('-').Where(a => a != "").ToList();
                AreasHelper.selected = area_check != "" ? areas_list : area_selected;

                if (c_url.Contains("none"))
                    return false;
                foreach (string a in AreasHelper.selected)
                {
                    if (!AreasHelper.IsArea(a))
                        return false;
                }

                if (!_GetParamsSelected(c, p, no_groups, out _params))
                    return false;

                return true;
            }
        }

        static int SparseBitcount(int n)
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n &= (n - 1);
            }
            return count;
        }

        private static BYTE_CHECK CheckByte_Same(byte curr, byte prev)
        {
            int count = 0;
            for (int i = 0; i < 7; i++)
            {
                if ((((prev & (0x01 << i)) == 0x01 << i) && ((curr & (0x01 << i)) == 0x01 << i)) || (((prev & (0x01 << i)) == 0x00) && (curr & (0x01 << i)) == 0x00))
                    ;
                else
                    count++;

                if (count > 0)
                    return BYTE_CHECK.ERROR;
            }
            return BYTE_CHECK.SAME;
        }

        private static BYTE_CHECK CheckByte_Up(byte curr, byte prev)
        {
            int count = 0;
            for (int i = 0; i < 7; i++)
            {
                if ((((prev & (0x01 << i)) == 0x01 << i) && ((curr & (0x01 << i)) == 0x01 << i)) || (((prev & (0x01 << i)) == 0x00) && (curr & (0x01 << i)) == 0x00))
                    ;
                else
                {
                    if ((curr & (0x01 << i)) == 0x01 << i)//opad
                        count++;
                }

                if (count > 1)
                    return BYTE_CHECK.ERROR;
            }
            return count == 0 ? BYTE_CHECK.SAME : BYTE_CHECK.UP;
        }

        private static BYTE_CHECK CheckByte_Down(byte curr, byte prev)
        {
            int count = 0;
            for (int i = 0; i < 7; i++)
            {
                if ((((prev & (0x01 << i)) == 0x01 << i) && ((curr & (0x01 << i)) == 0x01 << i)) || (((prev & (0x01 << i)) == 0x00) && (curr & (0x01 << i)) == 0x00))
                    ;
                else
                {
                    if ((curr & (0x01 << i)) == 0x00)//nedad
                        count++;
                }

                if (count > 1)
                    return BYTE_CHECK.ERROR;
            }
            return count == 0 ? BYTE_CHECK.SAME : BYTE_CHECK.DOWN;
        }

        private static BYTE_CHECK CheckCats_Same(string[] curr_arr, string[] prev_arr)
        {
            BYTE_CHECK ok;
            for (int i = 1; i < groups + 1; i++)
            {
                string curr = Dec_FromHashToChar(curr_arr[i]);
                string prev = Dec_FromHashToChar(prev_arr[i]);

                byte c_curr = (byte)int.Parse(curr);
                byte c_prev = (byte)int.Parse(prev);

                ok = CheckByte_Same(c_curr, c_prev);
                if (ok == BYTE_CHECK.ERROR)
                    return BYTE_CHECK.ERROR;
            }
            return BYTE_CHECK.SAME;
        }

        private static bool CheckCats_SameGroup(string[] curr_arr, string[] prev_arr)
        {
            int same = 0;
            for (int i = 1; i < groups + 1; i++)
            {
                if(curr_arr[i] != prev_arr[i])
                    same++;
            }
            return same == 0;
        }

        private static BYTE_CHECK CheckCats_Up(string[] curr_arr, string[] prev_arr)
        {
            BYTE_CHECK ok = BYTE_CHECK.ERROR;
            int counter = 0;
            for (int i = 1; i < groups + 1; i++)
            {
                string curr = Dec_FromHashToChar(curr_arr[i]);
                string prev = Dec_FromHashToChar(prev_arr[i]);

                byte c_curr = (byte)int.Parse(curr);
                byte c_prev = (byte)int.Parse(prev);

                ok = CheckByte_Up(c_curr, c_prev);
                if (ok == BYTE_CHECK.ERROR)
                    return BYTE_CHECK.ERROR;
                if (ok == BYTE_CHECK.UP)
                    counter++;
                if (counter > 1)
                    return BYTE_CHECK.ERROR;
            }
            return counter == 0 ? BYTE_CHECK.SAME : BYTE_CHECK.UP;
        }

        private static BYTE_CHECK CheckCats_Down(string[] curr_arr, string[] prev_arr)
        {
            BYTE_CHECK ok = BYTE_CHECK.ERROR;
            int counter = 0;
            for (int i = 1; i < groups + 1; i++)
            {
                string curr = Dec_FromHashToChar(curr_arr[i]);
                string prev = Dec_FromHashToChar(prev_arr[i]);

                byte c_curr = (byte)int.Parse(curr);
                byte c_prev = (byte)int.Parse(prev);

                ok = CheckByte_Down(c_curr, c_prev);
                if (ok == BYTE_CHECK.ERROR)
                    return BYTE_CHECK.ERROR;
                if (ok == BYTE_CHECK.DOWN)
                    counter++;
                if (counter > 1)
                    return BYTE_CHECK.ERROR;
            }
            return counter == 0 ? BYTE_CHECK.SAME : BYTE_CHECK.DOWN;
        }

        public static Boolean ipv4(String strIP)
        {
            //  Split string by ".", check that array length is 3
            char chrFullStop = '.';
            string[] arrOctets = strIP.Split(chrFullStop);
            if (arrOctets.Length != 4)
            {
                return false;
            }
            //  Check each substring checking that the int value is less than 255 and that is char[] length is !> 2
            Int16 MAXVALUE = 255;
            Int32 temp; // Parse returns Int32
            foreach (String strOctet in arrOctets)
            {
                if (strOctet.Length > 3)
                {
                    return false;
                }

                temp = int.Parse(strOctet);
                if (temp > MAXVALUE)
                {
                    return false;
                }
            }
            return true;
        }

        public static string CheckIPValid(string strIP)
        {
            //IPAddress result = null;
            //return !String.IsNullOrEmpty(strIP) && IPAddress.TryParse(strIP, out result);
            IPAddress address;
            if (IPAddress.TryParse(strIP, out address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        // we have IPv4
                        if (ipv4(strIP))
                            return "ipv4";
                        return "Not";
                    //break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        // we have IPv6
                        return "ipv6";
                    //break;
                    default:
                        // umm... yeah... I'm going to need to take your red packet and...
                        return null;
                        //break;
                }
            }
            return null;
        }

        public static bool Second(/*string ip, string cats*/)
        {
            HttpRequestBase httpRequestBase = new HttpRequestWrapper(System.Web.HttpContext.Current.Request);
            string ip = RequestHelpers.GetClientIpAddress(httpRequestBase);

            //if (string.IsNullOrEmpty(ip))
            //    return false;
            //if (string.IsNullOrEmpty(cats))
            //    return false;

            return CheckIPValid(ip) == "ipv4" || CheckIPValid(ip) == "ipv6";
            //if (!ch)
            //    return false;

            //if (ch && cats == "alle")
            //    return true;

            //List<Log> ip_stats = IP_Logs.Get(ip).OrderByDescending(i => i.date).ToList();
            //Log prev_log = ip_stats.Count() > 0 ? ip_stats.ElementAt(0) : null;
            //bool no_prev_ip = prev_ip == null || prev_ip.cats == "alle";
            
            //string[] curr_arr = cats.Split('-').Where(s => s != "").ToArray();
            //string[] prev_arr = new string[1];
            //if (cats != "alle")
            //    prev_arr = prev_log.cats.Split('-').Where(s => s != "").ToArray();

            //if (curr_arr.Count() == groups + 1 && curr_arr[0] != prev_arr[0])//hvis vi springer i kategorier
            //    return true;
            //if (curr_arr.Count() == 1)
            //    return true;
            //if (prev_arr.Count() == 1)
            //    return true;

            //if (curr_arr.Count() == groups + 1  && prev_arr.Count() == groups + 1)
            //{
            //    if(CheckCats_SameGroup(curr_arr, prev_arr))
            //    {
            //        BYTE_CHECK same = BYTE_CHECK.ERROR;
            //        same = CheckCats_Same(curr_arr, prev_arr);
            //        if (same != BYTE_CHECK.SAME)
            //        {
            //            BYTE_CHECK up = CheckCats_Up(curr_arr, prev_arr);
            //            BYTE_CHECK down = CheckCats_Down(curr_arr, prev_arr);
            //            if (!((up == BYTE_CHECK.UP && down == BYTE_CHECK.SAME) || (up == BYTE_CHECK.SAME && down == BYTE_CHECK.DOWN)))
            //                return false;
            //        }
            //    }
            //}
            
            //return true;
        }
    }
}