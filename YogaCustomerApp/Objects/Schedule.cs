using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogaCustomerApp.Objects
{
    public class Schedule
    {
        public int id { get; set; }
        public string date { get; set; }
        public string teacher { get; set; }
        public string comment { get; set; }
        public int yogaCourseId { get; set; }
    }
}
