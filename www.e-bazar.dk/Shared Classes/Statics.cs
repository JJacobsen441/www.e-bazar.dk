using System;
using System.IO;
using System.Web.Hosting;

namespace www.e_bazar.dk.SharedClasses
{
    public class Statics
    {
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
    }
}
