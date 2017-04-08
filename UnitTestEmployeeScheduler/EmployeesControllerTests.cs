using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EmployeeSchedulerAssignment;
using EmployeeSchedulerAssignment.Controllers;

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
            //Assert.AreEqual("Blah", result.ViewBag.Message);
            Assert.IsNotNull(result);
        }
    }
}
