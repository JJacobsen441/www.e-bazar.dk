using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;
using static www.e_bazar.dk.Models.ViewModels.ViewModels;

namespace www.e_bazar.dk.Statics
{
    public class CheckHelper
    {
        public class Generel
        {
            public static bool CheckMarketPlace(MarketplaceViewModel model, out string _s, out string _c_orig, out int _z, out int _t, out int _f, out bool _g)
            {
                if (model.IsNull())
                    throw new Exception();

                model.a = CheckHelper.Generel.FormatString(model.a, true, false, false, "no_tag", CharacterHelper.VeryLimited(false));
                model.s = CheckHelper.Generel.FormatString(model.s, true, false, true, "no_tag", CharacterHelper.VeryLimited(false));
                model.c = CheckHelper.Generel.FormatString(model.c, false, false, true, "no_tag", CharacterHelper.VeryLimited(false));
                model.p = CheckHelper.Generel.FormatString(model.p, true, false, false, "no_tag", CharacterHelper.Param());
                model.z = CheckHelper.Generel.FormatString(model.z, true, false, false, "no_tag", CharacterHelper.VeryLimited(false));
                model.f = CheckHelper.Generel.FormatString(model.f, true, false, false, "no_tag", CharacterHelper.VeryLimited(false));
                model.t = CheckHelper.Generel.FormatString(model.t, true, false, false, "no_tag", CharacterHelper.VeryLimited(false));
                model.gra = CheckHelper.Generel.FormatString(model.gra, true, false, false, "no_tag", CharacterHelper.VeryLimited(false));

                _s = model.s;
                _c_orig = model.c;

                if (string.IsNullOrEmpty(model.c))
                    model.c = "alle";

                _z = int.TryParse(model.z, out _z) && _z >= 0 && _z <= 10000 ? _z : 0;
                _z = AreasHelper.selected.Contains("dk") ? _z : 0;
                _t = int.TryParse(model.t, out _t) && _t >= 0 && _t <= 999999 ? _t : 999999;
                _f = int.TryParse(model.f, out _f) && _f >= 0 && _f <= _t ? _f : 0;
                _g = model.gra == "true";

                return true;
            }

            public static bool CheckBooth(BoothViewModel model)
            {
                if (model.IsNull())
                    throw new Exception();

                if ((model.a_sub = CheckHelper.Generel.FormatString(model.a_sub, true, false, false, "no_tag", CharacterHelper.VeryLimited(true))) == null)
                    throw new Exception();

                if ((model.b_sub = CheckHelper.Generel.FormatString(model.b_sub, true, false, false, "no_tag", CharacterHelper.VeryLimited(true))) == null)
                    throw new Exception();
    
                model.a_sub = model.a_sub.Replace("_", " ");
                model.b_sub = model.b_sub.Replace("_", " ");

                return true;
            }

            public static string FormatString(string str, bool to_lower, bool allow_newline, bool allow_upper, string allow_tag, char[] allowed)
            {
                if (str.IsNull())
                    return "";

                //str = HttpUtility.UrlDecode(str);
                //str = HttpUtility.HtmlDecode(str);

                str = to_lower ? str.ToLower() : str;
                str = str.Trim();

                bool ok;
                str = StringHelper.OnlyAlphanumeric(str, allow_newline, allow_upper, "no_tag", allowed, out ok);
                if (!ok)
                    throw new Exception();

                return str;
            }

            public static bool IsValidEmail(string email)
            {
                try
                {
                    if (string.IsNullOrEmpty(email))
                        return false;
                    var addr = new System.Net.Mail.MailAddress(email);
                    return addr.Address == email;
                }
                catch
                {
                    return false;
                }
            }

            public static bool IsAdmin(string ip)
            {
                if (string.IsNullOrEmpty(ip))
                    return false;

                string set_ip = SettingsHelper.Basic.IP().Trim();
                ip = ip.Trim();
                if (StaticsHelper.IsDebug)
                    return ip == "::1" || ip == "127.0.0.1" || ip == set_ip;

                return ip == set_ip;
            }

            public static bool IsPhonenumber(int? nr)
            {
                if (nr != null && nr > 0 && nr <= 99999999 && nr.ToString().Count() == 8)
                    return true;
                return false;
            }

