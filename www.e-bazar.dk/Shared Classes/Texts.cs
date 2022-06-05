using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Models.DTOs;

namespace www.e_bazar.dk.SharedClasses
{
    public class StatusNext
    {
        public DELIVERY[] states = new DELIVERY[2];
    }
    
    public class Texts
    {
        private static bool is_setup = false;
        private static bool is_running = false;

        private static Dictionary<NOP, string> nop = new Dictionary<NOP, string>();
        private static Dictionary<DELIVERY, string> status_delivery = new Dictionary<DELIVERY, string>();
        private static Dictionary<STOCK, string> status_stock = new Dictionary<STOCK, string>();
        private static Dictionary<CONDITION, string> status_condition = new Dictionary<CONDITION, string>();
        private static Dictionary<SYSTEM_MESSAGE, string> system_message = new Dictionary<SYSTEM_MESSAGE, string>();
        //private static Dictionary<CATEGORY, string> category_main = new Dictionary<CATEGORY, string>();
        //private static Dictionary<CATEGORY, string> category_desc = new Dictionary<CATEGORY, string>();
        //private static List<KeyValuePair<CATEGORY, string>> category_second = new List<KeyValuePair<CATEGORY, string>>();
        private static Dictionary<ERROR_MESSAGE, string> error_message = new Dictionary<ERROR_MESSAGE, string>();

        private static bool OK()
        {
            if (!is_setup && !is_running)
            {
                is_running = true;
                Setup();
                is_setup = true;
                is_running = false;
            }
            if (is_running)
                return false;
            return is_setup && !is_running;
        }

        private static void Setup()
        {
            nop.Add(NOP.NO_TAGS, "Ingen Tags.");
            nop.Add(NOP.NO_RATING, "Ingen Vurdering.");
            nop.Add(NOP.INGEN_PRIS, "Ikke oplyst.");
            nop.Add(NOP.NO_STATUS, "Ingen Status.");
            nop.Add(NOP.NO_CONDITION, "Ingen Behold.");/////////////////////////////////////////////////////////////
            nop.Add(NOP.NO_NOTE, "Ingen Note.");
            nop.Add(NOP.NO_FIRSTNAME, "Fornavn ikke oplyst.");
            nop.Add(NOP.NO_LASTNAME, "Efternavn ikke oplyst.");
            nop.Add(NOP.NO_PHONENUMBER, "Ikke oplyst.");
            nop.Add(NOP.NO_EMAIL, "Email ikke oplyst.");
            nop.Add(NOP.NO_DESCRIPTION, "Ingen beskrivelse.");
            nop.Add(NOP.UDFYLD, "(Udfyld venligst.)");

            status_delivery.Add(DELIVERY.DEFAULT, "No");
            status_delivery.Add(DELIVERY.UDSTILLING, "Varen står til udstilling.");
            status_delivery.Add(DELIVERY.KØBT, "Varen er købt.");
            status_delivery.Add(DELIVERY.SEND, "Varen er sendt.");
            status_delivery.Add(DELIVERY.GODKENDT, "Varen er modtaget og godkendt.");
            status_delivery.Add(DELIVERY.RETUR, "Varen er sendt retur.");

            status_stock.Add(STOCK.PÅ_LAGER, "Vare er på lager.");
            status_stock.Add(STOCK.IKKE_PÅ_LAGER, "Varen er ikke på lager.");
            status_stock.Add(STOCK.FÅ_PÅ_LAGER, "Få på lager.");

            status_condition.Add(CONDITION.VELHOLDT, "Varen er velholdt.");
            status_condition.Add(CONDITION.SLIDT, "Varen er slidt.");
            status_condition.Add(CONDITION.MEGET_SLIDT, "Varen er meget slidt.");

            system_message.Add(SYSTEM_MESSAGE.REGISTER, "En besked er blevet sendt. Tjek venligst din indbakke og spamfolder!");
            system_message.Add(SYSTEM_MESSAGE.COOKIE, "Accepter venligst cookies.");
            system_message.Add(SYSTEM_MESSAGE.CREATELEVEL, "Ugyldigt mappenavn.");
            system_message.Add(SYSTEM_MESSAGE.FEEDBACK, "Udfyld venligst både emne og besked.");

            //category_main.Add(CATEGORY.FURNITURE, "møbler");
            //category_desc.Add(CATEGORY.FURNITURE, "(div. møbler)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "spiseborde"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "skriveborde"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "tv-borde"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "lænestole"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "køkkenstole"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "sofaer"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "hjørnesofaer"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "chaiselon"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "divan"));


            //category_main.Add(CATEGORY.CLOTHES, "beklædning");
            //category_desc.Add(CATEGORY.CLOTHES, "(div.beklædningsgenstande)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "bluser"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "bukser"));

            //category_main.Add(CATEGORY.JEWELRY, "smykker");
            //category_desc.Add(CATEGORY.JEWELRY, "(div. smykker)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "ringe"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "halskæder"));

            //category_main.Add(CATEGORY.ART, "kunst");
            //category_desc.Add(CATEGORY.ART, "(f.eks. malerier, stentøj, keramik)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "malerier"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "stentøj"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "keramik"));

            //category_main.Add(CATEGORY.HOBBY, "hobby");
            //category_desc.Add(CATEGORY.HOBBY, "(f.eks. samlerobjekter, samlinger)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "plader"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "bøger"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "blade"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "mønter"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "frimærker"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "dukker"));

            //category.Add(CATEGORY.MUSIC, "musik");
            //category_desc.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.MUSIC, "(div. plader, pladesamlinger)"));

            //category.Add(CATEGORY.BOOKS, "bøger og blade");
            //category_desc.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.BOOKS, "(div. bøger eller magasiner)"));

            //category_main.Add(CATEGORY.EVERYDAYART, "brugskunst");
            //category_desc.Add(CATEGORY.EVERYDAYART, "(f.eks. platter og skåle)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "platter"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "skåle"));

            //category_main.Add(CATEGORY.EVERYDAYOBJECTS, "brugsgenstande");
            //category_desc.Add(CATEGORY.EVERYDAYOBJECTS, "(div. praktiske ting til boligen)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "bestik"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "stiger"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "skamler"));

            //category_main.Add(CATEGORY.INSTRUMENTS, "musik instrumenter");
            //category_desc.Add(CATEGORY.INSTRUMENTS, "(div. instrumenter)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "slagtøj"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "strenginstrumenter"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "klavere"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "blæseinstrimenter"));

            //category_main.Add(CATEGORY.ARTICLES, "hobby artikler");
            //category_desc.Add(CATEGORY.ARTICLES, "(f.eks. værktøj)");
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "hobby værktøj"));
            //category_second.Add(new KeyValuePair<CATEGORY, string>(CATEGORY.FURNITURE, "hobby materiale"));

