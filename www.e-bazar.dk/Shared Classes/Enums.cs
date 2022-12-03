﻿namespace www.e_bazar.dk.SharedClasses
{
    public enum NOP { NO_DESCRIPTION , NO_TAGS, NO_RATING, INGEN_PRIS, NO_STATUS, NO_CONDITION, NO_NOTE, NO_FIRSTNAME, NO_LASTNAME, NO_PHONENUMBER, NO_EMAIL, UDFYLD}
    public enum ERROR_MESSAGE { OK, PROFILE_FIRSTNAME, PROFILE_LASTNAME, PROFILE_PHONENUMBER, PROFILE_EMAIL, BOOTH_NAME, BOOTH_DESCRIPTION, BOOTH_ADDRESSFULL, BOOTH_ADDRESSPART, BOOTH_TAGS, PRODUCT_CATEGORY, PRODUCT_NAME, PRODUCT_PRICE, PRODUCT_NOTE, PRODUCT_DESCRIPTION, PRODUCT_NOOFUNITS, COLLECTION_CATEGORY, COLLECTION_NAME, COLLECTION_PRICE, COLLECTION_NOTE, COLLECTION_DESCRIPTION }
    public enum SYSTEM_MESSAGE { NO_MESSAGE, REGISTER, COOKIE, CREATELEVEL, FEEDBACK}
    public enum CATEGORY { FURNITURE, CLOTHES, JEWELRY, ART, BOOKS, HOBBY, INSTRUMENTS, ARTICLES, MUSIC, ELECTRONIC, SPORT, GARDEN, TOOLSHED, EVERYDAYART, EVERYDAYOBJECTS, MISC, FORKIDS }
    public enum DELIVERY { DEFAULT, UDSTILLING, KØBT, SEND, GODKENDT, RETUR }
    public enum STOCK { PÅ_LAGER, IKKE_PÅ_LAGER, FÅ_PÅ_LAGER }
    public enum CONDITION { VELHOLDT, SLIDT, MEGET_SLIDT }
    public enum FILE_NAME { NONE, HASH, GUID }
    public enum PATH { DEFAULT, /*PROFILE_DIRECTORY,*/ PROFILE_DIRECTORY_NAME, PROFILE_DIRECTORY_TMP, BOOTH_DIRECTORY, BOOTH_DIRECTORY_NAME, BOOTH_DIRECTORY_TMP, PRODUCT_DIRECTORY, PRODUCT_DIRECTORY_NAME, PRODUCT_DIRECTORY_TMP, COLLECTION_DIRECTORY, COLLECTION_DIRECTORY_NAME, COLLECTION_DIRECTORY_TMP }
    public enum ERROR { ISMOBILE, REMOVEPARAM, SAVEPARAM, EDITBOOTH, CREATEBOOTH, DELETEBOOTH, CREATEPRODUCT, EDITPRODUCT, DELETEPRODUCT, CREATECOLLECTION, EDITCOLLECTION, DELETECOLLECTION, ADDPRODUCTTOCOLLECTION, REMOVEPRODUCTFROMCOLLECTION, CUSTOMERPROFILE, SALESMANPROFILE, USERPROFILE, UPLOADIMAGE, REMOVEBOOTHIMAGE, REMOVEPRODUCTIMAGE, REMOVECOLLECTIONIMAGE, GETTAGS, SAVETAG, REMOVETAG, DELETECONVERSATION, REMOVEFAVORITE, CREATELEVEL, DELETELEVEL, MOVELEVEL, SETLEVEL, GETADDRESSEMAIL, GETADDRESSTOWN ,
                        MARKETPLACE, MARKETPLACESEARCH, BOOTH, PRODUCT, COLLECTION, MESSAGE, ADDFAVORITE, ADDRATING, INFO, CONDITIONS, FEATURES,
                        LOGIN, GETCATS }
    public enum MESSAGE_TAG { OK, MAXLIMIT, EMPTYNAME }

    public enum TYPE { DEFAULT, MARKETPLACE, PROFILE, BOOTH, PRODUCT, COLLECTION, FOLDER_A, FOLDER_B }
    public enum BYTE_CHECK { SAME, UP, DOWN, ERROR }
    public enum SETUP { XXXX, FTT, FFF, TFT}
}