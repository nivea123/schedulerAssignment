using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EmployeeSchedulerAssignment.Models;

namespace EmployeeSchedulerAssignment.ViewModels
{
    public class CalendarScheduleViewModel
    {
        public string employeeName { get; set; }
        public List<CalendarEvent> calendarEvents { get; set; }
        public string errorString { get; set; }
    }
}