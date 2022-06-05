using System;
using System.Collections.Generic;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DTOs;

namespace www.e_bazar.dk.SharedClasses
{
    public class Categorys
    {
        /*private static bool s_ok = false;
        private static bool p_ok = false;
        private static bool c_ok = false;
        private static bool p_r_ok = false;
        private static bool c_r_ok = false;

        public static bool shops_ok { get { return s_ok; } set { s_ok = value; } }
        public static bool products_ok { get { return p_ok; } set { p_ok = value; } }
        public static bool collections_ok { get { return c_ok; } set { c_ok = value; } }
        public static bool products_r_ok { get { return p_r_ok; } set { p_r_ok = value; } }
        public static bool collections_r_ok { get { return c_r_ok; } set { c_r_ok = value; } }

        private static IEnumerable<Shop> shops = null;
        public static IEnumerable<Shop> Shops
        {
            get
            {
                if (shops == null)
                    return new List<Shop>();
                return shops;
            }
            set
            {
                Cache.shops_ok = true;
                shops = value;
            }
        }

        private static IEnumerable<Product> products = null;
        public static IEnumerable<Product> Products
        {
            get
            {
                if (products == null)
                    return new List<Product>();
                return products.ToList();
            }
            set
            {
                Cache.products_ok = true;
                products = value;
            }
        }
        private static List<Product> products_r = null;
        public static List<Product> Products_Relevant
        {
            get
            {
                if (products_r == null)
                    return new List<Product>();
                return products_r;
            }
            set
            {
                Cache.products_r_ok = true;
                products_r = value;
            }
        }

        private static List<Collection> collections = null;
        public static List<Collection> Collections
        {
            get
            {
                if (collections == null)
                    return new List<Collection>();
                return collections.ToList();
            }
            set
            {
                Cache.collections_ok = true;
                collections = value;
            }
        }

        private static IEnumerable<Collection> collections_r = null;
        public static IEnumerable<Collection> Collections_Relevant
        {
            get
            {
                if (collections_r == null)
                    return new List<Collection>();
                return collections_r.ToList();
            }
            set
            {
                Cache.collections_r_ok = true;
                collections_r = value;
            }
        }*/

        public static List<dto_category> CatsParam
        {
            get
            {
                biz_category poco = new biz_category();
                return poco._GetAll(false, false);
            }
            private set { }
        }
        public static List<dto_category> CatsNoNo
        {
            get
            {
                biz_category poco = new biz_category();
                return poco._GetAll(false, false);
            }
            private set { }
        }
        public static List<dto_category> CatsYesNo
        {
            get
            {
                biz_category poco = new biz_category();
                return poco._GetAll(true, false);
            }
            private set { }
        }
        public static List<dto_category> CatsNoYes
        {
            get
            {
                biz_category poco = new biz_category();
                return poco._GetAll(false, true);
            }
            private set { }
        }
        public static List<dto_category> CatsYesYes
        {
            get
            {
                biz_category poco = new biz_category();
                return poco._GetAll(true, true);
            }
            private set { }
        }
        public static bool ListContains(List<dto_category> list, int id, out dto_category o) 
        {
            
            foreach(dto_category cat in list)
            {
                if (cat.category_id == id)
                {
                    o = cat;
                    return true;
                }
            }
            o = null;
            return false;
        }
        public static Dictionary<string, Dictionary<string, List<dto_params>>> Par { get; set; }
        /*public static List<biz_category> CatsTop { get; set; }
        public static List<biz_category> CatsAll { get; set; }
        public static Dictionary<string, Dictionary<string, List<biz_params>>> Par { get; set; }


        public static List<biz_category> s_CategorysTop()
        {
            try
            {
                biz_category cat_poco = new biz_category(new EbazarDB());
                if (CatsTop == null)
                    CatsTop = cat_poco._GetAll(false);
                return CatsTop;
            }
            catch (Exception e)
            {
                CatsTop = null;
                throw e;
            }
        }
        public static List<biz_category> s_CategorysAll()
        {
            try
            {
                biz_category cat_poco = new biz_category(new EbazarDB());
                if (CatsAll == null)
                    CatsAll = cat_poco._GetAll(true);
                return CatsAll;
            }
            catch (Exception e)
            {
                CatsAll = null;
                throw e;
            }
        }/**/
        public static Dictionary<string, Dictionary<string, List<dto_params>>> s_Params()
        {
            try
            {
                if (Par == null)
                {
                    Par = new Dictionary<string, Dictionary<string, List<dto_params>>>();
                    foreach (dto_category cat_a in CatsYesNo)
                    {
                        if (cat_a.params_dto == null)
                        {
                            Par = null;
                            throw new Exception("Param Cat A");
                        }

                        List<dto_params> ps_a = new List<dto_params>();
                        foreach (dto_params par in cat_a.params_dto)
                            ps_a.Add(par);

                        Dictionary<string, List<dto_params>> b = new Dictionary<string, List<dto_params>>();
                        foreach (dto_category cat_b in cat_a.children)
                        {
                            if (cat_b.params_dto == null)
                            {
                                Par = null;
                                throw new Exception("Param Cat B");
                            }
                            List<dto_params> ps_b = new List<dto_params>();
                            List<dto_params> ps_tmp = new List<dto_params>();
                            foreach (dto_params par in cat_b.params_dto)
                                ps_b.Add(par);
                            ps_tmp.AddRange(ps_b);
                            b.Add(cat_b.name, ps_tmp);
                        }
                        Par.Add(cat_a.name, b);
                    }
                }
                return Par;
            }
            catch (Exception e)
            {
                Par = null;
                
                throw e;
            }
        }
    }
}
