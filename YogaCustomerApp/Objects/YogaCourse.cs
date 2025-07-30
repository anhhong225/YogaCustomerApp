using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogaCustomerApp.Objects
{
    public class YogaCourse
    {
        public int id { get; set; }
        public string type { get; set; }
        public string dayofweek { get; set; }
        public string time { get; set; }
        public string description { get; set; }
        public string duration { get; set; }
        public int capacity { get; set; }
        public float price { get; set; }
    }
}
