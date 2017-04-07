using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
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
        /// 1.  Each employee can work any amount of shifts
        /// 2.  It is not stated if an employee has more seniority than another employee so this scheduler will sort
        ///     the employee name in alphabetical order and start scheduling from the top of list 
        /// 3.  There are enough employees to cover shift requests
        /// </summary>
        /// <param name="requestedEmployeeId">Employee Id</param>
        /// <returns>Employee's schedule for month of June 2015</returns>
        private CalendarScheduleViewModel GenerateSchedule(int requestedEmployeeId)
        {
            // Return if there are no employees
            var employees = EmployeeScheduleApi.GetEmployees().OrderBy(s => s.name);
            if (employees.Count() == 0)
            {
                // TODO - add error message to viewModel
                return null;
            }
            // Feature 1 - EMPLOYEES_PER_SHIFT
            var ruleDefinitions = EmployeeScheduleApi.GetRuleDefinitions();
            var ruleDefinition = ruleDefinitions.Where(r => r.value == "EMPLOYEES_PER_SHIFT").FirstOrDefault();
            if (ruleDefinition == null)
            {
                // TODO - add error message to viewModel
                return null;
            }

            // Return if EMPLOYEE_PER_SHIFT is not used
            int? employeePerShiftId = ruleDefinition.id;
            if (!employeePerShiftId.HasValue)
            {
                // TODO - add error message to viewModel
                return null;
            }

            // Assuming there's exactly one row for EMPLOYEES_PER_SHIFT rule
            // TODO - Loop through shiftRules to find all rules that apply to EMPLOYESS_PER_SHIFT 
            var shiftRules = EmployeeScheduleApi.GetShiftRules();

            // TODO - Data validation checks:  rule_id exists in ruleDefinition list, employee_id exists in employees list, value not negative
            var shiftRule = shiftRules.Where(s => s.rule_id == employeePerShiftId).FirstOrDefault();
            if (shiftRule == null)
            {
                // TODO - add error message to viewModel
                return null;
            }

            int? employeePerShiftValue = shiftRule.value;
            if (!employeePerShiftValue.HasValue)
            {
                // TODO - add error message to viewModel
                return null;
            }

            // employee_id in shiftRule is an optional parameter, empty value means ALL employee
            // TODO - identify employees with specific rule by looping through shiftRule by employee
            //        create a list that contains if employee should be included in the rule
            int? employeePerShiftEmpId = shiftRule.employee_id;

            // Schedule each employee for weeks 23 to 26
            var scheduleByWeeks = new Dictionary<int, List<EmployeeSchedule>>();
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

                        daysByEmployeeId[currentEmp.id].Add(dayNo);

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

                scheduleByWeeks.Add(weekNo, schedules);
            }
            // TODO - Write results out to JSON file so we are not generating results again

            // Return list of weekSchedules filtered by passed in employee 'id'
            var weekStartDates = EmployeeScheduleApi.Get2015CalendarWeeks();
            var calendarEvents = new List<CalendarEvent>();
            DateTime parsedDate;

            foreach (var week in scheduleByWeeks)
            {
                var weekStartDate = weekStartDates.Where(w => w.id == week.Key).FirstOrDefault().start_date;
                var employeeSchedule = week.Value;

                foreach (var sched in employeeSchedule)
                {
                    if (sched.employee_id == requestedEmployeeId)
                    {
                        foreach (var day in sched.schedule)
                        {
                            if (DateTime.TryParseExact(weekStartDate, "yyyy/MM/dd", null, DateTimeStyles.None, out parsedDate))
                            {
                                calendarEvents.Add(new CalendarEvent() { title = "Work", start = parsedDate.AddDays(day - 1).ToString("yyyy-MM-dd") });
                            }
                        }

                    }
                }
            }

            var viewModel = new CalendarScheduleViewModel()
            {
                employee_name = employees.Where(e => e.id == requestedEmployeeId).FirstOrDefault().name,
                calendarEvents = calendarEvents
            };

            return viewModel;
        }
#endregion

    }
}