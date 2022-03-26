using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace www.e_bazar.dk.Controllers
{
    public class NavigationController : Controller
    {
        // GET: Navigation
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Menu()
        {
            return PartialView("_Navigation");
        }
    }
}