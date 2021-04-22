using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuizApp.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Admin()
        {
            var welcome = TempData["ViewBag"];
            ViewBag.Message = welcome;
            return View();
        }
    }
}