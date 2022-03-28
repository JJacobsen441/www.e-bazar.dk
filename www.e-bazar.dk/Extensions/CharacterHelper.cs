using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace www.e_bazar.dk.Extensions
{
    public class CharacterHelper
    {
        public static char[] All(bool withreturnnewline)
        {
            char c = '\r';
            char r = '\n';
            //char[] a = new char[] { ' ', '.', ',', '\'', '"', ':', ';', '&', '#', '!', '?', '/', '%', '+', '-', '(', ')', '[', ']', '{', '}', '<', '>' };
            char[] a = new char[] { ' ', '.', ',', '+', '-', '*', '/', ':', ';', '_', '\'', '#', '(', ')', '[', ']', '<', '>', '{', '}', '=', '?', '!', '@', '%', '$', '&' };
            if (withreturnnewline)
                a = a.Concat(new char[] { c }).ToArray();
            if (withreturnnewline)
                a = a.Concat(new char[] { r }).ToArray();
            return a;
        }

        public static char[] Limited(bool withsemi)
        {
            char s = ';';
            char[] a = new char[] { ' ', '_', ',', '+', '-', '\'', '&', '#', '(', ')', '[', ']' };
            if (withsemi)
                a = a.Concat(new char[] { s }).ToArray();
            return a;
        }

        public static char[] VeryLimited()
        {
            return new char[] { ' ', '\'', '-', '&', '#' };
        }

        public static char[] Website()
        {
            return new char[] { '-', '/', '.', ':' };
        }

        public static char[] Category()
        {
            return new char[] { ' ', '-', '.' };
        }

        public static char[] Name()
        {
            return new char[] { ' ', '-', '*' };
        }

        public static char[] Country()
        {
            return new char[] { ' ', '-' };
        }

        public static char[] Address()
        {
            return new char[] { ' ', '.', ',', '-', '\'' };
        }

        public static char[] Space()
        {
            return new char[] { ' ' };
        }

        public static char[] Param()
        {
            return new char[] { '0', '1', ':', '_' };
        }
    }
}