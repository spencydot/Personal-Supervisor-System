using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentEngagementSystem.Models
{
    public class Communication
    {
      public string Id { get; set; }
      public DateTime Date { get; set; }
      public string SenderId { get; set; }
      public string ReceiverId { get; set; }
      public string Message { get; set; }
      public bool IsRead { get; set; }
      public CommunicationType Type { get; set; }
    }
    public enum CommunicationType
    {
      Message,
      Alert,
      MeetingRequest,
      Reminder
    }
}
