using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.Identity;

namespace www.e_bazar.dk.Statics
{
    public class AdminHelper
    {
        public static class Notification
        {
            public static void Run(string from, string to, string cred, string subject, string body)
            {
                MailMessage mail = new MailMessage(from, to);
                SmtpClient client = new SmtpClient();
                client = new SmtpClient();
                client.Credentials = new NetworkCredential(cred, "Nostromo2503");
                client.Port = 25;
                client.EnableSsl = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.UseDefaultCredentials = false;
                client.Host = "192.168.0.11";
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = body;

                client.Send(mail);
            }
        }
        public static class Commands
        {
            private static void UpdateGroup(string name_upper, int id_upper, int counter_upper, string desc, List<KeyValuePair<string, int>> lowers, List<int> id_lowers, EbazarDB db)
            {
                if (db.category.Where(x => x.name == ".ingen").FirstOrDefault() != null && name_upper == ".ingen")
                    return;

                List<category> cat_up = db.category/*.Where(c => c.is_parent == true)*/.ToList();
                if (cat_up != null)
                {
                    category cat = cat_up.Where(c => c.Id == id_upper).FirstOrDefault();
                    if (cat == null)
                    {
                        category new_cat = new category() { name = name_upper, is_parent = true, priority = counter_upper, description = desc };
                        db.category.Add(new_cat);
                        cat = new_cat;
                    }
                    else
                    {
                        cat.name = name_upper;
                        cat.priority = counter_upper;
                        cat.is_parent = true;
                        cat.description = desc;
                    }
                    string name_lower = "";
                    int id_lower = -1;
                    int priority_lower = -1;
                    int counter = 0;
                    foreach (KeyValuePair<string, int> p in lowers)
                    {
                        name_lower = p.Key;
                        priority_lower = p.Value;
                        id_lower = id_lowers.ElementAt(counter);
                        //if (!int.TryParse(p.Value, out id_lower))
                        //    id_lower = -1;
                        category c = cat_up.Where(ct => ct.Id == id_lower).FirstOrDefault();
                        if (c == null)
                        {
                            db.category.Add(new category() { name = name_lower, is_parent = false, priority = priority_lower, parent = cat });
                        }
                        else
                        {
                            c.name = name_lower;
                            c.priority = priority_lower;
                            c.is_parent = false;
                            c.parent = cat;
                        }
                        counter++;
                    }
                }
            }
            private static void AddCategories(XDocument xdoc, EbazarDB db)
            {
                string name_upper = "";
                int id_upper = -1;
                int counter_upper = 0;
                int counter_lower = 0;
                List<KeyValuePair<string, int>> lowers = new List<KeyValuePair<string, int>>();
                List<int> id_lowers = new List<int>();
                string desc = "";
                foreach (XElement element in xdoc.Descendants())
                {
                    if (element.Name == "newupper")
                    {
                        if (lowers.Count > 0)
                            UpdateGroup(name_upper, id_upper, counter_upper, desc, lowers, id_lowers, db);

                        lowers = new List<KeyValuePair<string, int>>();
                        id_lowers = new List<int>();
                        name_upper = element.Attribute("name").Value;
                        if (!int.TryParse(element.Attribute("id").Value, out id_upper))
                            id_upper = -1;
                        counter_upper++;
                        counter_lower = 0;
                    }
                    else if (element.Name == "newdesc")
                    {
                        desc = element.Value;
                    }
                    else if (element.Name == "newlower")
                    {
                        counter_lower++;
                        int id_lower = -1;
                        if (!int.TryParse(element.Attribute("id").Value, out id_lower))
                            id_lower = -1;
                        lowers.Add(new KeyValuePair<string, int>(element.Attribute("name").Value, counter_lower));
                        id_lowers.Add(id_lower);
                    }
                    else
                        Console.WriteLine(element.Name);
                }
                UpdateGroup(name_upper, id_upper, counter_upper, desc, lowers, id_lowers, db);
            }
            private static void AddParams(XDocument xdoc, EbazarDB db)
            {
                category cat_upper = null;
                category cat_lower = null;
                //string name_lower = "";
                //int id_upper = -1;
                //int id_lower = -1;

                bool is_upper = true;
                int param_counter = 1;
                foreach (XElement element in xdoc.Descendants().InDocumentOrder())
                {
                    //bool param_level_top = true;
                    if (element.Name == "newupper")
                    {
                        param_counter = 1;
                        string name_upper = element.Attribute("name").Value;
                        cat_upper = db.category.Where(c => c.name == name_upper).FirstOrDefault();
                        is_upper = true;
                    }
                    else if (element.Name == "newlower")
                    {
                        param_counter = 1;
                        string name_upper = element.Attribute("name").Value;
                        if (cat_upper != null)
                            cat_lower = cat_upper.children.Where(c => c.name == name_upper).FirstOrDefault();
                        else
                            throw new Exception("A-OK, Check.");
                        is_upper = false;
                        //param_level_top = false;
                    }
                    else if (element.Name == "param")
                    {
                        //if (param_level_top)
                        //    is_upper = true;
                        string name_param = element.Attribute("name").Value;
                        string type_param = element.Attribute("type").Value;
                        string val_param = "";
                        if (element.Attribute("val") != null)
                            val_param = element.Attribute("val").Value;

                        param param = new param() { name = name_param, type = type_param, value = val_param, prio = param_counter, category_id = is_upper ? cat_upper.Id : cat_lower.Id };

                        if(db.param.Where(x=>x.name == name_param && x.category_id == (is_upper ? cat_upper.Id : cat_lower.Id)).FirstOrDefault() == null)
                        {
                            db.param.Add(param);
                            db.SaveChanges();
                        }

                        List<value> vals = new List<value>();
                        int prio = 1;
                        foreach (XElement val in element.Descendants().InDocumentOrder())
                        {
                            string val_value = val.Attribute("val").Value;
                            value v = new value() { value1 = val_value, prio = prio, param_id = param.Id };
                            vals.Add(v);
                            prio++;
                            if(db.value.Where(x=>x.value1 == val_value && x.param_id == param.Id).FirstOrDefault() == null)
                            {
                                db.value.Add(v);
                                db.SaveChanges();
                            }
                        }
                        //param.value1 = vals;
                        param_counter++;
                    }
                    else
                        Console.WriteLine(element.Name);
                }
            }
            
