using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using www.e_bazar.dk.Controllers;
using www.e_bazar.dk.Models;
using www.e_bazar.dk.Models.DataAccess;
using www.e_bazar.dk.Models.DTOs;
using www.e_bazar.dk.SharedClasses;

namespace UnitTestEbazarDK
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Assert.AreEqual(true, false, "equal");
            //Assert.IsTrue(true);
        }
    }
    [TestClass]
    public class Marketplace
    {
        MarketplaceController marketplace;
        AdministrationController adm;
        DataAccessLayer dal = new DataAccessLayer(new EbazarDB());
        long product_id = 22;//Dark Crystal
        int collection_id = 9;//Pink Floyd
        int booth_id = 1;//Bernies Bazar
        string salesman_id = "e92d2833-995c-412f-9342-4e35d34d1375";//e-bernies@hotmail.com
        string customer_id = "1d8606b0-d125-4ed4-930d-4bc0c2e1bf47";//joakimjacobsen_441@hotmail.com
        string salesman_name = "e-bernies@hotmail.com";
        string customer_name = "joakimjacobsen_441@hotmail.com";

        biz_salesman salesman_poco;// = new biz_salesman();
        biz_customer customer_poco;// = new biz_customer();
        biz_booth booth_poco;// = new biz_booth();
        biz_product product_poco;// = new biz_product();
        
        private biz_salesman SetupSalesman()
        {
            salesman_poco = new biz_salesman();
            salesman_poco.created_on = DateTime.Now;
            salesman_poco.descriminator = "Salesman";
            salesman_poco.description = "TEST";
            salesman_poco.email = "joakimjacobsen_441@hotmail.com";
            salesman_poco.firstname = "TESTER";
            salesman_poco.lastname = "TESTER";
            salesman_poco.iscreated = false;
            salesman_poco.person_id = "abc";
            salesman_poco.phonenumber = 12345678;

            return salesman_poco;
        }

        private biz_customer SetupCustomer()
        {
            customer_poco = new biz_customer();
            customer_poco.created_on = DateTime.Now;
            customer_poco.descriminator = "Salesman";
            
            customer_poco.email = "joakimjacobsen_441@hotmail.com";
            customer_poco.firstname = "TESTER";
            customer_poco.lastname = "TESTER";
            customer_poco.iscreated = false;
            customer_poco.person_id = "def";
            

            return customer_poco;
        }

        private biz_booth SetupBoothPoco(int booth_id, bool withsysname)
        {
            biz_salesman salesman_poco = new biz_salesman();
            biz_booth booth_poco = new biz_booth();
            if (booth_id != -1)
                booth_poco.booth_id = booth_id;
            booth_poco.salesman_poco = salesman_poco.GetPersonPOCO<biz_salesman>(CurrentUser.GetInstance().UserID, false, true, false);
            booth_poco.salesman_id = CurrentUser.GetInstance().UserID;
            booth_poco.fulladdress = false;
            booth_poco.fulladdress_str = "Part";
            booth_poco.name = "TEST_BOOTH";
            if (withsysname)
                booth_poco.sysname = "TEST_{1234}";
            booth_poco.region_poco = new biz_region();
            booth_poco.region_poco.town = "søborg";
            booth_poco.region_poco.zip = 2860;
            //booth_poco.tag_pocos.Add(new biz_tag(new EbazarDB()) { name = "testproduct", form = "booth" });
            //booth_poco.tag_pocos.Add(new biz_tag(new EbazarDB()) { name = "testcollection", form = "booth" });

            return booth_poco;
        }

        private biz_product SetupProductPoco(biz_booth booth_poco, long product_id/*, long tag_id*/, bool withsysname)
        {
            biz_product product_poco = new biz_product(null, false);
            if (product_id != -1)
                product_poco.id = product_id;
            product_poco.booth_poco = booth_poco;
            product_poco.booth_id = booth_poco.booth_id;
            product_poco.category_main_id = -1;////////////////////////////////////////////////////////////////////////////////////////////////
            product_poco.name = "TEST_PRODUCT";
            if (withsysname)
                product_poco.sysname = "TEST_{1234}";
            //product_poco.created_on = DateTime.Now;
            product_poco.description = NOP.NO_DESCRIPTION.ToString();
            product_poco.note = NOP.NO_NOTE.ToString();
            product_poco.no_of_units = 1;
            product_poco.price = "GRATIS";
            product_poco.status_condition = CONDITION.VELHOLDT.ToString();
            product_poco.status_stock = STOCK.PÅ_LAGER.ToString();
            product_poco.tag_pocos = null;
            return product_poco;
        }
        private void Setup(bool withpersons)
        {
            if (withpersons)
            {
                salesman_poco = SetupSalesman();
                dal.SavePerson<biz_salesman>(salesman_poco);
                //dto_userprofile salesman_dto = new dto_userprofile(null, salesman_poco, null);
                //adm.SalesmanProfile(salesman_dto);
                customer_poco = SetupCustomer();
                dal.SavePerson<biz_customer>(customer_poco);
                //dto_userprofile customer_dto = new dto_userprofile(null, null, customer_poco);
                //adm.CustomerProfile(customer_dto);
            }
            booth_poco = SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "testproduct", "booth");
            long id = adm.GetDataAccessLayer().GetTagIdByName_FORTEST("testproduct");
            product_poco= SetupProductPoco(booth_poco/*, -1*/, id, false);
            adm.CreateProduct(product_poco);
            //biz_product product_poco = new biz_product();
            //product_poco = dal.GetProductPOCO(product_poco.id, false, false, false);
        }

        [TestMethod]
        public void Marketplace_Generel()
        {
            CurrentUser.GetInstance().Login("", "", false);
            marketplace = new MarketplaceController();

            ActionResult ar1 = marketplace.Marketplace(null, "", "", "", "", "", "", "");
            ActionResult ar2 = marketplace.Booth(booth_id, "");
            ActionResult ar3 = marketplace.Product(product_id);
            ActionResult ar4 = marketplace.Collection(collection_id);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Marketplace_MessageGET()
        {
            marketplace = new MarketplaceController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);

            ActionResult ar5 = marketplace.MessageGet(product_id, "product");//(long id, bool is_product)Dark Crystal
            ActionResult ar6 = marketplace.MessageGet(collection_id, "collection");//(long id, bool is_product)Pink Floyd

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Marketplace_MessagePOST()
        {
            adm = new AdministrationController();
            marketplace = new MarketplaceController();

            CurrentUser.GetInstance().Login("abc", "TESTER", true);
            Setup(false);

            /*biz_booth booth = SetupBoothPoco(-1, false);
            adm.CreateBooth(booth);
            adm.SaveTag(booth.booth_id + "", "testproduct", "booth", "booth");
            long id = adm.GetDataAccessLayer().GetTagIdByName_FORTEST("testproduct");
            biz_product poco = SetupProductPoco(booth, id, false);
            adm.CreateProduct(poco);
            biz_product product_poco = new biz_product();
            product_poco = dal.GetProductPOCO(poco.id, false, false, false);*/

            biz_person current_user = null;// CurrentUser.GetInstance().GetCurrentUser(true, false, true);
            //biz_salesman salesman_poco = new biz_salesman();
            salesman_poco = product_poco.booth_poco.salesman_poco;
                        
            col_message mess = new col_message();
            mess.type = TYPE.PRODUCT;
            mess.id = product_poco.id;
            mess.conversation = dal.GetConversation(product_poco.id, current_user != null ? current_user.person_id : "", TYPE.PRODUCT);
            mess.conversation_id = mess.conversation.conversation_id;
            
            mess.product_owner = salesman_poco;
            mess.other = current_user;//vil altid være den første

            mess.product_owner_id = salesman_poco.person_id;
            mess.other_id = mess.other.person_id;

            mess.product_owner_email = salesman_poco.email;
            mess.other_email = mess.other.email;

            mess.product_owner_firstname = salesman_poco.firstname;
            mess.other_firstname = mess.other.firstname;

            mess.message = "TEST " + DateTime.Now;
            ActionResult ar7 = marketplace.Message(mess);

            Assert.IsTrue(true);
        }
        /*[TestMethod]
        public void AddFavorite()
        {
            ActionResult ar8 = marketplace.AddFavorite(product_id, -1);
            ActionResult ar9 = marketplace.AddFavorite(-1, collection_id);

            Assert.IsTrue(true);
        }*/
    }
    [TestClass]
    public class Administration
    {
        EbazarDB db = new EbazarDB();
        AdministrationController adm;
        //DataAccessLayer dal = new DataAccessLayer(new EbazarDB());
        long product_id = 22;//Dark Crystal
        int collection_id = 9;//Pink Floyd
        int booth_id = -1;//Bernies Bazar
        string salesman_id = "e92d2833-995c-412f-9342-4e35d34d1375";//e-bernies@hotmail.com
        string customer_id = "c6f58858-928a-43c1-9ad8-3a9553f57136";//joakimjacobsen_441@hotmail.com
        string salesman_name = "e-bernies@hotmail.com";
        string customer_name = "joakimjacobsen_441@hotmail.com";


        private biz_booth SetupBoothPoco(int booth_id, bool withsysname)
        {
            biz_salesman salesman_poco = new biz_salesman();
            biz_booth booth_poco = new biz_booth();
            if(booth_id != -1)
                booth_poco.booth_id = booth_id;
            booth_poco.salesman_poco = salesman_poco.GetPersonPOCO<biz_salesman>(CurrentUser.GetInstance().UserID, false, true, false);
            booth_poco.salesman_id = CurrentUser.GetInstance().UserID;
            booth_poco.fulladdress = false;
            booth_poco.fulladdress_str = "Part";
            booth_poco.name = "TEST_BOOTH";
            if(withsysname)
                booth_poco.sysname = "TEST_{1234}";
            booth_poco.region_poco = new biz_region();
            booth_poco.region_poco.town = "søborg";
            booth_poco.region_poco.zip = 2860;
            //booth_poco.tag_pocos.Add(new biz_tag(new EbazarDB()) { name = "testproduct", form = "booth" });
            //booth_poco.tag_pocos.Add(new biz_tag(new EbazarDB()) { name = "testcollection", form = "booth" });

            return booth_poco;
        }
        private biz_product SetupProductPoco(biz_booth booth_poco, long product_id, bool withsysname)
        {
            biz_product product_poco = new biz_product(null, false);
            if (product_id != -1)
                product_poco.id = product_id;
            product_poco.booth_poco = booth_poco;
            product_poco.booth_id = booth_poco.booth_id;
            product_poco.category_main_id = -1;/////////////////////////////////////////////////////////////////////////////////////////////////
            product_poco.name = "TEST_PRODUCT";
            if (withsysname)
                product_poco.sysname = "TEST_{1234}";
            //product_poco.created_on = DateTime.Now;
            product_poco.description = NOP.NO_DESCRIPTION.ToString();
            product_poco.note = NOP.NO_NOTE.ToString();
            product_poco.no_of_units = 1;
            product_poco.price = "GRATIS";
            product_poco.status_condition = CONDITION.VELHOLDT.ToString();
            product_poco.status_stock = STOCK.PÅ_LAGER.ToString();
            product_poco.tag_pocos = null;
            return product_poco;
        }
        private biz_collection SetupCollectionPoco(biz_booth booth_poco, int collection_id, bool withsysname)
        {
            biz_collection collection_poco = new biz_collection(null);
            if (collection_id != -1)
                collection_poco.id = collection_id;
            collection_poco.booth_poco = booth_poco;
            collection_poco.booth_id = booth_poco.booth_id;
            collection_poco.category_main_id = -1;///////////////////////////////////////////////////////////////////////////////////////////// 
            collection_poco.name = "TEST_COLLECTION";
            if (withsysname)
                collection_poco.sysname = "TEST_{1234}";
            //product_poco.created_on = DateTime.Now;
            collection_poco.description = NOP.NO_DESCRIPTION.ToString();
            collection_poco.note = NOP.NO_NOTE.ToString();
            //collection_poco.no_of_units = 1;
            collection_poco.price = "GRATIS";
            collection_poco.status_condition = CONDITION.VELHOLDT.ToString();
            collection_poco.status_stock = STOCK.PÅ_LAGER.ToString();
            collection_poco.tag_pocos = null;
            return collection_poco;
        }

        [TestMethod]
        public void Adm_UserProfileGET_Salesman()
        {
            /*var mocks = new MockRepository(MockBehavior.Default);
            Mock<IPrincipal> mockPrincipal = mocks.Create<IPrincipal>();
            
            mockPrincipal.Setup(p => p.Identity.GetUserId()).Returns(salesman_id);
            mockPrincipal.Setup(p => p.Identity.GetUserName()).Returns("e-bernies@hotmail.com");
            mockPrincipal.Setup(p => p.Identity.IsAuthenticated).Returns(true);

            var contextMock = new Mock<HttpContextBase>();
            contextMock.SetupGet(ctx => ctx.User)
                       .Returns(mockPrincipal.Object);

            var controllerContextMock = new Mock<ControllerContext>();
            controllerContextMock.SetupGet(con => con.HttpContext)
                                 .Returns(contextMock.Object);
            adm.ControllerContext = controllerContextMock.Object;*/
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            ActionResult ar8 = adm.UserProfile();

            ar8.AssertViewWasReturned("SalesmanProfile", "TEST");
        }
        [TestMethod]
        public void Adm_UserProfileGET_Customer()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(customer_id, customer_name, true);
            ActionResult ar8 = adm.UserProfile();// as ActionResult as RedirectToRouteResult;
            //Assert.AreEqual("CustomerProfile", ar8.RouteValues["action"].ToString());

            ar8.AssertViewWasReturned("CustomerProfile", "TEST");
        }
        [TestMethod]
        public void Adm_SalesmanProfileGET()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            List<biz_booth> booth_pocos = new List<biz_booth>();
            col_userprofile profile = new col_userprofile();
            biz_salesman salesman_poco = (biz_salesman)adm.GetDataAccessLayer().GetPersonPOCO<biz_salesman>(CurrentUser.GetInstance().UserID, false, false, true);
            profile.salesman_poco = salesman_poco;
            booth_pocos = adm.GetDataAccessLayer().GetBoothPOCOs(salesman_id, false);
            ActionResult ar8 = adm.SalesmanProfile(profile);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Adm_CustomerProfileGET()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(customer_id, customer_name, true);
            List<biz_booth> booth_pocos = new List<biz_booth>();
            col_userprofile profile = new col_userprofile();
            biz_customer customer_poco = (biz_customer)adm.GetDataAccessLayer().GetPersonPOCO<biz_customer>(CurrentUser.GetInstance().UserID, false, false, true);
            profile.customer_poco = customer_poco;

            booth_pocos = adm.GetDataAccessLayer().GetBoothPOCOs(salesman_id, false);
            ActionResult ar8 = adm.CustomerProfile(profile);

            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Booth_CreateEditDeleteGETPOST_OK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);

            ActionResult ar8 = adm.CreateBooth();
            ar8.AssertViewWasReturned("CreateBooth", "TEST_BOOTH");

            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            RedirectToRouteResult ar9 = adm.CreateBooth(booth_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar9.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, false);
            ActionResult ar10 = adm.EditBooth(booth_poco.booth_id);
            ar10.AssertViewWasReturned("EditBooth", "TEST_BOOTH");

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, false);
            booth_poco.name = "TEST_BOOTH2";
            col_folders cat = new col_folders(null, "");
            col_booth dto = new col_booth(booth_poco, cat, null, null, null, null, false, -1);/////////////////////////////////ved ikke om parametrene passer
            RedirectToRouteResult ar11 = adm.EditBooth(dto) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar11.RouteValues["action"].ToString());

            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth");/////////////////////////////////ved ikke om parametrene passer
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT2", "booth");/////////////////////////////////ved ikke om parametrene passer
            long id = adm.GetDataAccessLayer().GetTagIdByName_FORTEST("testproduct2");
            adm.RemoveTag(id + "", booth_poco.booth_id + "", "BOOTH");
            JsonResult ar12 = adm.GetTags("TEST_PRODUCT");
            Assert.IsNotNull(ar12);
            

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, false);
            RedirectToRouteResult ar13 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar13.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Booth_CreateGETPOST_CreateNotOK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);

            ActionResult ar8 = adm.CreateBooth();
            ar8.AssertViewWasReturned("CreateBooth", "TEST_BOOTH");

            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            booth_poco.name = "";
            RedirectToRouteResult ar9 = adm.CreateBooth(booth_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("CreateBooth", ar9.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Booth_CreateEditDeleteGETPOST_EditNotOK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);

            ActionResult ar8 = adm.CreateBooth();
            ar8.AssertViewWasReturned("CreateBooth", "TEST_BOOTH");

            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            RedirectToRouteResult ar10 = adm.CreateBooth(booth_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar10.RouteValues["action"].ToString());

            /*booth_poco = this.SetupBoothPoco(booth_poco.booth_id, false);
            ActionResult ar9 = adm.EditBooth(booth_poco.booth_id);
            ar9.AssertViewWasReturned("EditBooth", "TEST_BOOTH");*/

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, false);
            booth_poco.name = "";
            col_folders cat = new col_folders(null, "");
            col_booth dto = new col_booth(booth_poco, cat, null, null, null, null, false, -1);/////////////////////////////////ved ikke om parametrene passer
            RedirectToRouteResult ar11 = adm.EditBooth(dto) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar11.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, false);
            RedirectToRouteResult ar12 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar12.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Product_CreateEditDeleteGETPOST_OK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth");//ved ikke om parametrene passer

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth", "booth");//ved ikke om parametrene passer
            biz_product product_poco = new biz_product(null, false);
            product_poco.booth_poco = booth_poco;
            ActionResult ar9 = adm.CreateProduct(booth_poco.booth_id);
            ar9.AssertViewWasReturned("CreateProduct", "TEST");

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);

            product_poco = this.SetupProductPoco(booth_poco, -1, false);
            RedirectToRouteResult ar10 = adm.CreateProduct(product_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditProduct", ar10.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            product_poco = this.SetupProductPoco(booth_poco, product_poco.id, true);
            product_poco.name = "TEST_PRODUCT2";
            RedirectToRouteResult ar11 = adm.EditProduct(product_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditProduct", ar11.RouteValues["action"].ToString());

            //adm.SaveTag(product_poco.id + "", "TEST_PRODUCT", "product");//ved ikke om parametrene passer
            //adm.SaveTag(product_poco.id + "", "TEST_PRODUCT2", "product");//ved ikke om parametrene passer
            long id = adm.GetDataAccessLayer().GetTagIdByName_FORTEST("testproduct2");
            adm.RemoveTag(id + "", product_poco.id + "", "product");
            JsonResult ar12 = adm.GetTags("TEST_PRODUCT");
            Assert.IsNotNull(ar12);

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            RedirectToRouteResult ar13 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar13.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Product_CreatePOST_CreateNotOK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth");//ved ikke om parametrene passer

            /*booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            biz_product product_poco = new biz_product();
            product_poco.booth_poco = booth_poco;
            ActionResult ar9 = adm.CreateProduct(booth_poco.booth_id);
            ar9.AssertViewWasReturned("CreateProduct", "TEST");*/

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            biz_product product_poco = this.SetupProductPoco(booth_poco, -1, false);
            product_poco.name = "";
            RedirectToRouteResult ar10 = adm.CreateProduct(product_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("CreateProduct", ar10.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            RedirectToRouteResult ar12 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar12.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Product_CreateEditDeleteGETPOST_EditNotOK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth");//ved ikke om parametrene passer

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            biz_product product_poco = this.SetupProductPoco(booth_poco, -1, false);
            RedirectToRouteResult ar10 = adm.CreateProduct(product_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditProduct", ar10.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            product_poco = this.SetupProductPoco(booth_poco, product_poco.id, true);
            product_poco.name = "";
            RedirectToRouteResult ar11 = adm.EditProduct(product_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditProduct", ar11.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            RedirectToRouteResult ar12 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar12.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Product_DeleteGET_OK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth");//ved ikke om parametrene passer

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            biz_product product_poco = this.SetupProductPoco(booth_poco, -1, false);
            RedirectToRouteResult ar10 = adm.CreateProduct(product_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditProduct", ar10.RouteValues["action"].ToString());

            product_poco = this.SetupProductPoco(booth_poco, product_poco.id, true);
            RedirectToRouteResult ar11 = adm.DeleteProduct(product_poco.id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar11.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            RedirectToRouteResult ar12 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar12.RouteValues["action"].ToString());
        }

        [TestMethod]
        public void Collection_CreateEditDeleteGETPOST_OK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_COLLECTION", "booth");//ved ikke om parametrene passer

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            biz_collection collection_poco = new biz_collection(null);
            collection_poco.booth_poco = booth_poco;
            ActionResult ar9 = adm.CreateCollection(booth_poco.booth_id);
            ar9.AssertViewWasReturned("CreateCollection", "TEST");

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            collection_poco = this.SetupCollectionPoco(booth_poco, -1, false);
            RedirectToRouteResult ar10 = adm.CreateCollection(collection_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditCollection", ar10.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            collection_poco = this.SetupCollectionPoco(booth_poco, (int)collection_poco.id, true);
            collection_poco.name = "Test_Collection2";
            RedirectToRouteResult ar11 = adm.EditCollection(collection_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditCollection", ar11.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth");//ved ikke om parametrene passer
            collection_poco = this.SetupCollectionPoco(booth_poco, (int)collection_poco.id, true);
            biz_product product_poco = this.SetupProductPoco(booth_poco, -1, false);
            adm.CreateProduct(product_poco);
            RedirectToRouteResult ar12 = adm.AddProductToCollection((int)collection_poco.id, product_poco.id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditCollection", ar12.RouteValues["action"].ToString());
            RedirectToRouteResult ar13 = adm.RemoveProductFromCollection((int)collection_poco.id, product_poco.id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditCollection", ar13.RouteValues["action"].ToString());
            adm.DeleteProduct(product_poco.id);

            //adm.SaveTag((int)collection_poco.id + "", "TEST_COLLECTION", "collection");//ved ikke om parametrene passer
            //adm.SaveTag((int)collection_poco.id + "", "TEST_COLLECTION2", "collection");//ved ikke om parametrene passer
            long id = adm.GetDataAccessLayer().GetTagIdByName_FORTEST("testcollection2");
            adm.RemoveTag(id + "", (int)collection_poco.id + "", "collection");
            JsonResult ar14 = adm.GetTags("TEST_COLLECTION");
            Assert.IsNotNull(ar14);

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            RedirectToRouteResult ar15 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar15.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Collection_CreatePOST_CreateNotOK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_COLLECTION", "booth");//ved ikke om parametrene passer

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            biz_collection collection_poco = this.SetupCollectionPoco(booth_poco, -1, false);
            collection_poco.name = "";
            RedirectToRouteResult ar10 = adm.CreateCollection(collection_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("CreateCollection", ar10.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            RedirectToRouteResult ar14 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar14.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Collection_CreateEditDeleteGETPOST_EditNotOK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_COLLECTION", "booth");//ved ikke om parametrene passer

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            //adm.SaveBoothTag("testcollection", booth_poco.booth_id + "");
            biz_collection collection_poco = this.SetupCollectionPoco(booth_poco, -1, false);
            RedirectToRouteResult ar10 = adm.CreateCollection(collection_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditCollection", ar10.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            collection_poco = this.SetupCollectionPoco(booth_poco, (int)collection_poco.id, true);
            collection_poco.name = "";
            RedirectToRouteResult ar11 = adm.EditCollection(collection_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditCollection", ar11.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            RedirectToRouteResult ar14 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar14.RouteValues["action"].ToString());
        }
        [TestMethod]
        public void Collection_DeleteGET_OK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_COLLECTION", "booth");//ved ikke om parametrene passer

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            biz_collection collection_poco = this.SetupCollectionPoco(booth_poco, -1, false);
            RedirectToRouteResult ar10 = adm.CreateCollection(collection_poco) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditCollection", ar10.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth");//ved ikke om parametrene passer
            collection_poco = this.SetupCollectionPoco(booth_poco, (int)collection_poco.id, true);
            biz_product product_poco = this.SetupProductPoco(booth_poco, -1, false);
            adm.CreateProduct(product_poco);
            RedirectToRouteResult ar12 = adm.AddProductToCollection((int)collection_poco.id, product_poco.id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditCollection", ar12.RouteValues["action"].ToString());

            collection_poco = this.SetupCollectionPoco(booth_poco, (int)collection_poco.id, true);
            RedirectToRouteResult ar11 = adm.DeleteCollection((int)collection_poco.id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar11.RouteValues["action"].ToString());

            booth_poco = this.SetupBoothPoco(booth_poco.booth_id, true);
            RedirectToRouteResult ar14 = adm.DeleteBooth(booth_poco.booth_id) as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("UserProfile", ar14.RouteValues["action"].ToString());
        }

        [TestMethod]
        public void Booth_Catalog_OK()
        {
            adm = new AdministrationController();

            CurrentUser.GetInstance().Login(salesman_id, salesman_name, true);
            biz_booth booth_poco = this.SetupBoothPoco(-1, false);
            adm.CreateBooth(booth_poco);
            adm = new AdministrationController();
            //adm.SaveTag(booth_poco.booth_id + "", "TEST_PRODUCT", "booth");//ved ikke om parametrene passer


            RedirectToRouteResult ar9 = adm.CreateFolder("testA", booth_poco.booth_id + ""/*lev_a eller booth*/, "lev_a", booth_poco.booth_id + "") as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar9.RouteValues["action"].ToString());
            int lev_ida = adm.GetDataAccessLayer().GetLevelId_FORTEST("lev_a", "testA");

            RedirectToRouteResult ar9a = adm.CreateFolder("testB", booth_poco.booth_id + ""/*lev_a eller booth*/, "lev_a", booth_poco.booth_id + "") as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar9a.RouteValues["action"].ToString());
            int lev_idb = adm.GetDataAccessLayer().GetLevelId_FORTEST("lev_a", "testB");

            RedirectToRouteResult ar9b = adm.CreateFolder("testC", booth_poco.booth_id + ""/*lev_a eller booth*/, "lev_a", booth_poco.booth_id + "") as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar9b.RouteValues["action"].ToString());
            int lev_idc = adm.GetDataAccessLayer().GetLevelId_FORTEST("lev_a", "testC");

            RedirectToRouteResult ar9c = adm.CreateFolder("testAA", lev_ida + ""/*lev_a eller booth*/, "lev_b", booth_poco.booth_id + "") as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar9c.RouteValues["action"].ToString());
            int lev_idaa = adm.GetDataAccessLayer().GetLevelId_FORTEST("lev_b", "testAA");


            RedirectToRouteResult ar10 = adm.MoveFolder(lev_idb + "", "down", booth_poco.booth_id + ""/*lev_a eller booth*/, "lev_a", booth_poco.booth_id + "") as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar10.RouteValues["action"].ToString());
            int? priority = adm.GetDataAccessLayer().GetLevelPriority_FORTEST("lev_a", "testB");
            Assert.AreEqual(2, priority);//skal der lige kigges på det er MEGET mærkeligt, den får ikke priority med(default value måske)

            RedirectToRouteResult ar11 = adm.DeleteFolder(lev_ida + "", booth_poco.booth_id + ""/*booth eller lev_a*/, "lev_a", booth_poco.booth_id + "") as ActionResult as RedirectToRouteResult;
            Assert.AreEqual("EditBooth", ar9.RouteValues["action"].ToString());

            //RedirectToRouteResult ar12 = adm.SetLevel(string lev_id, string id/*product eller collection*/, string type, string is_product) as ActionResult as RedirectToRouteResult;
            //RedirectToAction("EditCollection", new { collection_id = int.Parse(id) });

            //clean up
            adm.DeleteFolder(lev_idb + "", booth_poco.booth_id + ""/*booth eller lev_a*/, "lev_a", booth_poco.booth_id + "");
            adm.DeleteFolder(lev_idc + "", booth_poco.booth_id + ""/*booth eller lev_a*/, "lev_a", booth_poco.booth_id + "");
        }
        //public void UploadImage()
        //{
        //    HttpPostedFile file = new HttpPostedFile();
        //    string typeFile;
        //    adm.UploadImage();
        //}
        /*ActionResult ar8 = adm.UploadImage();
        ActionResult ar8 = adm.RemoveProductImage();
        ActionResult ar8 = adm.RemoveCollectionImage();
        ActionResult ar8 = adm.RemoveFavorite(long product_id, int collection_id);
        ActionResult ar8 = adm.GetAddressTown();*/

    }
    public static class Extension
    {
        public static void AssertViewWasReturned(this ActionResult result, string viewName, string defaultViewName)
        {
            Assert.IsInstanceOfType(result, typeof(ViewResultBase), "Result is not an instance of ViewResultBase");
            var viewResult = (ViewResultBase)result;

            var actualViewName = viewResult.ViewName;

            if (actualViewName == "")
                actualViewName = defaultViewName;

            Assert.AreEqual(viewName, actualViewName, string.Format("Expected a View named{0}, got a View named {1}", viewName, actualViewName));
        }
    }
}
