using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using EmployeeSchedulerAssignment.Models;


namespace EmployeeSchedulerAssignment.Services
{
    public class EmployeeScheduleApi
    {
        /// <summary>
        /// Get employee information from JSON url
        /// </summary>
        /// <returns>A list of employee objects</returns>
        public static IEnumerable<Employee> GetEmployees()
        {
            string url = @"http://interviewtest.replicon.com/employees";

            var jsonData = new WebClient().DownloadString(url);
            var obj = JSONHelper.JsonDeserializer<List<Employee>>(jsonData);

            // TODO - add error handling for when there's error retrieving data
            return obj;
        }

        /// <summary>
        /// Get employee time-off requests information from JSON url
        /// </summary>
        /// <returns>A list of employee time off request objects</returns>
        public static IEnumerable<TimeOffRequest> GetEmployeeTimeOffRequests()
        {
            string url = @"http://interviewtest.replicon.com/time-off/requests";

            var jsonData = new WebClient().DownloadString(url);
            var obj = JSONHelper.JsonDeserializer<List<TimeOffRequest>>(jsonData);

            return obj;
        }

        /// <summary>
        /// Get scheduling rule definitions from JSON url
        /// </summary>
        /// <returns>A list of rule definition objects</returns>
        public static IEnumerable<RuleDefinition> GetRuleDefinitions()
        {
            string url = @"http://interviewtest.replicon.com/rule-definitions";

            var jsonData = new WebClient().DownloadString(url);
            var obj = JSONHelper.JsonDeserializer<List<RuleDefinition>>(jsonData);

            return obj;
        }

        /// <summary>
        /// Get scheduling shift rules from JSON url
        /// </summary>
        /// <returns>A list of shift rule objects</returns>
        public static IEnumerable<ShiftRule> GetShiftRules()
        {
            string url = @"http://interviewtest.replicon.com/shift-rules";

            var jsonData = new WebClient().DownloadString(url);
            var obj = JSONHelper.JsonDeserializer<List<ShiftRule>>(jsonData);
            return obj;
        }

        /// <summary>
        /// Get calendar weeks detail for year 2015 from JSON url
        /// </summary>
        /// <returns>A list of shift rule objects</returns>
        public static IEnumerable<Week> Get2015CalendarWeeks()
        {
            string url = @"http://interviewtest.replicon.com/weeks";

            var jsonData = new WebClient().DownloadString(url);
            var obj = JSONHelper.JsonDeserializer<List<Week>>(jsonData);
            return obj;
        }

    }
}