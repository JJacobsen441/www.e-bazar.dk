using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using static www.e_bazar.dk.Models.DTOs.poco_booth;

namespace www.e_bazar.dk.SharedClasses
{
    public class Statics
    {
        public class Characters
        {
            public static char[] All(bool withreturnnewline)
            {
                char c = '\r';
                char r = '\n';
                //char[] a = new char[] { ' ', '.', ',', '\'', '"', ':', ';', '&', '#', '!', '?', '/', '%', '+', '-', '(', ')', '[', ']', '{', '}', '<', '>' };
                char[] a = new char[] { ' ', '.', ',', '+', '-', '*', '/', ':', ';', '_', '\'', '#', '(', ')', '[', ']', '<', '>', '{', '}', '=', '?', '!', '@', '%', '$', '&' };
                if (withreturnnewline)
                    a = a.Concat(new char[] { c }).ToArray();
                if (withreturnnewline)
                    a = a.Concat(new char[] { r }).ToArray();
                return a;
            }
            public static char[] Limited(bool withsemi)
            {
                char s = ';';
                char[] a = new char[] { ' ', '_', ',', '+', '-', '\'', '&', '#', '(', ')', '[', ']' };
                if (withsemi)
                    a = a.Concat(new char[] { s }).ToArray();
                return a;
            }
            public static char[] VeryLimited()
            {
                return new char[] { ' ', '\'', '-', '&', '#' };
            }
            public static char[] Website()
            {
                return new char[] { '-', '/', '.', ':' };
            }
            public static char[] Category()
            {
                return new char[] { ' ', '-', '.' };
            }
            public static char[] Name()
            {
                return new char[] { ' ', '-', '*' };
            }
            public static char[] Country()
            {
                return new char[] { ' ', '-' };
            }
            public static char[] Address()
            {
                return new char[] { ' ', '.', ',', '-', '\'' };
            }
            public static char[] Space()
            {
                return new char[] { ' ' };
            }
            public static char[] Param()
            {
                return new char[] { '0', '1', ':', '_' };
            }
        }
        public static string FormatMail(string text)
        {
            if (String.IsNullOrEmpty(text))
                return "";

            text = text.Replace("@", " AT ");
            text = text.Replace(".", " DOT ");

            return text;
        }

        private static bool _m = false;
        public static bool Maintenance 
        {
            get
            {
                return _m;
            }
            set
            {
                _m = value;
            }
        }
        public static bool IsDebug
        {
            get
            {
                bool isDebug = false;
#if DEBUG
                isDebug = true;
#endif
                return isDebug;
            }
        }
        //public static string MyIP
        //{
        //    get
        //    {
        //        return "80.161.50.61";
        //    }
        //}
        public static string Root
        {
            get
            {
                string app_path = HostingEnvironment.ApplicationPhysicalPath;
                string nd = Path.DirectorySeparatorChar.ToString();
                string r = "";
                if (IsDebug)
                    r = app_path;
                else
                {
                    //Statics.Log("path > " + app_path);
                    if (app_path.EndsWith("bin\\"))
                        app_path = app_path.Replace("bin\\", "");
                    if (app_path.EndsWith("bin/"))
                        app_path = app_path.Replace("bin/", "");
                    if (app_path.EndsWith("bin"))
                        app_path = app_path.Substring(0, app_path.Length - 3);
                    r = app_path;
                    //Statics.Log("path > " + app_path);
                }
                return r;
            }
        }
        public static string Content
        {
            get
            {
                string nd = Path.DirectorySeparatorChar.ToString();
                return Root + "_content" + nd;
            }
        }
        //public static bool IsAdmin(string ip)
        //{
        //    if (IsDebug)
        //        return ip == "::1" || ip == "127.0.0.1" || ip == MyIP;

        //    return ip == MyIP;
        //}
        public static void Log(string msg, bool loop = false)
        {
            try
            {
                string nd = Path.DirectorySeparatorChar.ToString();
                string path = HostingEnvironment.ApplicationPhysicalPath + "_content" + nd + "log" + nd + "logfile.txt";
                
                using (StreamWriter writer = System.IO.File.AppendText(path))
                {
                    string d = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                    writer.WriteLine(d + ": " + msg);
                }
            }
            catch (Exception e)
            {
                ;
            }
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        public static bool IsNullOrEmpty<T>(ICollection<T> value) where T : class
        {
            return value == null || value.Count() == 0;
        }
        public static bool IsNull<T>(T value) where T : class
        {
            return value == null;
        }

        public static bool IsNotNull<T>(T value) where T : class
        {
            return value != null;
        }

        public static bool IsNull<T>(T? nullableValue) where T : struct
        {
            return !nullableValue.HasValue;
        }

        public static bool IsNotNull<T>(T? nullableValue) where T : struct
        {
            return nullableValue.HasValue;
        }

        public static bool HasValue<T>(T? nullableValue) where T : struct
        {
            return nullableValue.HasValue;
        }

        public static bool HasNoValue<T>(T? nullableValue) where T : struct
        {
            return !nullableValue.HasValue;
        }

        
    }
}
