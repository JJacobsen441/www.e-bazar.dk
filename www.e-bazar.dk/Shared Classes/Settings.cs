using System;
using System.Xml.Linq;

namespace www.e_bazar.dk.SharedClasses
{
    public class Settings
    {
        public class Security
        {
            public static string MD5()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("security");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "MD5")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string MD5_COUNT()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("security");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "MD5_COUNT")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string GROUPS()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("security");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "GROUPS")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
        }
        public class Basic
        {
            public static string SITENAME()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "SITE_NAME")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string SITENAME_SHORT()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "SITE_NAME_SHORT")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string SITENAME_SHORT_CAP()
            {
                string str = SITENAME_SHORT();
                return ("" + str[0]).ToUpper() + str.Substring(1);
            }
            public static string SITENAME_SHORT_UP()
            {
                string str = SITENAME_SHORT();
                return str.ToUpper();
            }
            public static string SITE_NAME_FULL()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "SITE_NAME_FULL")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string SLOGAN()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "SLOGAN")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string COMMENT()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "COMMENT")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string EMAIL_ADMIN()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "EMAIL_ADMIN")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string EMAIL_MAIL()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "EMAIL_MAIL")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string EMAIL_NO_REPLY()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "EMAIL_NO_REPLY")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string EMAIL_TEST()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "EMAIL_TEST")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            public static string IP()
            {
                var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
                var group = xdoc.Elements("basic");

                foreach (XElement elem in group.Descendants())
                {
                    if (elem.Name == "setting" && elem.Attribute("name").Value == "IP")
                    {
                        return elem.Value;
                    }
                }
                throw new Exception("A-OK, Check.");
            }
            //public static string CVR()
            //{
            //    var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
            //    var group = xdoc.Elements("basic");

            //    foreach (XElement elem in group.Descendants())
            //    {
            //        if (elem.Name == "setting" && elem.Attribute("name").Value == "CVR")
            //        {
            //            return elem.Value;
            //        }
            //    }
            //    throw new Exception("A-OK, Check.");
            //}
            //public static string ACCOUNT()
            //{
            //    var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
            //    var group = xdoc.Elements("basic");

            //    foreach (XElement elem in group.Descendants())
            //    {
            //        if (elem.Name == "setting" && elem.Attribute("name").Value == "ACCOUNT")
            //        {
            //            return elem.Value;
            //        }
            //    }
            //    throw new Exception("A-OK Handled.");
            //}
            //public static string CVR()
            //{
            //    var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
            //    var group = xdoc.Elements("basic");

            //    foreach (XElement elem in group.Descendants())
            //    {
            //        if (elem.Name == "setting" && elem.Attribute("name").Value == "CVR")
            //        {
            //            return elem.Value;
            //        }
            //    }
            //    throw new Exception("A-OK Handled.");
            //}
            //public static string CLIENT_ID()
            //{
            //    var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
            //    var group = xdoc.Elements("basic");

            //    foreach (XElement elem in group.Descendants())
            //    {
            //        if (elem.Name == "setting" && elem.Attribute("name").Value == "CLIENT_ID")
            //        {
            //            return elem.Value;
            //        }
            //    }
            //    throw new Exception("A-OK Handled.");
            //}
            //public static string SECRET()
            //{
            //    var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
            //    var group = xdoc.Elements("basic");

            //    foreach (XElement elem in group.Descendants())
            //    {
            //        if (elem.Name == "setting" && elem.Attribute("name").Value == "SECRET")
            //        {
            //            return elem.Value;
            //        }
            //    }
            //    throw new Exception("A-OK Handled.");
            //}
            //public static string TESTPROFILE()
            //{
            //    var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
            //    var group = xdoc.Elements("basic");

            //    foreach (XElement elem in group.Descendants())
            //    {
            //        if (elem.Name == "setting" && elem.Attribute("name").Value == "TESTPROFILE")
            //        {
            //            return elem.Value;
            //        }
            //    }
            //    throw new Exception("A-OK Handled.");
            //}
            //public static string TESTPROFILEPASS()
            //{
            //    var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
            //    var group = xdoc.Elements("basic");

            //    foreach (XElement elem in group.Descendants())
            //    {
            //        if (elem.Name == "setting" && elem.Attribute("name").Value == "TESTPROFILEPASS")
            //        {
            //            return elem.Value;
            //        }
            //    }
            //    throw new Exception("A-OK Handled.");
            //}
        }
        //public class System
        //{
        //    public static string TAG_MAX_LENGTH()
        //    {
        //        var xdoc = XElement.Load(Statics.Root + "App_Data\\settings.xml");
        //        var group = xdoc.Elements("system");

        //        foreach (XElement elem in group.Descendants())
        //        {
        //            if (elem.Name == "setting" && elem.Attribute("name").Value == "TAG_MAX_LENGTH")
        //            {
        //                return elem.Value;
        //            }
        //        }
        //        throw new Exception("A-OK, Check.");
        //    }
        //}
    }
}
