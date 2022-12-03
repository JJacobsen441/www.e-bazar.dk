using System.Collections.Generic;
using System.Linq;

namespace www.e_bazar.dk.SharedClasses
{
    public class StatusNext
    {
        public DELIVERY[] states = new DELIVERY[2];
    }
    
    public class TextHelper
    {
        private static bool is_setup = false;
        private static bool is_running = false;

        private static Dictionary<NOP, string> nop = new Dictionary<NOP, string>();
        private static Dictionary<DELIVERY, string> status_delivery = new Dictionary<DELIVERY, string>();
        private static Dictionary<STOCK, string> status_stock = new Dictionary<STOCK, string>();
        private static Dictionary<CONDITION, string> status_condition = new Dictionary<CONDITION, string>();
        private static Dictionary<SYSTEM_MESSAGE, string> system_message = new Dictionary<SYSTEM_MESSAGE, string>();
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

            //is_setup = true;
        }
                
        public static string GetNopValue(string key)
        {
            if (!OK())
                return "default";
            string result = nop.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }

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

        public static string GetSystemMessageValue(string key)
        {
            if (!OK())
                return "default";
            string result = system_message.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }
        
        public static string GetErrorMessageValue(string key)
        {
            if (!OK())
                return "default";
            string result = error_message.FirstOrDefault(s => s.Key.ToString() == key).Value;
            return result;
        }       
    }
}