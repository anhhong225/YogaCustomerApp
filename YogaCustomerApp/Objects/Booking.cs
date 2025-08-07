using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogaCustomerApp.Objects
{
    public class Booking
    {
        public String customerEmail { get; set; }
        public String customerName { get; set; }
        public string bookingDate { get; set; }
        public String status { get; set; } = "Booked";
        public String scheduleId { get; set; }
    }
}
