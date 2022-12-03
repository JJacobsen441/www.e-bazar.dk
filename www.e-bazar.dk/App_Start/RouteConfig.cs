using System.Web.Mvc;
using System.Web.Routing;

namespace www.e_bazar.dk
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapMvcAttributeRoutes();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");



            //////////////////routes.MapRoute(
            //////////////////    name: "Index",
            //////////////////    url: "",
            //////////////////    defaults: new { controller = "Home", action = "Index" }//,
            //////////////////    //constraints: new { msg = "login" }
            //////////////////);

            //routes.MapRoute(
            //    name: "Features",
            //    url: "Marketplace/Features",
            //    defaults: new { controller = "Marketplace", action = "Features" }//,
            //    //constraints: new { msg = "login" }
            //);

            //routes.MapRoute(
            //    name: "IsMobile",
            //    url: "Marketplace/IsMobile",
            //   defaults: new { controller = "Marketplace", action = "IsMobile" }//,
            //    //constraints: new { msg = "login" }
            //);

            //routes.MapRoute(
            //    name: "Maintenance",
            //    url: "Administration/Maintenance",
            //    defaults: new { controller = "Administration", action = "Maintenance" }//,
            //    //constraints: new { msg = "login" }
            //);
            //routes.MapRoute(
            //    name: "GetCats",
            //    url: "Marketplace/GetCats",
            //    defaults: new { controller = "Marketplace", action = "GetCats" }//,
            //    //constraints: new { msg = "login" }
            //);



            //routes.MapRoute(
            //    name: "UserProfile",
            //    url: "administration/redigerprofil",
            //    defaults: new { controller = "Administration", action = "UserProfile" }//,
            //    //constraints: new { msg = "login" }
            //);
            //routes.MapRoute(
            //    name: "SalesmanProfile",
            //    url: "administration/redigerprofil/sales",
            //    defaults: new { controller = "Administration", action = "SalesmanProfile" }//,
            //    //constraints: new { msg = "login" }
            //);
            //routes.MapRoute(
            //    name: "CustomerProfile",
            //    url: "administration/redigerprofil/cust",
            //    defaults: new { controller = "Administration", action = "CustomerProfile" }//,
            //    //constraints: new { msg = "login" }
            //);
            //routes.MapRoute(
            //    name: "AdminGet",
            //    url: "admin/run",
            //    defaults: new { controller = "Marketplace", action = "AdminGet" }//,
            //    //constraints: new { msg = "login" }
            //);
            //routes.MapRoute(
            //    name: "AdminPost",
            //    url: "Marketplace/AdminPost",
            //    defaults: new { controller = "Marketplace", action = "AdminPost" }//,
            //    //constraints: new { msg = "login" }
            //);




            //administration
            //routes.MapRoute(
            //    name: "CreateProduct",
            //    url: "administration/opretprodukt",
            //    defaults: new { controller = "Administration", action = "CreateProduct" }
            //);
            //routes.MapRoute(
            //    name: "EditProduct1",
            //    url: "administration/redigerprodukt/{product_id}",
            //    defaults: new { controller = "Administration", action = "EditProduct" }//,
            //    //constraints: new { product_id = @"\d+" }
            //);
            //routes.MapRoute(
            //    name: "EditProduct2",
            //    url: "administration/redigerprodukt/post",
            //    defaults: new { controller = "Administration", action = "EditProduct" }
            //);
            //routes.MapRoute(
            //    name: "DeleteProduct",
            //    url: "deleteproduct",
            //    defaults: new { controller = "Administration", action = "DeleteProduct" }
            //);
            //routes.MapRoute(
            //    name: "CreateCollection",
            //    url: "administration/opretsamling",
            //    defaults: new { controller = "Administration", action = "CreateCollection" }
            //);
            //routes.MapRoute(
            //    name: "EditCollection1",
            //    url: "administration/redigersamling/{collection_id}",
            //    defaults: new { controller = "Administration", action = "EditCollection" }//,
            //    //constraints: new { collection_id = @"\d+" }
            //);
            //routes.MapRoute(
            //    name: "EditCollection2",
            //    url: "administration/redigersamling/post",
            //    defaults: new { controller = "Administration", action = "EditCollection" }
            //);
            //routes.MapRoute(
            //    name: "DeleteCollection",
            //    url: "deletecollection",
            //    defaults: new { controller = "Administration", action = "DeleteCollection" }
            //);
            //routes.MapRoute(
            //    name: "CreateBooth",
            //    url: "administration/opretbod",
            //    defaults: new { controller = "Administration", action = "CreateBooth" }
            //);
            //routes.MapRoute(
            //    name: "EditBooth1",
            //    url: "administration/redigerbod/{booth_id}",
            //    defaults: new { controller = "Administration", action = "EditBooth" }//,
            //    //constraints: new { id = @"\d+" }
            //);
            //routes.MapRoute(
            //    name: "EditBooth2",
            //    url: "administration/redigerbod/post",
            //    defaults: new { controller = "Administration", action = "EditBooth" }
            //);
            //routes.MapRoute(
            //    name: "DeleteBooth",
            //    url: "deletebooth",
            //    defaults: new { controller = "Administration", action = "DeleteBooth" }
            //);







            //marketplace
            //routes.MapRoute(
            //    name: "Info",
            //    url: "info/info",
            //    defaults: new { controller = "Marketplace", action = "Info" }
            //);
            //routes.MapRoute(
            //    name: "Conditions",
            //    url: "info/betingelser",
            //    defaults: new { controller = "Marketplace", action = "Conditions" }
            //);
            //routes.MapRoute(
            //    name: "Cookie",
            //    url: "Marketplace/Cookie",
            //    defaults: new { controller = "Marketplace", action = "Cookie" }
            //);




            //marketplace
            //routes.MapRoute(
            //    name: "SelectCatelog",
            //    url: "Marketplace/SelectCatelog",
            //    defaults: new { controller = "Marketplace", action = "SelectCatelog" }//,
            //    //constraints: new { msg = "login" }
            //);
            //routes.MapRoute(
            //    name: "AddFavorite",
            //    url: "Marketplace/AddFavorite",
            //    defaults: new { controller = "Marketplace", action = "AddFavorite" }
            //);
            //routes.MapRoute(
            //    name: "RemoveFavorite1",
            //    url: "Marketplace/RemoveFavorite",
            //    defaults: new { controller = "Marketplace", action = "RemoveFavorite" }
            //);
            //routes.MapRoute(
            //    name: "AddFollowing",
            //    url: "Marketplace/AddFollowing",
            //    defaults: new { controller = "Marketplace", action = "AddFollowing" }
            //);
            //routes.MapRoute(
            //    name: "RemoveFollowing1",
            //    url: "Marketplace/RemoveFollowing",
            //    defaults: new { controller = "Marketplace", action = "RemoveFollowing" }
            //);
            //routes.MapRoute(
            //    name: "AddRating",
            //    url: "Marketplace/AddRating",
            //    defaults: new { controller = "Marketplace", action = "AddRating" }
            //);



            //administration
            //routes.MapRoute(
            //    name: "AddProductToCollection",
            //    url: "Administration/AddProductToCollection",
            //    defaults: new { controller = "Administration", action = "AddProductToCollection" }
            //);
            //routes.MapRoute(
            //    name: "RemoveProductFromCollection",
            //    url: "Administration/RemoveProductFromCollection",
            //    defaults: new { controller = "Administration", action = "RemoveProductFromCollection" }
            //);
            //routes.MapRoute(
            //    name: "UploadImage",
            //    url: "Administration/UploadImage",
            //    defaults: new { controller = "Administration", action = "UploadImage" }
            //);
            //routes.MapRoute(
            //    name: "RemoveImage",
            //    url: "Administration/RemoveImage",
            //    defaults: new { controller = "Administration", action = "RemoveImage" }
            //);



            //administration
            //routes.MapRoute(
            //    name: "GetTags",
            //    url: "Administration/_GetTags",
            //    defaults: new { controller = "Administration", action = "GetTags" }
            //);
            //routes.MapRoute(
            //    name: "SaveTag",
            //    url: "Administration/_SaveTag",
            //    defaults: new { controller = "Administration", action = "SaveTag" }
            //);
            //routes.MapRoute(
            //    name: "RemoveTag",
            //    url: "Administration/_RemoveTag",
            //    defaults: new { controller = "Administration", action = "RemoveTag" }
            //);
            //routes.MapRoute(
            //    name: "SaveParam",
            //    url: "Administration/_SaveParam",
            //    defaults: new { controller = "Administration", action = "_SaveParam" }
            //);
            //routes.MapRoute(
            //    name: "RemoveParam",
            //    url: "Administration/_RemoveParam",
            //    defaults: new { controller = "Administration", action = "_RemoveParam" }
            //);
            //routes.MapRoute(
            //    name: "AddCategory",
            //    url: "Administration/AddCategory",
            //    defaults: new { controller = "Administration", action = "AddCategory" }
            //);
            //routes.MapRoute(
            //    name: "RemoveCategory",
            //    url: "Administration/RemoveCategory",
            //    defaults: new { controller = "Administration", action = "RemoveCategory" }
            //);
            //routes.MapRoute(
            //    name: "DeleteConversation",
            //    url: "Administration/DeleteConversation",
            //    defaults: new { controller = "Administration", action = "DeleteConversation" }
            //);
            //routes.MapRoute(
            //    name: "RemoveFavorite2",
            //    url: "Administration/RemoveFavorite",
            //    defaults: new { controller = "Administration", action = "RemoveFavorite" }
            //);
            //routes.MapRoute(
            //    name: "RemoveFollowing2",
            //    url: "Administration/RemoveFollowing",
            //    defaults: new { controller = "Administration", action = "RemoveFollowing" }
            //);
            //routes.MapRoute(
            //    name: "CreateFolder",
            //    url: "Administration/CreateFolder",
            //    defaults: new { controller = "Administration", action = "CreateFolder" }
            //);
            //routes.MapRoute(
            //    name: "MoveFolder",
            //    url: "Administration/MoveFolder",
            //    defaults: new { controller = "Administration", action = "MoveFolder" }
            //);
            //routes.MapRoute(
            //    name: "DeleteFolder",
            //    url: "Administration/DeleteFolder",
            //    defaults: new { controller = "Administration", action = "DeleteFolder" }
            //);
            //routes.MapRoute(
            //    name: "SetFolder",
            //    url: "Administration/SetFolder",
            //    defaults: new { controller = "Administration", action = "SetFolder" }
            //);
            //routes.MapRoute(
            //    name: "GetEmail",
            //    url: "Administration/GetEmail",
            //    defaults: new { controller = "Administration", action = "GetEmail" }
            //);
            //routes.MapRoute(
            //    name: "GetAddressTown",
            //    url: "Administration/GetAddressTown",
            //    defaults: new { controller = "Administration", action = "GetAddressTown" }
            //);
            //routes.MapRoute(
            //    name: "SetActive",
            //    url: "Administration/SetActive",
            //    defaults: new { controller = "Administration", action = "SetActive" }
            //);
            //routes.MapRoute(
            //    name: "ChangeBoothId",
            //    url: "Administration/ChangeBoothId",
            //    defaults: new { controller = "Administration", action = "ChangeBoothId" }
            //);
            //routes.MapRoute(
            //    name: "Feedback",
            //    url: "Administration/Feedback",
            //    defaults: new { controller = "Administration", action = "Feedback" }
            //);
            //routes.MapRoute(
            //    name: "ErrorPage2",
            //    url: "administration/fejlside",
            //    defaults: new { controller = "Administration", action = "ErrorPage" }
            //);
            //routes.MapRoute(
            //    name: "NotFound2",
            //    url: "administration/not_found",
            //    defaults: new { controller = "Administration", action = "NotFound" }
            //);
            //routes.MapRoute(
            //    name: "AjaxError",
            //    url: "Administration/AjaxError",
            //    defaults: new { controller = "Administration", action = "AjaxError" }
            //); 
            //routes.MapRoute(
            //    name: "AjaxErrorReturn2",
            //    url: "Administration/AjaxErrorReturn",
            //    defaults: new { controller = "Administration", action = "AjaxErrorReturn" }
            //);






            //marketplace
            //routes.MapRoute(
            //    name: "ErrorPage1",
            //    url: "markedsplads/fejlside",
            //    defaults: new { controller = "Marketplace", action = "ErrorPage" }
            //);
            //routes.MapRoute(
            //    name: "NotFound1",
            //    url: "marketplace/not_found",
            //    defaults: new { controller = "Marketplace", action = "NotFound" }
            //); 
            //routes.MapRoute(
            //    name: "AjaxErrorReturn1",
            //    url: "Marketplace/AjaxErrorReturn",
            //    defaults: new { controller = "Marketplace", action = "AjaxErrorReturn" }
            //); 





            //marketplace
            //routes.MapRoute(
            //    name: "Product",
            //    url: "front/produkt/{id}",
            //    defaults: new { controller = "Marketplace", action = "Product" }
            //);
            //routes.MapRoute(
            //    name: "Collection",
            //    url: "front/samling/{id}",
            //    defaults: new { controller = "Marketplace", action = "Collection" }
            //);
            //routes.MapRoute(
            //    name: "Booth",
            //    url: "front/bod/{id}",
            //    defaults: new { controller = "Marketplace", action = "Booth"/*, a_sub = UrlParameter.Optional, b_sub = UrlParameter.Optional */},
            //    constraints: new { id = @"\d+" }
            //);
            //routes.MapRoute(
            //    name: "Marketplace",
            //    url: "front/markedsplads",
            //    defaults: new { controller = "Marketplace", action = "Marketplace"/*, c = UrlParameter.Optional*/ }
            //);







            //routes.MapRoute(
            //    name: "Home1",
            //    url: "",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
            ////////////////////////routes.MapRoute(
            ////////////////////////    name: "Home2",
            ////////////////////////    url: "",
            ////////////////////////    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            ////////////////////////);



            //account
            routes.MapRoute(
                name: "Login1",
                url: "konto/login",
                defaults: new { controller = "Account", action = "Login" }//,
                //constraints: new { msg = "login" }
            );
            routes.MapRoute(
                name: "LogOff",
                url: "logoff",
                defaults: new { controller = "Account", action = "LogOff" }
            );
            ////////////////////routes.MapRoute(
            ////////////////////    name: "Login2",
            ////////////////////    url: "Account/Login",
            ////////////////////    defaults: new { controller = "Account", action = "Login" }//,
            ////////////////////    //constraints: new { msg = "login" }
            ////////////////////);
            routes.MapRoute(
                name: "Register1",
                url: "konto/registrer",
                defaults: new { controller = "Account", action = "Register" }//,
                //constraints: new { msg = "login" }
            );
            ////////////////////routes.MapRoute(
            ////////////////////     name: "Register2",
            ////////////////////     url: "Account/Register",
            ////////////////////     defaults: new { controller = "Account", action = "Register" }//,
            ////////////////////                                                                  //constraints: new { msg = "login" }
            //////////////////// );
            routes.MapRoute(
                name: "ConfirmEmail1",
                url: "konto/bekræft",
                defaults: new { controller = "Account", action = "ConfirmEmail" }//,
                //constraints: new { msg = "login" }
            );
            //////////////////routes.MapRoute(
            //////////////////    name: "ConfirmEmail2",
            //////////////////    url: "Account/ConfirmEmail",
            //////////////////    defaults: new { controller = "Account", action = "ConfirmEmail" }//,
            //////////////////    //constraints: new { msg = "login" }
            //////////////////);
            routes.MapRoute(
                name: "ForgotPassword",
                url: "konto/glemtkodeord",
                defaults: new { controller = "Account", action = "ForgotPassword" }
            );
            routes.MapRoute(
                name: "ForgotPasswordConfirmation",
                url: "konto/glemtkodeordbekræftet",
                defaults: new { controller = "Account", action = "ForgotPasswordConfirmation" }
            );
            routes.MapRoute(
                name: "ResetPassword",
                url: "konto/ændrkodeord",
                defaults: new { controller = "Account", action = "ResetPassword" }
            );
            routes.MapRoute(
                name: "ResetPasswordConfirmation",
                url: "konto/ændrkodeordbekræftet",
                defaults: new { controller = "Account", action = "ResetPasswordConfirmation" }
            );


            //routes.MapRoute(
            //    name: "MessageA",
            //    url: "beskeder/skriv_besked/{id}/{type}",
            //    defaults: new { controller = "Marketplace", action = "MessageA" }
            //);
            //routes.MapRoute(
            //    name: "MessageB",
            //    url: "beskeder/skriv_besked/{id}/{owner}/{type}",
            //    defaults: new { controller = "Marketplace", action = "MessageB" }
            //);
            //routes.MapRoute(
            //    name: "Message",
            //    url: "beskeder/post",
            //    defaults: new { controller = "Marketplace", action = "Message" }
            //);

            routes.MapRoute(
                name: "CatchAll",
                url: "{*any}",
                defaults: new { controller = "Marketplace", action = "ErrorPage"}
            );
        }
    }
}
