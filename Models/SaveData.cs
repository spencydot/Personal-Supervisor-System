using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentEngagementSystem.Models
{
    public class SaveData
    {
      public List<User> Users { get; set; }
      public List<Meeting> Meetings { get; set; }
      public List<StudentWellbeing> WellbeingRecords { get; set; }
      public List<Communication> Communications { get; set; }
      public Dictionary<string, StudentStatistics> StudentStats { get; set; }
    }
}