            //category.Add(CATEGORY.ELECTRONIC, "elektronik");/////////////////////////////////////
            //category_desc.Add(CATEGORY.ELECTRONIC, "(eks. computer, billed, lyd)");

            //category.Add(CATEGORY.SPORT, "sport og fritid");/////////////////////////////////////
            //category_desc.Add(CATEGORY.SPORT, "(eks. cyklen, camping)");

            //category.Add(CATEGORY.GARDEN, "haven");
            //category_desc.Add(CATEGORY.GARDEN, "(eks. haveredskaber)");

            //category.Add(CATEGORY.TOOLSHED, "værkstedet");
            //category_desc.Add(CATEGORY.TOOLSHED, "(eks. værktøj)");

            //category.Add(CATEGORY.FORKIDS, "børnene");
            //category_desc.Add(CATEGORY.FORKIDS, "(eks. legetøj, tøj)");

            //category.Add(CATEGORY.MISC, "diverse");//////////////////////////////////////////////
            //category_desc.Add(CATEGORY.MISC, "(eks. nips)");

            error_message.Add(ERROR_MESSAGE.PRODUCT_CATEGORY, "Kategori navn skal være udfyldt!");
            error_message.Add(ERROR_MESSAGE.PRODUCT_NAME, "Produkt navn skal være udfyldt!");
            error_message.Add(ERROR_MESSAGE.PRODUCT_PRICE, "Prisen skal være sat!");
            error_message.Add(ERROR_MESSAGE.PRODUCT_NOTE, "Mangler note!");
            error_message.Add(ERROR_MESSAGE.PRODUCT_DESCRIPTION, "Mangler beskrivelse!");
            error_message.Add(ERROR_MESSAGE.PRODUCT_NOOFUNITS, "Mangler antal!");
            error_message.Add(ERROR_MESSAGE.COLLECTION_CATEGORY, "Kategori navn skal være udfyldt!");
            error_message.Add(ERROR_MESSAGE.COLLECTION_NAME, "Navn skal være udfyldt!");
            error_message.Add(ERROR_MESSAGE.COLLECTION_PRICE, "Prisen skal være sat!");
            error_message.Add(ERROR_MESSAGE.COLLECTION_NOTE, "Mangler note!");
            error_message.Add(ERROR_MESSAGE.COLLECTION_DESCRIPTION, "Mangler beskrivelse!");
            error_message.Add(ERROR_MESSAGE.BOOTH_NAME, "Bod navn skal være udfyldt!");
            error_message.Add(ERROR_MESSAGE.BOOTH_ADDRESSFULL, "Mangler fuld addresse!");
            error_message.Add(ERROR_MESSAGE.BOOTH_ADDRESSPART, "Mangler postnr eller by!");
            error_message.Add(ERROR_MESSAGE.BOOTH_TAGS, "Der skal være valgt mindst en kategori!");
            error_message.Add(ERROR_MESSAGE.PROFILE_FIRSTNAME, "Fornavn skal være udfyldt!");
            error_message.Add(ERROR_MESSAGE.PROFILE_LASTNAME, "Efternavn skal være udfyldt!");
            error_message.Add(ERROR_MESSAGE.PROFILE_PHONENUMBER, "Udfyld venligst telefon nr.");
            error_message.Add(ERROR_MESSAGE.PROFILE_EMAIL, "Email skal være udfyldt!");