            public class _booth
            {
                public long b_id;
                public string cat_name;
            }
            public class _pro
            {
                public long p_id;
                public string cat_name_m;
                public string cat_name_s;
                public string active;
            }
            public class _col
            {
                public long c_id;
                public string cat_name_m;
                public string cat_name_s;
                public string active;
            }
            public class _lists
            {
                public List<_booth[]> _b;
                public List<_pro> _p;
                public List<_col> _c;
            }

            private static _lists StorePrevious(XDocument xdocb, EbazarDB db)
            {
                //category def_cat = new category() { name = "default", is_parent = true, priority = 1, description = "" };
                //if(db.category.Where(x=>x.name == "default").FirstOrDefault() == null)
                //    db.category.Add(def_cat);
                db.SaveChanges();
                List<_booth[]> booths = new List<_booth[]>();
                List<_pro> products = new List<_pro>();
                List<_col> collections = new List<_col>();

                category du = db.category.Where(x => x.name == ".ingen").FirstOrDefault();
                category dl = db.category.Where(x => x.name == "..ingen").FirstOrDefault();

                XElement root = xdocb.Element("root");
                foreach (booth b in db.booth.ToList())
                {
                    int counter = 0;
                    _booth[] arr = new _booth[b.category_main.Count()];
                    foreach (category c in b.category_main.ToList())
                    {
                        arr[counter] = new _booth();
                        arr[counter].b_id = b.Id;
                        arr[counter].cat_name = c.name;
                        b.category_main.Remove(c);
                        counter++;

                        XElement elem_b = new XElement("booth");
                        XAttribute b_id = new XAttribute("b_id", b.Id);
                        XAttribute b_name = new XAttribute("cat_name", c.name);
                        elem_b.Add(b_id);
                        elem_b.Add(b_name);
                        root.Add(elem_b);                    
                    }
                    booths.Add(arr);                    
                }

                foreach (product p in db.product.ToList())
                {
                    products.Add(new _pro { p_id = p.Id, cat_name_m = p.category_main.name, cat_name_s = p.category_second.name });
                    p.category_main_id = du.Id;
                    p.category_second_id = dl.Id;

                    XElement elem_p = new XElement("product");
                    XAttribute p_id = new XAttribute("p_id", p.Id);
                    XAttribute main = new XAttribute("cat_name_m", p.category_main.name);
                    XAttribute sec = new XAttribute("cat_name_s", p.category_second.name);
                    XAttribute active = new XAttribute("active", p.active ? "true" : "false");
                    elem_p.Add(p_id);
                    elem_p.Add(main);
                    elem_p.Add(sec);
                    elem_p.Add(active);
                    root.Add(elem_p);
                }
                foreach (collection c in db.collection.ToList())
                {
                    collections.Add(new _col { c_id = c.Id, cat_name_m = c.category_main.name, cat_name_s = c.category_second.name });
                    c.category_main_id = du.Id;
                    c.category_second_id = dl.Id;

                    XElement elem_c = new XElement("collection");
                    XAttribute c_id = new XAttribute("c_id", c.Id);
                    XAttribute main = new XAttribute("cat_name_m", c.category_main.name);
                    XAttribute sec = new XAttribute("cat_name_s", c.category_second.name);
                    XAttribute active = new XAttribute("active", c.active ? "true" : "false");
                    elem_c.Add(c_id);
                    elem_c.Add(main);
                    elem_c.Add(sec);
                    elem_c.Add(active);
                    root.Add(elem_c);
                }
                xdocb.Save(StaticsHelper.Root + "App_Data\\BackupCats.xml");
                db.SaveChanges();

                return new _lists { _b = booths, _p = products, _c = collections };
            }