            public static bool IsPhonenumber(string nr)
            {
                int n;
                bool ok = int.TryParse(nr, out n);
                if (ok && n > 0 && n <= 99999999 && n.ToString().Count() == 8)
                    return true;
                return false;
            }

            public static bool IsAmount(ref string str)
            {
                int tmp;
                bool parse_ok = int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out tmp);
                if (parse_ok && tmp >= 0)
                {
                    str = "" + tmp;
                    return true;
                }
                return false;
            }
        }

        public static string ProductPrice(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            str = str.Replace(".", ",");
            str = str.Split(',')[0];
            if (!string.IsNullOrEmpty(str) && (str == NOP.INGEN_PRIS.ToString() || CheckHelper.Generel.IsAmount(ref str)))
            {
                err["PRODUCT_PRICE"] = ERROR_MESSAGE.OK;
                //if (str =="0")
                //    str = NOP.INGEN_PRIS.ToString();
                return str;
            }
            err["PRODUCT_PRICE"] = ERROR_MESSAGE.PRODUCT_PRICE;
            return NOP.INGEN_PRIS.ToString();
        }

        public static string ProductCategory(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (str != "-1")
            {
                bool ok;
                err["PRODUCT_CATEGORY"] = ERROR_MESSAGE.OK;
                return StringHelper.OnlyAlphanumeric(str, false, true, "notag", new char[] { }, out ok);
            }
            err["PRODUCT_CATEGORY"] = ERROR_MESSAGE.PRODUCT_CATEGORY;
            return TextHelper.GetNopValue(NOP.UDFYLD.ToString());
        }

