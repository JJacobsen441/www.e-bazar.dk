using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Statics
{
    public static class Extensions
    {
        public static MvcHtmlString ActionImage(this HtmlHelper html, string action, string controllerName, object routeValues, string imagePath, string data_src, string alt = null, string[] classes_img = null, string[] classes_a = null)
        {
            var url = new UrlHelper(html.ViewContext.RequestContext);

            // build the <img> tag
            var imgBuilder = new TagBuilder("img");
            if (imagePath != null)
                imgBuilder.MergeAttribute("src", url.Content(imagePath));
            if (data_src != null)
                imgBuilder.MergeAttribute("data-src", url.Content(data_src));
            if (alt != null)
                imgBuilder.MergeAttribute("alt", alt);
            string classes_img_string = "";
            foreach (string cssclass in classes_img)
            {
                if (cssclass != null)
                    classes_img_string += " " + cssclass;
            }
            imgBuilder.MergeAttribute("class", classes_img_string);
            string imgHtml = imgBuilder.ToString(TagRenderMode.SelfClosing);

            // build the <a> tag
            var anchorBuilder = new TagBuilder("a");
            if (action != "")
                anchorBuilder.MergeAttribute("href", url.Action(action, controllerName, routeValues));
            else
                anchorBuilder.MergeAttribute("href", "#/");
            if (classes_a != null)
            {
                string classes_a_string = "";
                foreach (string cssclass in classes_a)
                {
                    if (cssclass != null)
                        classes_a_string += " " + cssclass;
                }
                anchorBuilder.MergeAttribute("class", classes_a_string);
            }
            anchorBuilder.InnerHtml = imgHtml; // include the <img> tag inside
            string anchorHtml = anchorBuilder.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(anchorHtml);
        }

        private static string FixFolders(string path, string remove, bool tosystem) 
        {
            string nd = Path.DirectorySeparatorChar.ToString();
            string tmp = path;
            if (tmp.StartsWith("~"))
                tmp = tmp.Substring(1, tmp.Length - 1);
            if (tmp.StartsWith("/"))
                tmp = tmp.Substring(1, tmp.Length - 1);
            if (tmp.StartsWith(remove + "/"))
                tmp = tmp.Replace(remove + "/", "");
            if (tosystem)
                tmp = tmp.Replace("/", nd);
            return tmp;
        }

        public static MvcHtmlString RouteImage(this HtmlHelper html, string routename, object routeValues, string imagePath, string data_src, string alt, string[] classes_img, string[] classes_anchor, string style, string style_a)
        {
            try
            {
                if (!File.Exists(StaticsHelper.Content + FixFolders(imagePath, "_content", true)))
                    imagePath = alt;
            }
            catch (Exception e) { imagePath = alt; }

            var url = new UrlHelper(html.ViewContext.RequestContext);

            // build the <img> tag
            var imgBuilder = new TagBuilder("img");
            if (!string.IsNullOrEmpty(imagePath))
                imgBuilder.MergeAttribute("src", url.Content(imagePath));
            if (!string.IsNullOrEmpty(data_src))
                imgBuilder.MergeAttribute("data-src", url.Content(data_src));
            if (!string.IsNullOrEmpty(alt))
                imgBuilder.MergeAttribute("alt", url.Content(alt));
            imgBuilder.MergeAttribute("z-loading", "lazy");
            if(classes_img != null)
            {
                string classes_img_string = "";
                foreach (string cssclass in classes_img)
                    classes_img_string += " " + cssclass;
                imgBuilder.MergeAttribute("class", classes_img_string);                
            }
            if (!string.IsNullOrEmpty(style))
                imgBuilder.MergeAttribute("style", style);
            string imgHtml = imgBuilder.ToString(TagRenderMode.SelfClosing);

            // build the <a> tag
            var anchorBuilder = new TagBuilder("a");
            if (routename != "")
                anchorBuilder.MergeAttribute("href", url.RouteUrl(routename, routeValues));
            else
                anchorBuilder.MergeAttribute("href", "#/");
            if (classes_anchor != null)
            {
                string classes_a_string = "";
                foreach (string cssclass in classes_anchor)
                    classes_a_string += " " + cssclass;
                anchorBuilder.MergeAttribute("class", classes_a_string);
            }
            if (!string.IsNullOrEmpty(style_a))
                anchorBuilder.MergeAttribute("style", style_a);
            anchorBuilder.InnerHtml = imgHtml; // include the <img> tag inside
            string anchorHtml = anchorBuilder.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(anchorHtml);
        }

        public static MvcHtmlString ActionCheckBox(this HtmlHelper html, string name, bool isChecked, string action, string controllerName, object routeValues, string[] classes = null)
        {
            var url = new UrlHelper(html.ViewContext.RequestContext);

            // build the <img> tag
            var checkBuilder = new TagBuilder("input");
            checkBuilder.MergeAttribute("type", "checkbox");
            if (name != null)
                checkBuilder.MergeAttribute("id", name);

            if(isChecked)
                checkBuilder.MergeAttribute("checked", "checked");
            
            string classes_string = "";
            foreach (string cssclass in classes)
            {
                if (cssclass != null)
                    classes_string += " " + cssclass;
            }
            checkBuilder.MergeAttribute("class", classes_string);
            checkBuilder.MergeAttribute("onclick", "location.href='" + url.Action(action, controllerName, routeValues) + "'");// + "?c=" + routeValues);
            string checkHtml = checkBuilder.ToString(TagRenderMode.SelfClosing);

            // build the <a> tag
            //var anchorBuilder = new TagBuilder("a");
            //anchorBuilder.MergeAttribute("href", url.Action(action, controllerName, routeValues));
            //anchorBuilder.InnerHtml = checkHtml; // include the <img> tag inside
            //string anchorHtml = anchorBuilder.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(checkHtml);
        }

        public static MvcHtmlString DisplayWithBreaksFor(this HtmlHelper html, string text/*, Expression<Func<TModel, TValue>> expression*/)
        {
            //var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            //string model = html.Encode(metadata.Model).Replace(Environment.NewLine, "<br />");

            //if (string.IsNullOrEmpty(model))
            //    return MvcHtmlString.Empty;

            if (string.IsNullOrEmpty(text))
                return MvcHtmlString.Empty;

            //var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string model = "" + html.Raw(text.Replace(Environment.NewLine, "<br />"));


            return MvcHtmlString.Create(model);
        }

        public static string HtmlEncode(string html)
        {
            if(!string.IsNullOrEmpty(html))
            {
                var httpUtil = new HttpServerUtilityWrapper(HttpContext.Current.Server);
                //string encoded = httpUtil.HtmlEncode(html).Replace("\r\n", "<br />\r\n");
                string encoded = httpUtil.HtmlEncode(html).Replace(Environment.NewLine, "<br />");

                //if (String.IsNullOrEmpty(encoded))
                //    return MvcHtmlString.Empty;

                //return MvcHtmlString.Create(encoded);
                return encoded;
            }
            return "";
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            Random rnd = new Random();
            return source.OrderBy<T, int>((item) => rnd.Next());
        }

        public static bool IsNull<T>(this T source)
        {
            return source == null;
        }

        public static bool IsNotNull<T>(this T source)
        {
            return source != null;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> value) where T : class
        {
            return value == null || value.Count() == 0;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return value == null || value.Length == 0;
        }

        /*public static string ToTraceStringA<T>(this IQueryable<T> t)
        {
            var sqlC = t.ToString();

            string sql = "";
            ObjectQuery<T> oqt = t as ObjectQuery<T>;
            if (oqt != null)
                sql = oqt.ToTraceString();
            return sql;
        }
        public static string ToTraceStringB<T>(this IQueryable<T> t)
        {
            string sql = t.ToString();
            return sql;
        }

        public static string ToTraceStringC<T>(this IQueryable<T> t)
        {
            var sqlC = t.ToString();

            string sql = "";
            ObjectQuery<T> oqt = t as ObjectQuery<T>;
            if (oqt != null)
                sql = oqt.ToTraceString();
            return sql;
        }/**/

        public static void ToTraceStringD<T>(this IQueryable<T> t)
        {
            //var sqlA = ((System.Data.Objects.ObjectQuery)t).ToTraceString();

            //or in EF6:

            //var sqlB = ((System.Data.Entity.Core.Objects.ObjectQuery)t).ToTraceString();
            var sqlC = t.ToString();
            System.Diagnostics.Trace.WriteLine(t.ToString());
        }
    }
}