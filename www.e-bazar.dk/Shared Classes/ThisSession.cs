using System.Collections.Generic;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.SharedClasses
{
    public class ThisSession
    {
        public static string CurrentUserId
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["user_id"] != null)
                    return (string)System.Web.HttpContext.Current.Session["user_id"];
                return "";
            }
            set
            {
                string s = "";
                if (!string.IsNullOrEmpty(value))
                    s = value;
                System.Web.HttpContext.Current.Session["user_id"] = s;
            }
        }

        public static string CurrentUserName
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["user_name"] != null)
                    return (string)System.Web.HttpContext.Current.Session["user_name"];
                return "";
            }
            set
            {
                string s = "";
                if (!string.IsNullOrEmpty(value))
                    s = value;
                System.Web.HttpContext.Current.Session["user_name"] = s;
            }
        }

        public static bool CurrentIsAuthenticated
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["is_auth"] != null)
                    return (string)System.Web.HttpContext.Current.Session["is_auth"] == "true" ? true : false;
                return false;
            }
            set
            {
                System.Web.HttpContext.Current.Session["is_auth"] = value == true ? "true" : "false";
            }
        }

        public static string CurrentType
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["c_type"] != null)
                    return (string)System.Web.HttpContext.Current.Session["c_type"];
                return "none";
            }
            set
            {
                System.Web.HttpContext.Current.Session["c_type"] = value;
            }
        }

        public static List<string> Area
        {
            get
            {
                return System.Web.HttpContext.Current.Session["area"] as List<string>;
            }
            set
            {
                System.Web.HttpContext.Current.Session["area"] = value;
            }
        }

        public static string Catalog
        {
            get
            {
                if (!ThisSession.Cookie)
                    return "";
                if (System.Web.HttpContext.Current.Session["catalog"] == null)
                    return "";
                return (string)System.Web.HttpContext.Current.Session["catalog"];
            }
            set
            {
                if (!ThisSession.Cookie)
                    return;
                string s = string.IsNullOrEmpty(value) ? "" : value;
                System.Web.HttpContext.Current.Session["catalog"] = s;
            }
        }
        
        public static bool Cookie
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["cookie"] == null)
                    return false;
                return (bool)System.Web.HttpContext.Current.Session["cookie"] || CurrentUser.Inst().CurrentIsAuthenticated;
            }
            set
            {
                System.Web.HttpContext.Current.Session["cookie"] = value;
            }
        }

        public static string Search
        {
            get
            {
                if (!ThisSession.Cookie)
                    return "";
                if (System.Web.HttpContext.Current.Session["z"] == null)
                    return "";
                return (string)System.Web.HttpContext.Current.Session["s"];
            }
            set
            {
                if (!ThisSession.Cookie)
                    return;
                string s = string.IsNullOrEmpty(value) ? "" : value;
                System.Web.HttpContext.Current.Session["s"] = s;
            }
        }

        public static string Category
        {
            get
            {
                if (!ThisSession.Cookie)
                    return "alle";
                if (System.Web.HttpContext.Current.Session["c"] == null)
                    return "alle";
                return (string)System.Web.HttpContext.Current.Session["c"];
            }
            set
            {
                if (!ThisSession.Cookie)
                    return;
                string s = string.IsNullOrEmpty(value) ? "" : value;
                System.Web.HttpContext.Current.Session["c"] = value;
            }
        }

        public static List<biz_params> Params
        {
            get
            {
                //if (!Cookie)
                //    return new Dictionary<string, string>();
                if (System.Web.HttpContext.Current.Session["param"] != null)
                    return (List<biz_params>)System.Web.HttpContext.Current.Session["param"];
                return new List<biz_params>();
            }
            set
            {
                //if (!Cookie)
                //    return;
                System.Web.HttpContext.Current.Session["param"] = value;
            }
        }

        public static RelevantHelper Relevant
        {
            get
            {
                //if (!Cookie)
                //    return new Dictionary<string, string>();
                //if (System.Web.HttpContext.Current.Session["param"] != null)
                    return (RelevantHelper)System.Web.HttpContext.Current.Session["rele"];
                //return new List<biz_params>();
            }
            set
            {
                //if (!Cookie)
                //    return;
                if (System.Web.HttpContext.Current.Session["rele"].IsNull())
                    System.Web.HttpContext.Current.Session["rele"] = value;
            }
        }

        public static int Zip
        {
            get
            {
                if (!ThisSession.Cookie)
                    return 0;
                if (System.Web.HttpContext.Current.Session["z"] == null)
                    return 0;
                return (int)System.Web.HttpContext.Current.Session["z"];
            }
            set
            {
                if (!ThisSession.Cookie)
                    return;
                System.Web.HttpContext.Current.Session["z"] = value;
            }
        }

        public static int Fra
        {
            get
            {
                if (!ThisSession.Cookie)
                    return 0;
                if (System.Web.HttpContext.Current.Session["f"] == null)
                    return 0;
                return (int)System.Web.HttpContext.Current.Session["f"];
            }
            set
            {
                if (!ThisSession.Cookie)
                    return;
                System.Web.HttpContext.Current.Session["f"] = value;
            }
        }

        public static int Til
        {
            get
            {
                if (!ThisSession.Cookie)
                    return 999999;
                if (System.Web.HttpContext.Current.Session["t"] == null)
                    return 999999;
                return (int)System.Web.HttpContext.Current.Session["t"];
            }
            set
            {
                if (!ThisSession.Cookie)
                    return;
                System.Web.HttpContext.Current.Session["t"] = value;
            }
        }

        public static bool FastPris
        {
            get
            {
                if (!ThisSession.Cookie)
                    return false;
                if (System.Web.HttpContext.Current.Session["g"] == null)
                    return false;
                return (bool)System.Web.HttpContext.Current.Session["g"];
            }
            set
            {
                if (!ThisSession.Cookie)
                    return;
                System.Web.HttpContext.Current.Session["g"] = value;
            }
        }

        public static int Paginator
        {
            get
            {
                if (!ThisSession.Cookie)
                    return 1;
                if (System.Web.HttpContext.Current.Session["paginator"] == null)
                    return 1;
                return (int)System.Web.HttpContext.Current.Session["paginator"];
            }
            set
            {
                if (!ThisSession.Cookie)
                    return;
                System.Web.HttpContext.Current.Session["paginator"] = value;
            }
        }

        public static string Tab
        {
            get
            {
                if (!Cookie)
                    return "";
                if (System.Web.HttpContext.Current.Session["Tab"] != null)
                    return (string)System.Web.HttpContext.Current.Session["Tab"];
                return "";
            }
            set
            {
                if (!Cookie)
                    return;
                string s = string.IsNullOrEmpty(value) ? "" : value;
                System.Web.HttpContext.Current.Session["Tab"] = s;
            }
        }

        public static Dictionary<string, string> Json_Errors
        {
            get
            {
                //if (!Cookie)
                //    return new Dictionary<string, string>();
                if (System.Web.HttpContext.Current.Session["Json_Errors"] != null)
                    return (Dictionary<string, string>)System.Web.HttpContext.Current.Session["Json_Errors"];
                return new Dictionary<string, string>();
            }
            set
            {
                //if (!Cookie)
                //    return;
                System.Web.HttpContext.Current.Session["Json_Errors"] = value;
            }
        }

        public static Dictionary<string, string> Json_Messages
        {
            get
            {
                //if (!Cookie)
                //    return new Dictionary<string, string>();
                if (System.Web.HttpContext.Current.Session["Json_Messages"] != null)
                    return (Dictionary<string, string>)System.Web.HttpContext.Current.Session["Json_Messages"];
                return new Dictionary<string, string>();
            }
            set
            {
                //if (!Cookie)
                //    return;
                System.Web.HttpContext.Current.Session["Json_Messages"] = value;
            }
        }
    }
}