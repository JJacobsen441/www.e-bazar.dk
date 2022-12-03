using Newtonsoft.Json;
using System;
using System.Web.Mvc;
using www.e_bazar.dk.Interfaces;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Statics
{
    /*public class Params
    {
        //public dynamic bag;
        public Access access;
        //public dto_person current_user;
    }/**/

    //class ControllerHelper<T> where T : Controller, IControllerSetup
    class ControllerHelper
    {

        public static void Setup<T>(T con, SETUP seq, bool check_user, bool check_user_not_admin) where T : Controller, IControllerSetup
        {            
            if (!con.access.Queue())
                throw new Exception();
            
            CurrentUser.Inst().Setup(con);
                                    
            con.current_user = null;
            if(seq != SETUP.XXXX)
                con.current_user = CurrentUser.Inst().GetDTO(seq);
            
            if (check_user)
            {
                if (con.current_user == null)
                    throw new Exception("A-OK, handled.");
            }

            if (check_user_not_admin)
            {
                if (CurrentUser.Inst().CurrentUserName != SettingsHelper.Basic.EMAIL_ADMIN())
                {
                    if (con.current_user == null)
                        throw new Exception("A-OK, handled.");
                }
            }

            con.ViewBag.CurrentUser = con.current_user;
            con.ViewBag.User = CurrentUser.Inst();
        }

        public static void Finally<T>(T con) where T: Controller, IControllerSetup
        {
            try
            {
                con.access.UnQueue();
            }
            catch (Exception e)
            {
                string subject = "Fejl i finally!";
                string body = ErrorHelper.FormatError(e);
                AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), subject, body);
            }
        }

        public static void JsonError<T>(T con) where T : Controller
        {
            if (ThisSession.Json_Errors != null)
            {
                con.ViewBag.JSON_ERRORS = "";
                string s = JsonConvert.SerializeObject(ThisSession.Json_Errors);
                if (!string.IsNullOrEmpty(s) && s != "{}")
                    con.ViewBag.JSON_ERRORS = s;
                ThisSession.Json_Errors = null;
            }
        }

        public static void JsonMessage<T>(T con) where T : Controller
        {
            if (ThisSession.Json_Messages != null)
            {
                con.ViewBag.JSON_SYSTEM_MESSAGE = "";
                string s2 = JsonConvert.SerializeObject(ThisSession.Json_Messages);
                if (!string.IsNullOrEmpty(s2) && s2 != "{}")
                    con.ViewBag.JSON_SYSTEM_MESSAGE = s2;
                ThisSession.Json_Messages = null;
            }
        }
    }
}