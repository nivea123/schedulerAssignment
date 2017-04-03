using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployeeSchedulerAssignment.Models
{
    public class TimeOffRequest
    {
        public int employee_id { get; set; }
        public int week { get; set; }
        public int[] days { get; set; }
    }
}