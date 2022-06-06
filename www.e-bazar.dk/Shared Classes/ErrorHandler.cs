using System;

namespace www.e_bazar.dk.SharedClasses
{
    public class ErrorHandler
    {
        //public String HandleError_Administration(ERROR_ADMINISTRATION function, Exception e)
        //{
        //    string res = "Exception: " + function.ToString() + ", " + e.Message +
        //                "<br /><br />InnerException: " + e.InnerException +
        //                "<br /><br />StackTraceException: " + e.StackTrace;
        //    /*String res = "Unknown Error" + e.Message;
        //    switch (function)
        //    {
        //        case ERROR_ADMINISTRATION.EDITBOOTH:
        //            res = "Exception: " + e.Message + " - EditBooth" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace;
        //            break;
        //        case ERROR_ADMINISTRATION.CREATEBOOTH:
        //            res = "Exception: " + e.Message + " - CreateBooth" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.DELETEBOOTH:
        //            res = "Exception: " + e.Message + " - DeleteBooth" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.CREATEPRODUCT:
        //            res = "Exception: " + e.Message + " - CreateProduct" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.EDITPRODUCT:
        //            res = "Exception: " + e.Message + " - EditProduct" +
        //                    "<br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.DELETEPRODUCT:
        //            res = "Exception: " + e.Message + " - DeleteProduct" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.CREATECOLLECTION:
        //            res = "Exception: " + e.Message + " - CreateCollection" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.EDITCOLLECTION:
        //            res = "Exception: " + e.Message + " - EditCollection" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.DELETECOLLECTION:
        //            res = "Exception: " + e.Message + " - DeleteCollection" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.CUSTOMERPROFILE:
        //            res = "Exception: " + e.Message + " - CustomerProfile" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace;
        //            break;
        //        case ERROR_ADMINISTRATION.SALESMANPROFILE:
        //            res = "Exception: " + e.Message + " - SalesmanProfile" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace; 
        //            break;
        //        case ERROR_ADMINISTRATION.USERPROFILE:
        //            res = "Exception: " + e.Message + " - UserProfile" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace;
        //            break;
        //        case ERROR_ADMINISTRATION.UPLOADIMAGE:
        //            res = "Exception: " + e.Message + " - UploadImage" +
        //                    "<br /><br />InnerException: " + e.InnerException +
        //                    "<br /><br />StackTraceException: " + e.StackTrace;
        //            break;

        //        default:
        //            res = e.Message + " - Default";
        //            break;
        //    }*/
        //    return res;
        //}

        public String HandleError(ERROR function, Exception e)
        {
            string res = "Where: " + function.ToString() +
                         "<br />" + FormatError(e);
            return res;
        }
        public String FormatError(Exception e)
        {
            string res = "Message: " + e.Message +
                            "<br /><br />InnerException: " + e.InnerException +
                            "<br /><br />StackTrace: " + e.StackTrace;
            return res.Replace(" at ", " <br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;at ");
        }

        //public String HandleError_Account(ERROR_ACCOUNT function, Exception e)
        //{
        //    String res = "Exception: " + function.ToString() + ", " + e.Message +
        //                "<br /><br />InnerException: " + e.InnerException +
        //                "<br /><br />StackTraceException: " + e.StackTrace;
        //    /*switch (function)
        //    {
        //        case ERROR_ADMINISTRATION_ACCOUNT.Login:
        //            if (e.GetType() == typeof(ArgumentNullException))
        //                res = "ERROR: " + e.Message;
        //            break;
        //    }*/
        //    return res;
        //}
    }
}