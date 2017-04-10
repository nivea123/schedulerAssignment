using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using EmployeeSchedulerAssignment;
using EmployeeSchedulerAssignment.EmployeeScheduler;
using EmployeeSchedulerAssignment.Models;
using EmployeeSchedulerAssignment.ViewModels;

namespace UnitTestEmployeeScheduler
{
    [TestClass]
    public class SchedulerTests
    {
        [TestMethod]
        public void TestBuildScheduleByWeeks_InvalidId()
        {
            // TEST:  Invalid employee number
            int? employeePerShiftValue = 2;
            string errorString = null;
            var scheduleByWeeks = new List<ScheduleByWeeks>();
            IEnumerable<Employee> employees = new List<Employee>();
            IEnumerable<TimeOffRequest> timeOffRequests = new List<TimeOffRequest>();

            // Act
            scheduleByWeeks = Scheduler.BuildScheduleByWeeks(employees, employeePerShiftValue, timeOffRequests, ref errorString);

            // Assert
            Assert.AreEqual("There are no employees to schedule.", errorString);
        }

        [TestMethod]
        public void TestBuildCalendarEvents_InvalidId()
        {
            // Test data - invalid employee id
            int employeeId = 10;
            string employeeName = null;
            string errorString = null;
            var weekStartDates = new List<Week>();
            var scheduleByWeeks = new List<ScheduleByWeeks>();
            var calendarEvents = new List<CalendarEvent>();
            IEnumerable<Employee> employees = new List<Employee>()
            {
                new Employee() {id=2, name="Name1"}
            };

            // Act
            calendarEvents = Scheduler.BuildCalendarEvents(employeeId, scheduleByWeeks, weekStartDates, employees, ref employeeName, ref errorString);

            // Assert
            Assert.AreEqual("Could not find employee name with employee id of 10.", errorString);
        }

        [TestMethod]
        public void TestBuildCalendarEvents_InvalidCalendarDateFormat()
        {
            // Test data - invalid employee id
            int employeeId = 10;
            string employeeName = null;
            string errorString = null;
            var scheduleByWeeks = new List<ScheduleByWeeks>();
            var scheudleDays = new List<int>() { 1, 2 };
            IEnumerable<Employee> employees = new List<Employee>()
            {
                new Employee() {id=10, name="Name1"}
            };
            var employeeSchedule = new List<EmployeeSchedule>()
            {
                new EmployeeSchedule() {employee_id=10, schedule=scheudleDays }
            };
            var weekStartDates = new List<Week>()
            {
                new Week {id = 23, start_date = "2016-06-01" }
            };
            var calendarEvents = new List<CalendarEvent>();
            scheduleByWeeks.Add(new ScheduleByWeeks() { week = 23, schedules = employeeSchedule });

            // Act
            calendarEvents = Scheduler.BuildCalendarEvents(employeeId, scheduleByWeeks, weekStartDates, employees, ref employeeName, ref errorString);

            // Assert
            Assert.AreEqual("Could not parse JSON date.  Expected date format: 'yyyy/MM/dd'.", errorString);
        }

    }
}