            //public static string GetErrorMessageProduct(ERROR_MESSAGE? msg)
            //public static string GetErrorMessageCollection(ERROR_MESSAGE? msg)
            //public static string GetErrorMessageBooth(ERROR_MESSAGE? msg)
            //public static string GetErrorMessageProfile(ERROR_MESSAGE? msg)

            //is_setup = true;
        }
                
        /*public static Dictionary<CATEGORY, string> GetCategorys()
        {
            if (!is_setup)
                Setup();
            return category_main;
        }
        public static Dictionary<CATEGORY, string> GetCategorysDesc()
        {
            if (!is_setup)
                Setup();
            return category_desc;
        }*/


        /*public static string GetNopName(string value)
        {
            if(!is_setup)
                Setup();
            string result = nop.FirstOrDefault(s => s.Value == value).Key.ToString();
            return result;
        }*/
        public static string GetNopValue(string key)
        {
            if (!OK())
                return "default";
            string result = nop.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }
        /*public static NOP GetNopEnum(string key)
        {
            if (!is_setup)
                Setup();
            NOP result = nop.FirstOrDefault(s => s.Key.ToString() == key).Key;
            return result;
        }*/

        /*public static string GetDeliveryName(string value)
        {
            if (!is_setup)
                Setup();
            string result = status_delivery.FirstOrDefault(s => s.Value == value).Key.ToString();
            return result;
        }*/
        public static string GetDeliveryValue(string key)
        {
            if (!OK())
                return "default";
            string result = status_delivery.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }
        public static DELIVERY GetDeliveryEnum(string key)
        {
            if (!OK())
                return DELIVERY.DEFAULT;
            DELIVERY result = status_delivery.FirstOrDefault(s => s.Key.ToString() == key).Key;
            return result;
        }

        /*public static StatusNext GetDeliveryNext(DELIVERY current, biz_person person_poco)
        {
            bool is_salesman = person_poco is biz_person;
            StatusNext n = new StatusNext();
            switch (current)
            {
                case DELIVERY.UDSTILLING:
                    if (is_salesman)
                        n.states[0] = DELIVERY.DEFAULT;
                    else
                        n.states[0] = DELIVERY.KØBT;
                    break;
                case DELIVERY.KØBT:
                    if (is_salesman)
                        n.states[0] = DELIVERY.SEND;
                    else
                        n.states[0] = DELIVERY.DEFAULT;
                    break;
                case DELIVERY.SEND:
                    if (is_salesman)
                        n.states[0] = DELIVERY.DEFAULT;
                    else
                    {
                        n.states[0] = DELIVERY.GODKENDT;
                        n.states[1] = DELIVERY.RETUR;
                    }
                    break;
                default:
                    //throw new Exception();
                    return null;
            }
            return n;
        }*/
        /*public static string GetStockName(string value)
        {
            if (!is_setup)
                Setup();
            string result = status_stock.FirstOrDefault(s => s.Value == value).Key.ToString();
            return result;
        }*/
        public static string GetStockValue(string key)
        {
            if (!OK())
                return "default";
            string result = status_stock.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }
        public static STOCK GetStockEnum(string key)
        {
            if (!OK())
                return STOCK.IKKE_PÅ_LAGER;
            STOCK result = status_stock.FirstOrDefault(s => s.Key.ToString() == key).Key;
            return result;
        }

        /*public static string GetConditionName(string value)
        {
            if (!is_setup)
                Setup();
            string result = status_condition.FirstOrDefault(s => s.Value == value).Key.ToString();
            return result;
        }*/
        public static string GetConditionValue(string key)
        {
            if (!OK())
                return "default";
            string result = status_condition.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }
        public static CONDITION GetConditionEnum(string key)
        {
            if (!OK())
                return CONDITION.MEGET_SLIDT;
            CONDITION result = status_condition.FirstOrDefault(s => s.Key.ToString() == key).Key;
            return result;
        }

        /*public static string GetSystemMessageName(string value)
        {
            if (!is_setup)
                Setup();
            string result = system_message.FirstOrDefault(s => s.Value == value).Key.ToString();
            return result;
        }*/
        public static string GetSystemMessageValue(string key)
        {
            if (!OK())
                return "default";
            string result = system_message.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }
        /*public static SYSTEM_MESSAGE GetSystemMessageEnum(string key)
        {
            if (!is_setup)
                Setup();
            SYSTEM_MESSAGE result = system_message.FirstOrDefault(s => s.Key.ToString() == key).Key;
            return result;
        }*/

        /*public static string GetErrorMessageName(string value)
        {
            if (!is_setup)
                Setup();
            string result = error_message.FirstOrDefault(s => s.Value == value).Key.ToString();
            return result;
        }*/
        public static string GetErrorMessageValue(string key)
        {
            if (!OK())
                return "default";
            string result = error_message.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }
        /*public static ERROR_MESSAGE GetErrorMessageEnum(string key)
        {
            if (!is_setup)
                Setup();
            ERROR_MESSAGE result = error_message.FirstOrDefault(s => s.Key.ToString() == key).Key;
            return result;
        }*/

        /*public static string GetErrorMessageProduct(ERROR_MESSAGE? msg)
        {
            switch (msg)
            {
                case ERROR_MESSAGE.PRODUCT_CATEGORY:
                    return "Kategori navn skal være udfyldt!";
                case ERROR_MESSAGE.PRODUCT_NAME:
                    return "Produkt navn skal være udfyldt!";
                case ERROR_MESSAGE.PRODUCT_PRICE:
                    return "Prisen skal være sat!";
                case ERROR_MESSAGE.PRODUCT_NOTE:///////////////////////////////////////////////////////
                    return "Mangler note!";
                case ERROR_MESSAGE.PRODUCT_DESCRIPTION:////////////////////////////////////////////////////
                    return "Mangler beskrivelse!";
                case ERROR_MESSAGE.PRODUCT_NOOFUNITS:////////////////////////////////////////////////////
                    return "Mangler antal!";
                default:
                    return "";
            }
        }

        public static string GetErrorMessageCollection(ERROR_MESSAGE? msg)
        {
            switch (msg)
            {
                case ERROR_MESSAGE.COLLECTION_CATEGORY:
                    return "Kategori navn skal være udfyldt!";
                case ERROR_MESSAGE.COLLECTION_NAME:
                    return "Navn skal være udfyldt!";
                case ERROR_MESSAGE.COLLECTION_PRICE:
                    return "Prisen skal være sat!";
                case ERROR_MESSAGE.COLLECTION_NOTE:///////////////////////////////////////////////////////
                    return "Mangler note!";
                case ERROR_MESSAGE.COLLECTION_DESCRIPTION:////////////////////////////////////////////////////
                    return "Mangler beskrivelse!";
                default:
                    return "";
            }
        }

        public static string GetErrorMessageBooth(ERROR_MESSAGE? msg)
        {
            switch (msg)
            {
                case ERROR_MESSAGE.BOOTH_NAME:
                    return "Bod navn skal være udfyldt!";
                /*case ERROR_MESSAGE.BOOTH_DESCRIPTION:////////////////////////////////////////////////////
                    return "Mangler beskrivelse!";
                case ERROR_MESSAGE.BOOTH_ADDRESSFULL:
                    return "Mangler fuld addresse!";
                case ERROR_MESSAGE.BOOTH_ADDRESSPART:
                    return "Mangler postnr eller by!";
                case ERROR_MESSAGE.BOOTH_TAGS:
                    return "Der skal være mindst et søgeterm(søgetermer bruges som kategorier på en vare)!";
                default:
                    throw new Exception();
            }
        }
        public static string GetErrorMessageProfile(ERROR_MESSAGE? msg)
        {
            switch (msg)
            {
                case ERROR_MESSAGE.PROFILE_FIRSTNAME:
                    return "Fornavn skal være udfyldt!";
                case ERROR_MESSAGE.PROFILE_LASTNAME:
                    return "Efternavn skal være udfyldt!";
                case ERROR_MESSAGE.PROFILE_PHONENUMBER:
                    return "Telefon nr. skal være udfyldt!";
                case ERROR_MESSAGE.PROFILE_EMAIL:////////////////////////////////////////////////////
                    return "Email skal være udfyldt!";
                default:
                    return "";
            }
        }*/
    }
}