using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using www.e_bazar.dk.Models;

namespace www.e_bazar.dk.Extensions
{
    public class NullHelper
    {
        /*
         * booth
         * */
        public static List<conversation> BNull(List<conversation> c)
        {
            foreach (conversation _c in c)
            {

                if (_c.booth != null)
                    _c.booth = null;
                if (_c.product != null)
                    _c.product = null;
                if (_c.collection != null)
                    _c.collection = null;
                if (_c.person != null)
                    _c.person = null;
                if (_c.comment != null)
                    _c.comment = null;
            }

            return c;
        }

        public static List<folder> BNull(List<folder> f)
        {
            foreach (folder _f in f)
            {

                if (_f.booth != null)
                    _f.booth = null;
                if (_f.children != null)
                    _f.children = null;
                //if (_f.collection != null)
                //    _f.collection = null;
                //if (_f.collection1 != null)
                //    _f.collection1 = null;
                //if (_f.product != null)
                //    _f.product = null;
                //if (_f.product1 != null)
                //    _f.product1 = null;

            }
            return f;
        }

        public static List<boothrating> BNull(List<boothrating> b)
        {
            foreach (boothrating _b in b)
            {

                if (_b.booth != null)
                    _b.booth = null;
                if (_b.person != null)
                    _b.person = null;
            }

            return b;
        }

        public static region BNull(region r)
        {
            if (r.booth != null)
                r.booth = null;

            return r;
        }

        public static List<person> BNull(List<person> p)
        {
            foreach (person _p in p)
                BNull(_p);

            return p;
        }

        public static person BNull(person per)
        {
            if (per.booth != null)
                per.booth = null;
            if (per.boothrating != null)
                per.boothrating = null;
            if (per.comment != null)
                per.comment = null;
            if (per.conversation != null)
                per.conversation = null;
            if (per.favorites_collection != null)
                per.favorites_collection = null;
            if (per.favorites_product != null)
                per.favorites_product = null;
            if (per.following != null)
                per.following = null;

            return per;
        }

        public static List<product> BNull(List<product> _p)
        {
            foreach (product p in _p)
            {
                if (p.booth != null)
                    p.booth = null;
                if (p.tag != null)
                    p.tag = null;
                if (p.foldera != null)
                    p.foldera = null;
                if (p.folderb != null)
                    p.folderb = null;
                if (p.category_main != null)
                    p.category_main = null;
                if (p.category_second != null)
                    p.category_second = null;
                if (p.collection != null)
                    p.collection = null;
                if (p.conversation != null)
                    p.conversation = null;
                if (p.favorites != null)
                    p.favorites = null;
                //if (p.image != null)
                //    p.image = null;
                if (p.product_param != null)
                    p.product_param = null;
            }

            return _p;
        }

        public static List<collection> BNull(List<collection> _p)
        {
            foreach (collection p in _p)
            {
                if (p.booth != null)
                    p.booth = null;
                if (p.tag != null)
                    p.tag = null;
                if (p.foldera != null)
                    p.foldera = null;
                if (p.folderb != null)
                    p.folderb = null;
                if (p.category_main != null)
                    p.category_main = null;
                if (p.category_second != null)
                    p.category_second = null;
                if (p.product != null)
                    p.product = null;
                if (p.conversation != null)
                    p.conversation = null;
                if (p.favorites != null)
                    p.favorites = null;
                //if (p.image != null)
                //    p.image = null;
                if (p.collection_param != null)
                    p.collection_param = null;

            }

            return _p;
        }

        public static List<category> BNull(List<category> c)
        {
            List<category> list = new List<category>();
            foreach (category cat in c)
            {
                list.Add(BNull(cat));
            }
            return list;
        }

        public static category BNull(category cat)
        {
            if (cat.booth != null)
                cat.booth = null;
            if (cat.children != null)
                cat.children = null;
            if (cat.collection_main != null)
                cat.collection_main = null;
            if (cat.collection_second != null)
                cat.collection_second = null;
            if (cat.param != null)
                cat.param = null;
            if (cat.parent != null)
                cat.parent = null;
            if (cat.product_main != null)
                cat.product_main = null;
            if (cat.product_second != null)
                cat.product_second = null;

            return cat;
        }

        /*
         * product and collection
         * */
        public static tag PNull(tag t)
        {
            if (t.collection != null)
                t.collection = null;
            if (t.product != null)
                t.product = null;

            return t;
        }

        public static person PNull(person per)
        {
            if (per.booth != null)
                per.booth = null;
            if (per.boothrating != null)
                per.boothrating = null;
            if (per.comment != null)
                per.comment = null;
            if (per.conversation != null)
                per.conversation = null;
            if (per.favorites_collection != null)
                per.favorites_collection = null;
            if (per.favorites_product != null)
                per.favorites_product = null;
            if (per.following != null)
                per.following = null;

            return per;
        }

        public static List<category> PNull(List<category> c)
        {
            List<category> list = new List<category>();
            foreach (category cat in c)
            {
                list.Add(PNull(cat));
            }
            return list;
        }

        public static category PNull(category cat)
        {
            if (cat.booth != null)
                cat.booth = null;
            //if (cat.children != null)
            //    cat.children = null;
            if (cat.collection_main != null)
                cat.collection_main = null;
            if (cat.collection_second != null)
                cat.collection_second = null;
            //if (cat.param != null)
            //    cat.param = null;
            if (cat.parent != null)
                cat.parent = null;
            if (cat.product_main != null)
                cat.product_main = null;
            if (cat.product_second != null)
                cat.product_second = null;

            return cat;
        }

        public static booth PNull(booth bth)
        {
            if (bth.boothrating != null)
                bth.boothrating = null;
            if (bth.category_main != null)
                bth.category_main = PNull(bth.category_main.ToList());
            if (bth.collection != null)
                bth.collection = null;
            if (bth.conversation != null)
                bth.conversation = null;
            if (bth.foldera != null)
                bth.foldera = null;
            if (bth.followers != null)
                bth.followers = null;
            if (bth.person != null)
                bth.person = PNull(bth.person);// Null(b.person);
            if (bth.product != null)
                bth.product = null;
            //if (bth.region != null)
            //    bth.region = null;

            return bth;
        }
    }
}