        public static string ProductName(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str) && str != TextHelper.GetNopValue(NOP.UDFYLD.ToString()))
            {
                bool ok;
                err["PRODUCT_NAME"] = ERROR_MESSAGE.OK;
                str = StringHelper.OnlyAlphanumeric(str, false, true, "notag", CharacterHelper.Limited(false), out ok);
                return str;
            }
            err["PRODUCT_NAME"] = ERROR_MESSAGE.PRODUCT_NAME;
            return TextHelper.GetNopValue(NOP.UDFYLD.ToString());
        }

        public static string ProductNote(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str))
            {
                err["PRODUCT_NOTE"] = ERROR_MESSAGE.OK;
                return str;
            }
            err["PRODUCT_NOTE"] = ERROR_MESSAGE.OK;
            return "";
        }

        public static string ProductDescription(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str))
            {
                err["PRODUCT_DESCRIPTION"] = ERROR_MESSAGE.OK;
                bool ok;
                str = StringHelper.OnlyAlphanumeric(str, true, true, "notag", CharacterHelper.All(true), out ok);
                return str;
            }
            err["PRODUCT_DESCRIPTION"] = ERROR_MESSAGE.OK;
            return "";
        }

        public static int ProductNoOfUnits(string nou, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            int res;
            bool tmp = int.TryParse(nou, out res);
            if (tmp && res > 0)
            {
                err["PRODUCT_NOOFUNITS"] = ERROR_MESSAGE.OK;
                return res;
            }
            err["PRODUCT_NOOFUNITS"] = ERROR_MESSAGE.PRODUCT_NOOFUNITS;
            return 0;//dette er jo heller ikke rigtigt
        }

        //public static bool CreateProduct(biz_product product_poco, ref Dictionary<string, ERROR_MESSAGE> err)
        //{
        //    if (product_poco.booth_poco.category_main == null || product_poco.booth_poco.category_main.Count <= 0)
        //    {
        //        err["BOOTH_TAGS"] = ERROR_MESSAGE.BOOTH_TAGS;
        //        return false;
        //    }
        //    err["BOOTH_TAGS"] = ERROR_MESSAGE.OK;
        //    return true;
        //}

        public static string CollectionPrice(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            str = str.Replace(".", ",");
            str = str.Split(',')[0];
            if (!string.IsNullOrEmpty(str) && (str == NOP.INGEN_PRIS.ToString() || CheckHelper.Generel.IsAmount(ref str)))
            {
                err["COLLECTION_PRICE"] = ERROR_MESSAGE.OK;
                //if (str =="0")
                //    str = NOP.INGEN_PRIS.ToString();
                return str;
            }
            err["COLLECTION_PRICE"] = ERROR_MESSAGE.COLLECTION_PRICE;
            return NOP.INGEN_PRIS.ToString();
        }

        public static string CollectionCategory(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (str != "-1")
            {
                bool ok;
                err["COLLECTION_CATEGORY"] = ERROR_MESSAGE.OK;
                return StringHelper.OnlyAlphanumeric(str, false, true, "notag", new char[] { }, out ok);
            }
            err["COLLECTION_CATEGORY"] = ERROR_MESSAGE.COLLECTION_CATEGORY;
            return TextHelper.GetNopValue(NOP.UDFYLD.ToString());
        }

        public static string CollectionName(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str) && str != TextHelper.GetNopValue(NOP.UDFYLD.ToString()))
            {
                bool ok;
                err["COLLECTION_NAME"] = ERROR_MESSAGE.OK;
                str = StringHelper.OnlyAlphanumeric(str, false, true, "notag", CharacterHelper.Limited(false), out ok);
                return str;
            }
            err["COLLECTION_NAME"] = ERROR_MESSAGE.COLLECTION_NAME;
            return TextHelper.GetNopValue(NOP.UDFYLD.ToString());
        }

        public static string CollectionNote(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str))
            {
                err["COLLECTION_NOTE"] = ERROR_MESSAGE.OK;
                return str;
            }
            err["COLLECTION_NOTE"] = ERROR_MESSAGE.OK;
            return "";
        }

        public static string CollectionDescription(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str))
            {
                err["COLLECTION_DESCRIPTION"] = ERROR_MESSAGE.OK;
                bool ok;
                str = StringHelper.OnlyAlphanumeric(str, true, true, "notag", CharacterHelper.All(true), out ok);
                return str;
            }
            err["COLLECTION_DESCRIPTION"] = ERROR_MESSAGE.OK;
            return "";
        }

        //public static bool CreateCollection(biz_collection collection_poco, ref Dictionary<string, ERROR_MESSAGE> err)
        //{
        //    if (collection_poco.booth_poco.category_main == null || collection_poco.booth_poco.category_main.Count <= 0)
        //    {
        //        err["BOOTH_TAGS"] = ERROR_MESSAGE.BOOTH_TAGS;
        //        return false;
        //    }
        //    err["BOOTH_TAGS"] = ERROR_MESSAGE.OK;
        //    return true;
        //}

        public static string BoothName(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str) && str != TextHelper.GetNopValue(NOP.UDFYLD.ToString()))
            {
                err["BOOTH_NAME"] = ERROR_MESSAGE.OK;
                bool ok;
                str = StringHelper.OnlyAlphanumeric(str, false, true, "notag", CharacterHelper.Limited(false), out ok);
                return str;
            }
            err["BOOTH_NAME"] = ERROR_MESSAGE.BOOTH_NAME;
            return TextHelper.GetNopValue(NOP.UDFYLD.ToString());
        }

        public static string BoothDescription(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str))
            {
                err["BOOTH_DESCRIPTION"] = ERROR_MESSAGE.OK;
                bool ok;
                str = StringHelper.OnlyAlphanumeric(str, true, true, "notag", CharacterHelper.All(true), out ok);
                return str;
            }
            err["BOOTH_DESCRIPTION"] = ERROR_MESSAGE.OK;
            return "";
        }

        public static string ProfileFirstname(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str) && str != TextHelper.GetNopValue(NOP.UDFYLD.ToString()))
            {
                bool ok;
                err["PROFILE_FIRSTNAME"] = ERROR_MESSAGE.OK;
                str = StringHelper.OnlyAlphanumeric(str, false, true, "notag", CharacterHelper.Name(), out ok);
                return str;
            }
            err["PROFILE_FIRSTNAME"] = ERROR_MESSAGE.PROFILE_FIRSTNAME;
            return "";
        }

        public static string ProfileLastname(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            str = string.IsNullOrEmpty(str) ? "********" : str;
            if (!string.IsNullOrEmpty(str) && str != TextHelper.GetNopValue(NOP.UDFYLD.ToString()))
            {
                bool ok;
                err["PROFILE_LASTNAME"] = ERROR_MESSAGE.OK;
                str = StringHelper.OnlyAlphanumeric(str, false, true, "notag", CharacterHelper.Name(), out ok);
                return str;
            }
            err["PROFILE_LASTNAME"] = ERROR_MESSAGE.PROFILE_LASTNAME;
            return "";
        }

        public static int? ProfilePhonenumber(int? nr, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (nr != null && !string.IsNullOrEmpty(nr.ToString()) && nr.ToString().Count() == 8)
            {
                err["PROFILE_PHONENUMBER"] = ERROR_MESSAGE.OK;
                return nr;
            }
            err["PROFILE_PHONENUMBER"] = ERROR_MESSAGE.PROFILE_PHONENUMBER;
            return null;
        }

        public static string ProfileEmail(string str, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if (!string.IsNullOrEmpty(str) && str != TextHelper.GetNopValue(NOP.UDFYLD.ToString()) && CheckHelper.Generel.IsValidEmail(str))
            {
                err["PROFILE_EMAIL"] = ERROR_MESSAGE.OK;
                return StringHelper._RemoveCharacters(str, new char[] { ' ' });
            }
            err["PROFILE_EMAIL"] = ERROR_MESSAGE.PROFILE_EMAIL;
            return "";
        }

        public static string ProfileDescription(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                bool ok;
                str = StringHelper.OnlyAlphanumeric(str, true, true, "notag", CharacterHelper.All(true), out ok);
                return str;
            }
            return "";
        }

        public static bool PartAddress(dto_booth model, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            model.region_dto.town = model.region_dto.town;
            model.region_dto.zip = model.region_dto.zip;
            
            if (!string.IsNullOrEmpty(model.region_dto.town) && model.region_dto.town != "(Ikke angivet.)" && model.region_dto.zip > 0 && model.region_dto.zip.ToString().Length == 4)
            {
                err["BOOTH_ADDRESSPART"] = ERROR_MESSAGE.OK;
                return true;
            }
            else
                err["BOOTH_ADDRESSPART"] = ERROR_MESSAGE.BOOTH_ADDRESSPART;
            return false;
        }

        public static bool FullAddress(dto_booth model, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            bool ok;
            model.street_address = StringHelper.OnlyAlphanumeric(model.street_address, false, true, "notag", CharacterHelper.Address(), out ok);
            
            string town = model.region_dto.town;
            int zip = model.region_dto.zip;
            model.country = StringHelper.OnlyAlphanumeric(model.country, false, true, "notag", CharacterHelper.Country(), out ok);

            if (ok && !string.IsNullOrEmpty(model.street_address) && !string.IsNullOrEmpty(model.country))
            {
                err["BOOTH_ADDRESSFULL"] = ERROR_MESSAGE.OK;
                return true;
            }
            else if (model.fulladdress_str == "Part")
                err["BOOTH_ADDRESSFULL"] = ERROR_MESSAGE.OK;
            else
                err["BOOTH_ADDRESSFULL"] = ERROR_MESSAGE.BOOTH_ADDRESSFULL;
            return false;
        }

        /*public static bool Tags(biz_booth model, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            bool ok;
            if (model.booth_id == 0 || (model.tag_pocos != null && model.tag_pocos.Count > 0))
            {
                err["BOOTH_TAGS"] = ERROR_MESSAGE.OK;
                return true;
            }
            else
                err["BOOTH_TAGS"] = ERROR_MESSAGE.BOOTH_TAGS;
            return false;
        }*/

        public static void Register(bool success, ref Dictionary<string, SYSTEM_MESSAGE> err)
        {
            bool ok;
            if (success)
                err["REGISTER"] = SYSTEM_MESSAGE.REGISTER;
            else
                err["REGISTER"] = SYSTEM_MESSAGE.NO_MESSAGE;
        }

        public static void CookieLogin(bool success, ref Dictionary<string, SYSTEM_MESSAGE> err)
        {
            bool ok;
            if (success)
                err["COOKIE"] = SYSTEM_MESSAGE.NO_MESSAGE;
            else
                err["COOKIE"] = SYSTEM_MESSAGE.COOKIE;
        }

        public static void CreateLevel(bool success, ref Dictionary<string, SYSTEM_MESSAGE> err)
        {
            bool ok;
            if (success)
                err["CREATELEVEL"] = SYSTEM_MESSAGE.CREATELEVEL;
            else
                err["CREATELEVEL"] = SYSTEM_MESSAGE.NO_MESSAGE;
        }

        public static void CheckFeedback(col_email mail, ref Dictionary<string, SYSTEM_MESSAGE> err)
        {
            bool ok;
            if (string.IsNullOrEmpty(mail.Subject) || string.IsNullOrEmpty(mail.Message))
                err["FEEDBACK"] = SYSTEM_MESSAGE.FEEDBACK;
            else
                err["FEEDBACK"] = SYSTEM_MESSAGE.NO_MESSAGE;
        }

        /*public static bool FullAddress(poco_address address_poco, ref Dictionary<string, ERROR_MESSAGE> err)
        {
            if ((string.IsNullOrEmpty(address_poco.street_address) || address_poco.street_address == "(Ikke angivet.)") ||
                (address_poco.zip <= 0000) || 
                (string.IsNullOrEmpty(address_poco.town) || address_poco.town == "(Ikke angivet.)") ||
                (string.IsNullOrEmpty(address_poco.country) || address_poco.country == "(Ikke angivet.)"))
            {
                err["BOOTH_ADDRESSFULL"] = ERROR_MESSAGE.BOOTH_ADDRESSFULL;
                return false;
            }
            err["BOOTH_ADDRESSFULL"] = ERROR_MESSAGE.OK;
            return true;
        }*/

        public static class ErrorProduct
        {
            public static bool HasError(Dictionary<string, ERROR_MESSAGE> errors)
            {
                //if (errors["PRODUCT_CATEGORY"] == ERROR_MESSAGE.OK)
                    if (errors["PRODUCT_NAME"] == ERROR_MESSAGE.OK)
                        if (errors["PRODUCT_PRICE"] == ERROR_MESSAGE.OK)
                            //if (errors["PRODUCT_NOTE"] == ERROR_MESSAGE.OK)
                                //if (errors["PRODUCT_DESCRIPTION"] == ERROR_MESSAGE.OK)
                                    if (errors["PRODUCT_NOOFUNITS"] == ERROR_MESSAGE.OK)
                                        return false;
                return true;

            }
        }

        public static class ErrorCollection
        {
            public static bool HasError(Dictionary<string, ERROR_MESSAGE> errors)
            {
                //if (errors["COLLECTION_CATEGORY"] == ERROR_MESSAGE.OK)
                if (errors["COLLECTION_NAME"] == ERROR_MESSAGE.OK)
                    if (errors["COLLECTION_PRICE"] == ERROR_MESSAGE.OK)
                        //if (errors["COLLECTION_NOTE"] == ERROR_MESSAGE.OK)
                        //if (errors["COLLECTION_DESCRIPTION"] == ERROR_MESSAGE.OK)
                        return false;
                return true;

            }
        }

        public static class ErrorBooth
        {
            public static bool HasError(Dictionary<string, ERROR_MESSAGE> errors)
            {
                if (errors["BOOTH_NAME"] == ERROR_MESSAGE.OK &&
                    errors["BOOTH_DESCRIPTION"] == ERROR_MESSAGE.OK &&
                    errors["BOOTH_ADDRESSFULL"] == ERROR_MESSAGE.OK &&
                    errors["BOOTH_ADDRESSPART"] == ERROR_MESSAGE.OK)
                    return false;
                return true;
            }
        }

        public static class ErrorSalesmanProfile
        {
            public static bool HasError(Dictionary<string, ERROR_MESSAGE> errors)
            {
                if (errors["PROFILE_FIRSTNAME"] == ERROR_MESSAGE.OK)
                    if (errors["PROFILE_LASTNAME"] == ERROR_MESSAGE.OK)
                        if (errors["PROFILE_PHONENUMBER"] == ERROR_MESSAGE.OK)
                            if (errors["PROFILE_EMAIL"] == ERROR_MESSAGE.OK)
                                return false;
                return true;

            }
        }

        public static class ErrorCustomerProfile
        {
            public static bool HasError(Dictionary<string, ERROR_MESSAGE> errors)
            {
                if (errors["PROFILE_FIRSTNAME"] == ERROR_MESSAGE.OK)
                    if (errors["PROFILE_LASTNAME"] == ERROR_MESSAGE.OK)
                        //if (errors["PROFILE_PHONENUMBER"] == ERROR_MESSAGE.OK)
                            if (errors["PROFILE_EMAIL"] == ERROR_MESSAGE.OK)
                                return false;
                return true;

            }
        }
    }
}