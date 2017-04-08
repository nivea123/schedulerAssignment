using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EmployeeSchedulerAssignment.Services;
using EmployeeSchedulerAssignment.EmployeeScheduler;

namespace EmployeeSchedulerAssignment.Controllers
{
    public class EmployeesController : Controller
    {
        // GET: /Employees/
        public ActionResult Index()
        {
            var employees = EmployeeScheduleApi.GetEmployees();
            return View(employees.OrderBy(s => s.name));
        }
          
        [Route("employees/id/{id}")]
        public ActionResult Calendar(int id)
        {
            return View(Scheduler.GetSchedule(id));
        }
    }
}