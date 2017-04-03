using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployeeSchedulerAssignment.Models
{
    public class ShiftRule
    {
        public int rule_id { get; set; }
        public int employee_id { get; set; }
        public int value { get; set; }
    }
}