            private static void RestorePrevious(EbazarDB db, _lists _l)
            {
                category du = db.category.Where(x => x.name == ".ingen").FirstOrDefault();
                category dl = db.category.Where(x => x.name == "..ingen").FirstOrDefault();

                foreach (_booth[] _bs in _l._b)
                {
                    foreach (_booth _b in _bs)
                    {
                        booth b = db.booth.Where(x => x.Id == _b.b_id).FirstOrDefault();
                        category c = db.category.Where(x => x.name == _b.cat_name && x.is_parent).FirstOrDefault();
                        if (c != null)
                            b.category_main.Add(c);
                        else
                            b.category_main.Add(du);
                    }
                }

                foreach (_pro _p in _l._p)
                {
                    product p = db.product.Include("booth").Where(x => x.Id == _p.p_id).FirstOrDefault();
                    category a = db.category.Include("parent").Where(x => x.is_parent  && x.name == _p.cat_name_m).FirstOrDefault();
                    category b = db.category.Include("parent").Where(x => !x.is_parent && x.name == _p.cat_name_s && x.parent.name == _p.cat_name_m).FirstOrDefault();
                    if (a != null && b != null)
                    {
                        if (a.name == ".ingen" || b.name == "..ingen")
                        {
                            if (!p.booth.category_main.Where(x => x.Id == du.Id).Any())
                                p.booth.category_main.Add(du);
                            p.active = false;
                        }
                        p.category_main_id = a.Id;
                        p.category_second_id = b.Id;
                    }
                    else
                    {
                        if(!p.booth.category_main.Where(x=>x.Id == du.Id).Any())
                            p.booth.category_main.Add(du);
                        p.active = false;
                        p.category_main_id = du.Id;
                        p.category_second_id = dl.Id;
                    }
                }
                foreach (_col _c in _l._c)
                {
                    collection c = db.collection.Include("booth").Where(x => x.Id == _c.c_id).FirstOrDefault();
                    category a = db.category.Include("parent").Where(x => x.is_parent  && x.name == _c.cat_name_m).FirstOrDefault();
                    category b = db.category.Include("parent").Where(x => !x.is_parent && x.name == _c.cat_name_s && x.parent.name == _c.cat_name_m).FirstOrDefault();
                    if (a != null && b != null)
                    {
                        if (a.name == ".ingen" || b.name == "..ingen")
                        {
                            if (!c.booth.category_main.Where(x => x.Id == du.Id).Any())
                                c.booth.category_main.Add(du);
                            c.active = false;
                        }
                        c.category_main_id = a.Id;
                        c.category_second_id = b.Id;
                    }
                    else
                    {
                        if (!c.booth.category_main.Where(x => x.Id == du.Id).Any())
                            c.booth.category_main.Add(du);
                        c.active = false;
                        c.category_main_id = du.Id;
                        c.category_second_id = dl.Id;
                    }
                }
                db.SaveChanges();
            }

