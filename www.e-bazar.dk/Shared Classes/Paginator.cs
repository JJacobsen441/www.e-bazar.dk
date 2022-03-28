using System;
using System.Web;


namespace www.e_bazar.dk.SharedClasses
{
    public class Paginator
    {
        private static int pages;
        public Paginator(int list_len, int items_per_page)
        {
            

            pages = list_len % items_per_page > 0 ? list_len / items_per_page + 1 : list_len / items_per_page;
            if (ThisSession.Cookie)
            {
                if (ThisSession.Paginator == -1)
                    GotoPage(1);
            }
        }
        public int GetPage()
        {
            if (!ThisSession.Cookie)
                return 1;
            if (ThisSession.Paginator == -1)
                GotoPage(1);
            return ThisSession.Paginator;
        }
        public void GotoPage(int index)
        {
            if (!ThisSession.Cookie)
                return;
            ThisSession.Paginator = index;
        }
        private static bool IsEnd(int current)
        {
            return current < pages ? false : true;
        }

        public static IHtmlString CreatePaginator(string cssclassdiv, string cssclassspan)
        {
            string result = "";

            try
            {
                string path = HttpContext.Current.Request.Url.AbsolutePath;
                string previous;
                string next;
                string middle;

                int paginator = ThisSession.Paginator;
                string s = ThisSession.Search;
                string c = ThisSession.Category;
                int z = ThisSession.Zip;
                int f = ThisSession.Fra;
                int t = ThisSession.Til;

                CreateElements(paginator, s, c, z, f, t, path, out previous, out next, out middle);
                CreateTag(paginator, previous, next, middle, cssclassdiv, cssclassspan, out result);
            }
            catch (NullReferenceException nre)
            {
                ;
            }

            return new HtmlString(result);
        }
        public static void CreateElements(int paginator, string s, string c, int z, int f, int t, string path, out string previous, out string next, out string middle)
        {
            previous = "<a href='" + path + "?s=" + s + "&c=" + c + "&z=" + z + "&f=" + f + "&t=" + t + "&page=" + (paginator - 1) + "' class='postback myfade'>&lt&lt </a>";
            next = "<a href='" + path + "?s=" + s + "&c=" + c + "&z=" + z + "&f=" + f + "&t=" + t + "&page=" + (paginator + 1) + "' class='postback myfade'> &gt&gt</a>";
            middle = "";

            string text = "";
            for (int i = 1; i < pages + 1; i++)
            {
                text = i == paginator ? "<b>" + i + "</b>" : i.ToString();
                middle += "<a href='" + path + "?s=" + s + "&c=" + c + "&z=" + z + "&f=" + f + "&t=" + t + "&page=" + i + "' class='postback myfade'> " + text + " </a>";
            }
        }
        public static void CreateTag(int paginator, string previous, string next, string middle, string cssclassdiv, string cssclassspan, out string result)
        {
            if (IsEnd(paginator))
                result = previous + middle;
            else if (paginator > 1)
                result = previous + middle + next;
            else
                result = middle + next;
            result = "<div class=\"" + cssclassdiv + "\"><span class=\"" + cssclassspan + "\">" + result + "</span></div>";
        }
    }
}