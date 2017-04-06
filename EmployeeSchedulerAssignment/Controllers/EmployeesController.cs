using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using EmployeeSchedulerAssignment.Models;
using EmployeeSchedulerAssignment.ViewModels;
using EmployeeSchedulerAssignment.Services;

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
            var calendarEvents = GenerateSchedule(id);
            return View(calendarEvents);
        }

#region Helpers
        /// <summary>
        /// Create the employee schedule using the EMPLOYEES_PER_SHIFT rule
        /// 
        /// Assumptions:
        /// * Each employee can only work a maximum of 5 days/week (40hours/week) because of labour rules
        /// * It is not stated if an employee has more seniority than another employee so this scheduler will sort
        ///   the employee name in alphabetical order and start scheduling from the top of list with the top of list meaning
        ///   they will be scheduled first.
        /// * There are enough employees to cover shift requests
        /// </summary>
        /// <param name="id">Employee Id</param>
        /// <returns>Employee's schedule for month of June 2015</returns>
        private CalendarScheduleViewModel GenerateSchedule(int id)
        {
            // Do nothing if there are no employees
            var employees = EmployeeScheduleApi.GetEmployees().OrderBy(s => s.name);  // Do this for now
            if (employees.Count() == 0)
                return null;

            // Feature 1 - EMPLOYEES_PER_SHIFT
            var ruleDefinitions = EmployeeScheduleApi.GetRuleDefinitions();

            var ruleDefinition = ruleDefinitions.Where(r => r.value == "EMPLOYEES_PER_SHIFT").FirstOrDefault();
            if (ruleDefinition == null)
                return null;

            int? employeePerShiftId = ruleDefinition.id;
            if (!employeePerShiftId.HasValue)
                return null;

            // Assuming there's exactly one row for EMPLOYEES_PER_SHIFT rule
            // TODO - Loop through shiftRules to find all rules that apply to EMPLOYESS_PER_SHIFT 
            var shiftRules = EmployeeScheduleApi.GetShiftRules();

            // TODO - Data validation checks:  rule_id exists in ruleDefinition list, employee_id exists in employees list, value not negative
            var shiftRule = shiftRules.Where(s => s.rule_id == employeePerShiftId).FirstOrDefault();
            if (shiftRule == null)
                return null;

            int? employeePerShiftValue = shiftRule.value;
            if (!employeePerShiftValue.HasValue)
                return null;

            // employee_id is optional parameter, empty value means ALL employee
            // TODO - identify employees with specific rule by looping through shiftRule by employee
            int? employeePerShiftEmpId = shiftRule.employee_id;

            // Schedule each employee for weeks 23 to 26
            var weekSchedules = new List<WeekSchedule>();

            int currentEmpIndex = 0;
            Employee currentEmp = new Employee();
            for (int weekNo = 23; weekNo <= 26; weekNo++)
            {
                var employeeSchedules = new List<EmployeeSchedule>();
                var daysByEmployeeId = new Dictionary<int, List<int>>();
                for (int dayNo = 1; dayNo <= 7; dayNo++)
                {
                    for (int shiftCount = 0; shiftCount < employeePerShiftValue; shiftCount++)
                    {
                        currentEmp = employees.ElementAt<Employee>(currentEmpIndex);
                        if (!daysByEmployeeId.ContainsKey(currentEmp.id))
                        {
                            daysByEmployeeId.Add(currentEmp.id, new List<int>());
                        }

                        var days = daysByEmployeeId[currentEmp.id];
                        days.Add(dayNo);

                        currentEmpIndex++;
                        if (currentEmpIndex >= employees.Count())
                            currentEmpIndex = 0;
                    }
                }

                var schedules = new List<EmployeeSchedule>();
                foreach(int key in daysByEmployeeId.Keys)
                {
                    schedules.Add(new EmployeeSchedule() { employee_id = key, schedule = daysByEmployeeId[key] });
                }

                weekSchedules.Add(new WeekSchedule() { week = weekNo, schedules = schedules });
            }
            // TODO - Write results out to JSON file so we are not generating results again

            // Return list of weekSchedules filtered by passed in employee 'id'
            //weekSchedules.RemoveAll(w => w.schedules.Where(e => e.employee_id == id));

            var calendarEvents = new List<CalendarEvent>()
            {
                new CalendarEvent(){ title = "Work", start = "2015-06-01" },
                new CalendarEvent(){ title = "Work", start = "2015-06-08" }
            };

            var viewModel = new CalendarScheduleViewModel() { employee_name = "Deanne Work MORE!!!", calendarEvents = calendarEvents };

            return viewModel;
        }
#endregion

    }
}