            public static void RestorePreviousBackup(XDocument xdoc, EbazarDB db)
            {
                foreach (XElement element in xdoc.Descendants().InDocumentOrder())
                {
                    if (element.Name == "booth")
                    {
                        int b_id = int.Parse(element.Attribute("b_id").Value);
                        string cat_name = element.Attribute("cat_name").Value;
                        booth b = db.booth.Where(x => x.Id == b_id).FirstOrDefault();
                        category c = db.category.Where(x => x.name == cat_name).FirstOrDefault();
                        b.category_main.Add(c);
                    }
                    if (element.Name == "product")
                    {
                        long p_id = long.Parse(element.Attribute("p_id").Value);
                        string cat_name_m = element.Attribute("cat_name_m").Value;
                        string cat_name_s = element.Attribute("cat_name_s").Value;
                        string active = element.Attribute("active").Value;
                        product p = db.product.Where(x => x.Id == p_id).FirstOrDefault();
                        category a = db.category.Where(x => x.name == cat_name_m).FirstOrDefault();
                        category b = db.category.Where(x => x.name == cat_name_s).FirstOrDefault();
                        p.category_main_id = a.Id;
                        p.category_second_id = b.Id;
                        p.active = active == "true";
                    }
                    if (element.Name == "collection")
                    {
                        long c_id = long.Parse(element.Attribute("c_id").Value);
                        string cat_name_m = element.Attribute("cat_name_m").Value;
                        string cat_name_s = element.Attribute("cat_name_s").Value;
                        string active = element.Attribute("active").Value;
                        collection c = db.collection.Where(x => x.Id == c_id).FirstOrDefault();
                        category a = db.category.Where(x => x.name == cat_name_m).FirstOrDefault();
                        category b = db.category.Where(x => x.name == cat_name_s).FirstOrDefault();
                        c.category_main_id = a.Id;
                        c.category_second_id = b.Id;
                        c.active = active == "true";
                    }
                }
                db.SaveChanges();
            }


            public static void CleanCats(EbazarDB db)
            {

                foreach (category cat in db.category.ToList())
                {
                    if (cat.is_parent && cat.name != ".ingen")
                    {
                        foreach (category c in cat.children.ToList())
                        {
                            category _c = db.category.Where(x => x.name == "..ingen").FirstOrDefault();
                            c.parent = _c;
                            c.parent_id = _c.Id;
                            db.SaveChanges();
                            foreach (product pro in c.product_main)
                                pro.category_main_id = _c.Id;
                            foreach (product pro in c.product_second)
                                pro.category_second_id = _c.Id;
                            foreach (collection col in c.collection_main)
                                col.category_main_id = _c.Id;
                            foreach (collection col in c.collection_second)
                                col.category_second_id = _c.Id;
                            db.category.Remove(c);
                            db.SaveChanges();
                        }
                        db.SaveChanges();
                        db.category.Remove(cat);
                        db.SaveChanges();
                    }
                }
                db.SaveChanges();


            }
            public static void CleanParams(EbazarDB db)
            {

                foreach (param par in db.param.ToList())
                {
                    //if (cat.is_parent && cat.name != "default")
                    {
                        foreach (value v in par.value1.ToList())
                        {
                            db.value.Remove(v);
                            db.SaveChanges();
                        }
                        db.param.Remove(par);
                        db.SaveChanges();
                    }
                }
                db.SaveChanges();


            }
            public static void Categorys(bool clean_install, bool clean_up, EbazarDB db)
            {
                string patha = StaticsHelper.Root + "App_Data\\Categories.xml";
                string pathb = StaticsHelper.Root + "App_Data\\BackupCats.xml";
                XDocument xdoca = XDocument.Load(patha);
                XDocument xdocb = XDocument.Load(pathb);
                //List<Category>
                if (clean_install)
                {
                    _lists _l = StorePrevious(xdocb, db);
                    db.SaveChanges();
                    CleanParams(db);
                    db.SaveChanges();
                    CleanCats(db);
                    db.SaveChanges();

                    //db.Database.ExecuteSqlCommand("ALTER SEQUENCE category_id_seq RESTART WITH 10");
                    //db.Database.ExecuteSqlCommand("ALTER SEQUENCE param_id_seq RESTART WITH 10");
                    //db.Database.ExecuteSqlCommand("SELECT setval(\"param_id_seq\", 10)");
                    //db.Database.ExecuteSqlCommand("SELECT setval(\"value_id_seq\", 10)");

                    db.SaveChanges();

                    AddCategories(xdoca, db);
                    db.SaveChanges();
                    AddParams(xdoca, db);
                    db.SaveChanges();

                    RestorePrevious(db, _l);
                    db.SaveChanges();
                }
                else
                {
                    RestorePreviousBackup(xdocb, db);
                    db.SaveChanges();
                }
            }

            
            public static bool DeleteUser(string email, UserManager<ApplicationUser> UserManager, EbazarDB db)
            {
                bool ok = false;
                person pers = db.person.Where(per => per.email == email).FirstOrDefault();
                if (pers == null)
                    ok = true;
                else if(pers.descriminator == "Customer" || (pers.descriminator == "Salesman" && pers.booth.Count == 0))
                {
                    db.person.Remove(pers);
                    db.SaveChanges();
                    ok = true;
                }
                ApplicationUser user = UserManager.FindByEmail(email);
                if (ok && user != null)
                    UserManager.Delete(user);
                return ok;
            }
        }
    }
}
