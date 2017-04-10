using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using EmployeeSchedulerAssignment.Services;
using EmployeeSchedulerAssignment.EmployeeScheduler;
using EmployeeSchedulerAssignment.Models;
using EmployeeSchedulerAssignment.ViewModels;

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

        
        [Route("PostAssignment/")]
        public ActionResult PostAssignment()
        {
            IEnumerable<Employee> employees = null;
            IEnumerable<TimeOffRequest> timeOffRequests = null;
            IEnumerable<Week> weekStartDates = null;
            int? employeePerShiftValue = null;
            string errorString = null;
            var scheduleByWeeks = new List<ScheduleByWeeks>();

            // Get data from JSON url and generate schedule
            if (Scheduler.RetrieveData(ref employees, ref timeOffRequests, ref weekStartDates, ref employeePerShiftValue, ref errorString))
                scheduleByWeeks = Scheduler.BuildScheduleByWeeks(employees, employeePerShiftValue, timeOffRequests, ref errorString);

            HttpWebResponse httpResponse = null;
            if (scheduleByWeeks != null)
            {
                // Serialize data and Post
                string serializedData = JSONHelper.JsonSerializer(scheduleByWeeks);
                httpResponse = EmployeeScheduleApi.PostSchedule(serializedData);               
            }
            
            return View(httpResponse);
        }

        [HttpPost]
        public ActionResult Calendar(Employee employee)
        {
            return Json(employee);
        }
    }
}