using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Models;

namespace www.e_bazar.dk.Statics
{
    public class NullHelper
    {
        /*
         * booth
         * */
        public static List<conversation> BthNull(List<conversation> c)
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

        public static List<folder> BthNull(List<folder> f)
        {
            foreach (folder _f in f)
            {

                if (_f.booth != null)
                    _f.booth = null;
                if (_f.children != null)
                    _f.children = null;
                if (_f.collection != null)
                    _f.collection = null;
                if (_f.collection1 != null)
                    _f.collection1 = null;
                if (_f.product != null)
                    _f.product = null;
                if (_f.product1 != null)
                    _f.product1 = null;

            }
            return f;
        }

        public static List<boothrating> BthNull(List<boothrating> b)
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

        public static region BthNull(region r)
        {
            if (r.booth != null)
                r.booth = null;

            return r;
        }

        public static List<person> BthNull(List<person> p)
        {
            foreach (person _p in p)
                BthNull(_p);

            return p;
        }

        public static person BthNull(person per)
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

        public static List<product> BthNull(List<product> _p, bool withcats)
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
                    p.category_main = withcats ? p.category_main : null;
                if (p.category_second != null)
                    p.category_second = withcats ? p.category_second : null;
                if (p.collection != null)
                    p.collection = null;
                if (p.conversation != null)
                    p.conversation = null;
                if (p.favorites != null)
                    p.favorites = null;
                if (p.image != null)
                    ;// p.image = null;
                if (p.product_param != null)
                    p.product_param = null;
            }

