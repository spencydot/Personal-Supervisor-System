using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentEngagementSystem.Models
{
  public class StudentStatistics
  {
    public string StudentId { get; set; }
    public double AverageFeelingScore { get; set; }
    public int MeetingAttendanceCount { get; set; }
    public DateTime LastEngagement { get; set; }
    public int ConsecutiveLowScores { get; set; }
    public bool RequiresAttention { get; set; }
  }
}
