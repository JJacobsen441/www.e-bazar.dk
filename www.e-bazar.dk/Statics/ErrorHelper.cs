using System;

namespace www.e_bazar.dk.SharedClasses
{
    public class ErrorHelper
    {
        public static String HandleError(ERROR function, Exception e)
        {
            string res = "Where: " + function.ToString() +
                         "<br />" + FormatError(e);
            return res;
        }

        public static String FormatError(Exception e)
        {
            string res = "Message: " + e.Message +
                            "<br /><br />InnerException: " + e.InnerException +
                            "<br /><br />StackTrace: " + e.StackTrace;
            return res.Replace(" at ", " <br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;at ");
        }
    }
}