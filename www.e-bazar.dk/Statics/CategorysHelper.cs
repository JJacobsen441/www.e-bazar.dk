using System;
using System.Collections.Generic;
using www.e_bazar.dk.Models.DTOs;

namespace www.e_bazar.dk.Statics
{
    public class CategorysHelper
    {
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
