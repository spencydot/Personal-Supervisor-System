using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentEngagementSystem.Models
{
  public class StudentWellbeing
  {
    public string StudentId { get; set; }
    public DateTime Date { get; set; }
    public int FeelingScore { get; set; }
    public string Comment { get; set; }  // Optional comment with feeling
    public bool NeedsSupport { get; set; }  // Student can flag if they need help
  }
}
