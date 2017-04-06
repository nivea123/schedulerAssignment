using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EmployeeSchedulerAssignment.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Employee Scheduler Assignment";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Email:  deanne.mann@gmail.com";

            return View();
        }
    }
}