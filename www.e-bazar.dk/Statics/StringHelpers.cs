using System;
using System.Linq;

namespace www.e_bazar.dk.Statics
{
    public static class StringHelper
    {
        public static string RemoveCharacter(string str, char character)
        {
            if (str == null)
                return null;
            int charindex = str.IndexOf(character);
            if (charindex != -1)
                str = str.Remove(charindex, 1);
            return str;
        }

        private static int CountChars(string str, char c)
        {
            int count = 0;
            foreach (char c_in_str in str)
            {
                if (c_in_str == c)
                    count += 1;
            }
            return count;
        }

        public static string _RemoveCharacters(string str, char[] characters)
        {
            if (str == null)
                return null;
            //string result="";
            foreach (char c in characters)
            {
                int count = CountChars(str, c);
                for (int i = 0; i < count; i++)
                    str = RemoveCharacter(str, c);
            }
            return str;
        }

        public static string RemoveStrings(string str, string[] strings)
        {
            if (str == null)
                return null;
            //string result="";
            foreach (string s in strings)
            {
                str = str.Replace(s, "");
            }
            return str;
        }

        public static string OnlyAlphanumeric(string str, bool allow_newline, bool allow_upper, string allow_tag, char[] allowed, out bool ok)
        {
            /*
             * allow_tag should be list or array
             * */

            if (str.IsNull())
                throw new Exception();

            if (allow_tag.IsNull())
                throw new Exception();

            if (allow_tag == "")
                throw new Exception();

            ok = true;
            
            char[] numeric = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            char[] alphalower = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'æ', 'ø', 'å' };
            char[] alphaupper = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'Æ', 'Ø', 'Å' };
            char[] newline = { '\r', '\n' };

            
            for (int i = 0; i < str.Length; i++)
            {
                char c = str.ElementAt(i);
                if (i < str.Length - allow_tag.Length - 2 && str.Substring(i, allow_tag.Length + 2) == "<" + allow_tag + ">")
                    i += allow_tag.Length + 1;
                else if (i < str.Length - allow_tag.Length - 3 && str.Substring(i, allow_tag.Length + 3) == "</" + allow_tag + ">")
                    i += allow_tag.Length + 2;
                else if (i < str.Length - allow_tag.Length - 4 && str.Substring(i, allow_tag.Length + 4) == "<" + allow_tag + " />")
                    i += allow_tag.Length + 3;
                else if (c == '<')
                {
                    int j = i + 1;
                                                
                    while (j < i + 15 && j < str.Length)
                    {
                        char elem = str.ElementAt(j);
                        if (alphalower.Contains(elem) || alphaupper.Contains(elem))
                            ;
                        else if (elem == '>')
                        {
                            str = "";
                            ok = false;
                            break;
                        }
                        else
                            break;
                                              
                        j++;                            
                    }
                }
                
                if (!ok)
                    break;
            }

            if (!ok)
                return str;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str.ElementAt(i);
                if (allowed.Contains(c) || (allow_newline && newline.Contains(c)) || (allow_upper && alphaupper.Contains(c)) || alphalower.Contains(c) || numeric.Contains(c))
                    ;
                else
                {
                    str = RemoveCharacter(str, c);
                    ok = false;
                    i--;
                }
            }
            
            return str;
        }

        public static string Only(string str, char[] allowed, out bool ok)
        {
            if (str.IsNull())
                throw new Exception();

            ok = true;
            
            for (int i = 0; i < str.Length; i++)
            {
                char c = str.ElementAt(i);
                if (allowed.Contains(c))
                    ;
                else
                {
                    str = RemoveCharacter(str, c);
                    ok = false;
                    i--;
                }
            }
            return str;
        }
    }
}