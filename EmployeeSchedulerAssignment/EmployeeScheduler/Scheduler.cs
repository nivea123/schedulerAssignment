using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using EmployeeSchedulerAssignment.Models;
using EmployeeSchedulerAssignment.ViewModels;
using EmployeeSchedulerAssignment.Services;

namespace EmployeeSchedulerAssignment.EmployeeScheduler
{
    public class Scheduler
    {
        /// <summary>
        /// Generate employee schedule and return requested employee schedule for June 2015
        /// </summary>
        /// <param name="id">Employee Id</param>
        /// <returns></returns>
        public static CalendarScheduleViewModel GetSchedule(int id)
        {
            IEnumerable<Employee> employees = null;
            IEnumerable<TimeOffRequest> timeOffRequests = null;
            IEnumerable<Week> weekStartDates = null;
            var scheduleByWeeks = new List<ScheduleByWeeks>();
            var calendarEvents = new List<CalendarEvent>();
            string employeeName = null;
            string errorString = null;
            int? employeePerShiftValue = null;

            // Get data from JSON url
            if (!RetrieveData(ref employees, ref timeOffRequests, ref weekStartDates, ref employeePerShiftValue, ref errorString))
                return new CalendarScheduleViewModel() { errorString = errorString };

            scheduleByWeeks = BuildScheduleByWeeks(employees, employeePerShiftValue, timeOffRequests, ref errorString);
            if (scheduleByWeeks == null)
                return new CalendarScheduleViewModel() { errorString = errorString };

            // Build calendar event record for use by fullcalendar 
            calendarEvents = BuildCalendarEvents(id, scheduleByWeeks, weekStartDates, employees, ref employeeName, ref errorString);
            if (calendarEvents == null && !String.IsNullOrEmpty(errorString))
                return new CalendarScheduleViewModel() { errorString = errorString };

            var viewModel = new CalendarScheduleViewModel()
            {
                employeeName = employeeName,
                calendarEvents = calendarEvents,
                errorString = errorString
            };

            return viewModel;
        }

        /// <summary>
        /// Give a list of schedules organized by weeks, reformat the records related to employee 'id'
        /// into a list of events that the fullcalendar javascript object understands to display on webpage
        /// </summary>
        /// <param name="scheduleByWeeks">List of all employee schedules</param>
        /// <param name="id">Requested employee schedule</param>
        /// <param name="errorString">Error message to display on webpage if there's an error</param>
        /// <returns></returns>
        public static List<CalendarEvent> BuildCalendarEvents( int id, List<ScheduleByWeeks> scheduleByWeeks, 
            IEnumerable<Week> weekStartDates, IEnumerable<Employee> employees, ref string employeeName, ref string errorString)
        {
            var calendarEvents = new List<CalendarEvent>();
            DateTime parsedDate;

            try
            {
                employeeName = employees.Single(e => e.id == id).name;
            }
            catch (System.InvalidOperationException)
            {
                errorString = String.Format("Could not find employee name with employee id of {0}.", id);
                return calendarEvents;
            }


            foreach (var week in scheduleByWeeks)
            {
                var weekStartDate = weekStartDates.Where(w => w.id == week.week).FirstOrDefault().start_date;
                var employeeSchedule = week.schedules;

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
                                errorString = String.Format("Could not parse JSON date.  Expected date format: 'yyyy/MM/dd'.");
                            }
                        }

                    }
                }
            }

            return calendarEvents;
        }

        /// <summary>
        /// Take the employee and shift information and generate the calendar schedule. 
        /// Create the employee schedule using the EMPLOYEES_PER_SHIFT rule
        /// 
        /// Assumptions:
        /// 1.  Each employee can work any amount of shifts
        /// 2.  It is not stated if an employee has more seniority than another employee so this scheduler will sort
        ///     the employee name in alphabetical order and start scheduling from the top of list 
        /// </summary>
        /// <param name="employees"></param>
        /// <param name="employeePerShiftValue"></param>
        /// <param name="timeOffRequests"></param>
        /// <param name="errorString"></param>
        /// <returns></returns>
        public static List<ScheduleByWeeks> BuildScheduleByWeeks(IEnumerable<Employee> employees,
            int? employeePerShiftValue, IEnumerable<TimeOffRequest> timeOffRequests, ref string errorString)
        {
            var scheduleByWeeks = new List<ScheduleByWeeks>();

            if (employees.Count() == 0)
            {
                errorString = "There are no employees to schedule.";
                return scheduleByWeeks;
            }
                
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

                scheduleByWeeks.Add(new ScheduleByWeeks() { week = weekNo, schedules = schedules } );
            }

            // TODO - Write results out to JSON file so we are not generating results again

            // Check if there are enough employees available to work 
            // shift requirements with employee time-off requests
            int requiredShifts = (int)employeePerShiftValue * 28; // 4 weeks * 7 days
            if (totalNumShifts < requiredShifts)
                errorString = String.Format("NOTE:  There are not enough employees to cover required shift of {0} employees per shift.  " +
                                            "Need to schedule {1} more shifts.", employeePerShiftValue, requiredShifts - totalNumShifts);

            return scheduleByWeeks;
        }

        /// <summary>
        /// Retrive JSON data and perform data validation
        /// </summary>
        /// <param name="employees">List of employee names and ids</param>
        /// <param name="timeOffRequests">List of employee time off requests for weeks 23 through 26</param>
        /// <param name="employeePerShiftValue">Number of employees required per shift</param>
        /// <param name="errorString">Error message to display on webpage if there's an error</param>
        /// <returns></returns>
        public static bool RetrieveData(ref IEnumerable<Employee> employees, ref IEnumerable<TimeOffRequest> timeOffRequests, 
            ref IEnumerable<Week> weekStartDates, ref int? employeePerShiftValue, ref string errorString)
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

            weekStartDates = EmployeeScheduleApi.Get2015CalendarWeeks();

            return true;
        }
    }
}