            return _p;
        }

        public static List<collection> BthNull(List<collection> _p, bool withcats)
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
                    p.category_main = withcats ? p.category_main : null;
                if (p.category_second != null)
                    p.category_second = withcats ? p.category_second : null;
                if (p.product != null)
                    p.product = null;
                if (p.conversation != null)
                    p.conversation = null;
                if (p.favorites != null)
                    p.favorites = null;
                if (p.image != null)
                    ;// p.image = null;
                if (p.collection_param != null)
                    p.collection_param = null;

            }

            return _p;
        }

        public static List<category> BthNull(List<category> c)
        {
            List<category> list = new List<category>();
            foreach (category cat in c)
            {
                list.Add(BthNull(cat));
            }
            return list;
        }

        public static category BthNull(category cat)
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
        public static tag ProNull(tag t)
        {
            if (t.collection != null)
                t.collection = null;
            if (t.product != null)
                t.product = null;

            return t;
        }

        public static person ProNull(person per)
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

        public static List<category> ProNull(List<category> c)
        {
            List<category> list = new List<category>();
            foreach (category cat in c)
            {
                list.Add(ProNull(cat));
            }
            return list;
        }

        public static category ProNull(category cat)
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

        public static booth ProNull(booth bth)
        {
            if (bth.boothrating != null)
                bth.boothrating = null;
            if (bth.category_main != null)
                bth.category_main = ProNull(bth.category_main.ToList());
            if (bth.collection != null)
                bth.collection = null;
            if (bth.conversation != null)
                bth.conversation = null;
            if (bth.foldera != null)
                bth.foldera = null;
            if (bth.followers != null)
                bth.followers = null;
            if (bth.person != null)
                bth.person = ProNull(bth.person);// Null(b.person);
            if (bth.product != null)
                bth.product = null;
            //if (bth.region != null)
            //    bth.region = null;

            return bth;
        }

        public static List<product> ProNull(List<product> _p, bool withbooth, bool withcollection, bool withcats, bool withtags, bool withconversation, bool withfolders)
        {
            foreach (product p in _p)
            {
                ProNull(p, withbooth, withcollection, withcats, withtags, withconversation, withfolders);
            }

            return _p;
        }

        public static product ProNull(product _p, bool withbooth, bool withcollection, bool withcats, bool withtags, bool withconversation, bool withfolders)
        {
            if (_p.booth != null)
                _p.booth = withbooth ? _p.booth : null;
            if (_p.tag != null)
                _p.tag = withtags ? _p.tag : null;
            if (_p.foldera != null)
                _p.foldera = withfolders ? _p.foldera : null;
            if (_p.folderb != null)
                _p.folderb = withfolders ? _p.folderb : null;
            if (_p.category_main != null)
                _p.category_main = withcats ? _p.category_main : null;
            if (_p.category_second != null)
                _p.category_second = withcats ? _p.category_second : null;
            if (_p.collection != null)
                _p.collection = withcollection ? _p.collection : null;
            if (_p.conversation != null)
                _p.conversation = withconversation ? _p.conversation : null;
            if (_p.favorites != null)
                _p.favorites = null;
            if (_p.image != null)
                _p.image = _p.image.OrderBy(i => i.created_on).ToList();
            if (_p.product_param != null)
                _p.product_param = null;
            
            return _p;
        }

        public static List<collection> ProNull(List<collection> _p, bool withbooth, bool withcats, bool withtags, bool withconversation, bool withfolders, bool withproducts)
        {
            foreach (collection p in _p)
            {
                ProNull(p, withbooth, withcats, withtags, withconversation, withfolders, withproducts);
            }

            return _p;
        }

        public static collection ProNull(collection _p, bool withbooth, bool withcats, bool withtags, bool withconversation, bool withfolders, bool withproducts)
        {
            if (_p.booth != null)
                _p.booth = withbooth ? _p.booth : null;
            if (_p.tag != null)
                _p.tag = withtags ? _p.tag : null;
            if (_p.foldera != null)
                _p.foldera = withfolders ? _p.foldera : null;
            if (_p.folderb != null)
                _p.folderb = withfolders ? _p.folderb : null;
            if (_p.category_main != null)
                _p.category_main = withcats ? _p.category_main : null;
            if (_p.category_second != null)
                _p.category_second = withcats ? _p.category_second : null;
            if (_p.product != null)
                _p.product = withproducts ? ProNull(_p.product.ToList(), false, false, false, false, false, false) : null;
            if (_p.conversation != null)
                _p.conversation = withconversation ? _p.conversation : null;
            if (_p.favorites != null)
                _p.favorites = null;
            if (_p.image != null)
                _p.image = _p.image.OrderBy(i => i.created_on).ToList();
            if (_p.collection_param != null)
                _p.collection_param = null;

            return _p;
        }








        /*
         * person
         * */

        public static person PerNull(person per, bool withbooth, bool withfavorites, bool withfollowing)
        {
            if (per.booth != null)
                per.booth = withbooth ? per.booth : null;
            if (per.boothrating != null)
                per.boothrating = null;
            if (per.comment != null)
                per.comment = null;
            if (per.conversation != null)
                per.conversation = null;
            if (per.favorites_collection != null)
                per.favorites_collection = withfavorites ? per.favorites_collection : null;
            if (per.favorites_product != null)
                per.favorites_product = withfavorites ? per.favorites_product : null;
            if (per.following != null)
                per.following = withfollowing ? per.following : null;

            return per;
        }

        public static List<product> PerNull(List<product> p, bool nullbooth)
        {
            List<product> list = new List<product>();
            foreach (product pro in p)
            {
                if (nullbooth)
                    pro.booth = null;
                if (pro.booth != null)
                    pro.booth = PerNull(pro.booth);
                if (pro.category_main != null)
                    pro.category_main = null;
                if (pro.category_second != null)
                    pro.category_second = null;
                if (pro.collection != null)
                    pro.collection = null;
                if (pro.conversation != null)
                    pro.conversation = null;
                if (pro.favorites != null)
                    pro.favorites = null;
                if (pro.foldera != null)
                    pro.foldera = null;
                if (pro.folderb != null)
                    pro.folderb = null;
                if (pro.image != null)
                    pro.image = null;
                if (pro.product_param != null)
                    pro.product_param = null;
                if (pro.tag != null)
                    pro.tag = null;
                list.Add(pro);
            }
            return list;
        }

        public static List<collection> PerNull(List<collection> c, bool nullbooth)
        {
            List<collection> list = new List<collection>();
            foreach (collection col in c)
            {
                if (nullbooth)
                    col.booth = null;
                if (col.booth != null)
                    col.booth = PerNull(col.booth);
                if (col.category_main != null)
                    col.category_main = null;
                if (col.category_second != null)
                    col.category_second = null;
                if (col.product != null)
                    col.product = null;
                if (col.conversation != null)
                    col.conversation = null;
                if (col.favorites != null)
                    col.favorites = null;
                if (col.foldera != null)
                    col.foldera = null;
                if (col.folderb != null)
                    col.folderb = null;
                if (col.image != null)
                    col.image = null;
                if (col.collection_param != null)
                    col.collection_param = null;
                if (col.tag != null)
                    col.tag = null;
                list.Add(col);
            }
            return list;
        }

        public static List<booth> PerNull(List<booth> b)
        {
            List<booth> list = new List<booth>();
            foreach (booth bth in b)
            {
                list.Add(PerNull(bth));
            }
            return list;
        }

        public static booth PerNull(booth bth)
        {
            if (bth.boothrating != null)
                bth.boothrating = null;
            if (bth.category_main != null)
                bth.category_main = null;
            if (bth.collection != null)
                bth.collection = null;
            if (bth.conversation != null)
                bth.conversation = null;
            if (bth.foldera != null)
                bth.foldera = null;
            if (bth.followers != null)
                bth.followers = null;
            if (bth.person != null)
                bth.person = null;// Null(b.person);
            if (bth.product != null)
                bth.product = null;
            if (bth.region != null)
                bth.region = null;

            return bth;
        }








        /*
         * comment
         * */

        public static person ComNull(person per)
        {
            if (per.booth != null)
                per.booth = null;
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







        /*
         * conversation
         * */

        public static person ConNull(person per)
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

        public static booth ConNull(booth b)
        {
            if (b.boothrating != null)
                b.boothrating = null;
            if (b.category_main != null)
                b.category_main = null;
            if (b.collection != null)
                b.collection = null;
            if (b.conversation != null)
                b.conversation = null;
            if (b.foldera != null)
                b.foldera = null;
            if (b.followers != null)
                b.followers = null;
            if (b.person != null)
                b.person = ConNull(b.person);
            if (b.product != null)
                b.product = null;
            if (b.region != null)
                b.region = null;
            return b;
        }

        public static product ConNull(product p)
        {
            //if (p.booth != null)
            //    p.booth = null;
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
            if (p.foldera != null)
                p.foldera = null;
            if (p.folderb != null)
                p.folderb = null;
            if (p.image != null)
                p.image = null;
            if (p.product_param != null)
                p.product_param = null;
            return p;
        }

        public static collection ConNull(collection c)
        {
            //if (c.booth != null)
            //    c.booth = null;
            if (c.category_main != null)
                c.category_main = null;
            if (c.category_second != null)
                c.category_second = null;
            if (c.product != null)
                c.product = null;
            if (c.conversation != null)
                c.conversation = null;
            if (c.favorites != null)
                c.favorites = null;
            if (c.foldera != null)
                c.foldera = null;
            if (c.folderb != null)
                c.folderb = null;
            if (c.image != null)
                c.image = null;
            if (c.collection_param != null)
                c.collection_param = null;
            return c;
        }
    }
}