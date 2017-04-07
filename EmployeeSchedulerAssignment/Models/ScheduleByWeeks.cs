using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployeeSchedulerAssignment.Models
{
    public class ScheduleByWeeks
    {
        public int week { get; set; }
        public List<EmployeeSchedule> schedules { get; set; }
    }
}