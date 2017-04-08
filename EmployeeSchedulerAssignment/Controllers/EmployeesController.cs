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
        private List<CalendarEvent> calendarEvents = new List<CalendarEvent>();
        private Dictionary<int, List<EmployeeSchedule>> scheduleByWeeks = new Dictionary<int, List<EmployeeSchedule>>();

        // GET: /Employees/
        public ActionResult Index()
        {
            var employees = EmployeeScheduleApi.GetEmployees();
            return View(employees.OrderBy(s => s.name));
        }
          
        [Route("employees/id/{id}")]
        public ActionResult Calendar(int id)
        {
            return View(GetSchedule(id));
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
        private bool GenerateSchedule(ref IEnumerable<Employee> employees, ref string errorString)
        {
            IEnumerable<TimeOffRequest> timeOffRequests = null;
            int? employeePerShiftValue = null;

            if (!RetrieveData(ref employees, ref timeOffRequests, ref employeePerShiftValue, ref errorString))
                return false;

            // Schedule each employee for weeks 23 to 26
            int currentEmpIndex = 0;
            int totalNumShifts = 0;
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

                        // Check employee time-off request
                        bool isAvaliable = true;
                        foreach (var request in timeOffRequests)
                        {
                            if ((request.employee_id == currentEmp.id) && (request.week == weekNo) && (request.days.Contains(dayNo)))
                                isAvaliable = false;
                        }

                        if (isAvaliable)
                        {
                            daysByEmployeeId[currentEmp.id].Add(dayNo);
                            totalNumShifts++;
                        }

                        currentEmpIndex++;
                        if (currentEmpIndex >= employees.Count())
                            currentEmpIndex = 0;
                    }
                }

                var schedules = new List<EmployeeSchedule>();
                foreach (int key in daysByEmployeeId.Keys)
                {
                    schedules.Add(new EmployeeSchedule() { employee_id = key, schedule = daysByEmployeeId[key] });
                }

                scheduleByWeeks.Add(weekNo, schedules);
            }

            // TODO - Write results out to JSON file so we are not generating results again

            // Check if there are enough employees available to work 
            // shift requirements with employee time-off requests
            int requiredShifts = (int)employeePerShiftValue * 28; // 4 weeks * 7 days

            if (totalNumShifts < requiredShifts)
                errorString = String.Format("NOTE:  There are not enough employees to cover required shift of {0} employees per shift", employeePerShiftValue);

            return true;
        }

        private bool RetrieveData(ref IEnumerable<Employee> employees, ref IEnumerable<TimeOffRequest> timeOffRequests, 
            ref int? employeePerShiftValue, ref string errorString)
        {
            // Return if there are no employees
            employees = EmployeeScheduleApi.GetEmployees().OrderBy(s => s.name);
            if (employees.Count() == 0)
            {
                errorString = String.Format("Could not retrieve any employee information.");
                return false;
            }

            // Feature 1 - EMPLOYEES_PER_SHIFT
            var ruleDefinitions = EmployeeScheduleApi.GetRuleDefinitions();
            var ruleDefinition = ruleDefinitions.Where(r => r.value == "EMPLOYEES_PER_SHIFT").FirstOrDefault();
            if (ruleDefinition == null)
            {
                errorString = String.Format("Could not retrieve rule definition 'EMPLOYEES_PER_SHIFT' information.");
                return false;
            }
            int? employeePerShiftId = ruleDefinition.id;

            // Assuming there's exactly one row for EMPLOYEES_PER_SHIFT rule
            // TODO - Loop through shiftRules to find all rules that apply to EMPLOYESS_PER_SHIFT 
            var shiftRules = EmployeeScheduleApi.GetShiftRules();
            var shiftRule = shiftRules.Where(s => s.rule_id == employeePerShiftId).FirstOrDefault();
            if (shiftRule == null)
            {
                errorString = String.Format("EMPLOYEES_PER_SHIFT rule is not configured for any shift rules.");
                return false;
            }
            employeePerShiftValue = shiftRule.value;

            // employee_id in shiftRule is an optional parameter, empty value means ALL employee
            // TODO - identify employees with specific rule by looping through shiftRule by employee
            //        create a list that contains if employee should be included in the rule
            int? employeePerShiftEmpId = shiftRule.employee_id;

            // Feature 2 - Account for emplolyee time-off requests
            timeOffRequests = EmployeeScheduleApi.GetEmployeeTimeOffRequests();

            return true;
        }


        private CalendarScheduleViewModel GetSchedule(int id)
        {
            IEnumerable<Employee> employees = null;
            string employeeName = null;
            string errorString = null;

            if (GenerateSchedule(ref employees, ref errorString))
            {
                employeeName = employees.Where(e => e.id == id).FirstOrDefault().name;

                // Return list of weekSchedules filtered by passed in employee 'id'
                var weekStartDates = EmployeeScheduleApi.Get2015CalendarWeeks();
                DateTime parsedDate;

                foreach (var week in scheduleByWeeks)
                {
                    var weekStartDate = weekStartDates.Where(w => w.id == week.Key).FirstOrDefault().start_date;
                    var employeeSchedule = week.Value;

                    foreach (var sched in employeeSchedule)
                    {
                        if (sched.employee_id == id)
                        {
                            foreach (var day in sched.schedule)
                            {
                                if (DateTime.TryParseExact(weekStartDate, "yyyy/MM/dd", null, DateTimeStyles.None, out parsedDate))
                                {
                                    calendarEvents.Add(new CalendarEvent() { title = "Work", start = parsedDate.AddDays(day - 1).ToString("yyyy-MM-dd") });
                                }
                                else
                                {
                                    return new CalendarScheduleViewModel()
                                    { errorString = String.Format("Could not parse JSON date.  Expected date format: 'yyyy/MM/dd'.") };
                                }
                            }

                        }
                    }
                }

            }
            else
            {
                return new CalendarScheduleViewModel() { errorString = errorString };
            }

            var viewModel = new CalendarScheduleViewModel()
            {
                employeeName = employeeName,
                calendarEvents = calendarEvents,
                errorString = errorString
            };

            return viewModel;
        }
#endregion

    }
}