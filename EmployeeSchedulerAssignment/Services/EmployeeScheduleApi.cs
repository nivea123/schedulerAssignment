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
            // Issue JSON call to return list of employee names
            string url = @"http://interviewtest.replicon.com/employees";

            var jsonData = new WebClient().DownloadString(url);
            var obj = JSONHelper.JsonDeserializer<List<Employee>>(jsonData);

            return obj;
        }

        /// <summary>
        /// Get employee time-off requests information from JSON url
        /// </summary>
        /// <returns>A list of employee time off request objects</returns>
        public static IEnumerable<TimeOffRequest> GetEmployeeTimeOffRequests()
        {
            // Issue JSON call to return list of employee time-off request
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
            // Issue JSON call to return list of employee time-off request
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
            // Issue JSON call to return list of employee time-off request
            string url = @"http://interviewtest.replicon.com/shift-rules";

            var jsonData = new WebClient().DownloadString(url);
            var obj = JSONHelper.JsonDeserializer<List<ShiftRule>>(jsonData);
            return obj;
        }
    }
}