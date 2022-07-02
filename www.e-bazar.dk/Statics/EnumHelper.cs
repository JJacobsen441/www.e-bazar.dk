using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace www.e_bazar.dk.Statics
{
    public static class EnumHelper
    {
        public static string GetDescription<TEnum>(this TEnum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            if (fi != null)
            {
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
            }

            return value.ToString();
        }

        public static SelectList SelectListFor<T>() where T : struct
        {
            Type t = typeof(T);
            return !t.IsEnum ? null : new SelectList(BuildSelectListItems(t), "Value", "Text");
        }

        public static SelectList SelectListFor<T>(T selected) where T : struct
        {
            Type t = typeof(T);
            return !t.IsEnum ? null : new SelectList(BuildSelectListItems(t), "Text", "Value", selected.ToString());
        }

        private static IEnumerable<SelectListItem> BuildSelectListItems(Type t)
        {
            return Enum.GetValues(t)
                       .Cast<Enum>()
                       .Select(e => new SelectListItem { Value = e.ToString(), Text = e.GetDescription() });
        }
    }
}