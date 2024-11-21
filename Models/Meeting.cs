using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentEngagementSystem.Models
{
    public class Meeting
    {
        public DateTime Date { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public string Note { get; set; }
    }
}
