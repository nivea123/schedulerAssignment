using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EmployeeSchedulerAssignment;
using EmployeeSchedulerAssignment.Controllers;
using EmployeeSchedulerAssignment.ViewModels;

namespace UnitTestEmployeeScheduler
{
    [TestClass]
    public class EmployeesControllerTests
    {
        [TestMethod]
        public void Index()
        {

            // Arrange
            EmployeesController controller = new EmployeesController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Calendar()
        {
            // Arrange
            EmployeesController controller = new EmployeesController();

            // Act
            ViewResult result = controller.Calendar(2) as ViewResult;

            // Assert
            CalendarScheduleViewModel calendarViewModel = result.Model as CalendarScheduleViewModel;
            Assert.AreEqual("Allen Pitts", calendarViewModel.employeeName);
            Assert.AreEqual("NOTE:  There are not enough employees to cover required shift of 2 employees per shift.  Need to schedule 11 more shifts.", 
                calendarViewModel.errorString);
            Assert.AreEqual("2015-06-01", calendarViewModel.calendarEvents[0].start);
            Assert.AreEqual("Work", calendarViewModel.calendarEvents[0].title);
            Assert.AreEqual("2015-06-03", calendarViewModel.calendarEvents[1].start);
            Assert.AreEqual("Work", calendarViewModel.calendarEvents[1].title);
            // and so ....
        }
    }
}
