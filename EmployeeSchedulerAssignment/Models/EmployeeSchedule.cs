using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployeeSchedulerAssignment.Models
{
    // Class used to store the generated schedule
    public class EmployeeSchedule
    {
        public int employee_id { get; set; }
        public List<int> schedule { get; set; }
    }
}