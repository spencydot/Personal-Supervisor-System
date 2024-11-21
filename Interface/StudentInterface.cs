using StudentEngagementSystem.Models;
using System;
using System.Linq;

namespace StudentEngagementSystem.Interfaces
{
  public static class StudentInterface
  {
    public static void HandleStudent(User student, SystemData systemData)
    {
      while (true)
      {
        Console.Clear();
        ShowUnreadMessages(student, systemData);

        Console.WriteLine($"Welcome, {student.Name}!");
        Console.WriteLine("1. Record Today's Feeling");
        Console.WriteLine("2. View My Meetings");
        Console.WriteLine("3. Send Message to Supervisor");
        Console.WriteLine("4. View My Progress");
        Console.WriteLine("5. Book Meeting");
        Console.WriteLine("6. Logout");

        var choice = Console.ReadLine();

        switch (choice)
        {
          case "1":
            RecordFeeling(student, systemData);
            break;
          case "2":
            ViewStudentMeetings(student, systemData);
            break;
          case "3":
            SendMessageToSupervisor(student, systemData);
            break;
          case "4":
            ViewStudentProgress(student, systemData);
            break;
          case "5":
            BookMeeting(student, systemData);
            break;
          case "6":
            if (Utils.Confirm("Are you sure you want to logout?"))
              return;
            break;
          default:
            Console.WriteLine("Invalid option. Please try again.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            break;
        }
      }
    }

    private static void ShowUnreadMessages(User student, SystemData systemData)
    {
      var unreadMessages = systemData.GetUnreadMessages(student.Id);
      if (unreadMessages.Any())
      {
        Console.WriteLine("\nUnread Messages:");
        foreach (var msg in unreadMessages)
        {
          string sender = msg.SenderId == "SYSTEM" ? "SYSTEM" :
              systemData.Users.First(u => u.Id == msg.SenderId).Name;

          if (msg.Type == CommunicationType.Alert)
          {
            Console.ForegroundColor = ConsoleColor.Red;
          }

          Console.WriteLine($"From: {sender}");
          Console.WriteLine($"Message: {msg.Message}");
          Console.WriteLine("-------------------");

          Console.ResetColor();
          systemData.MarkMessageAsRead(msg.Id);
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
      }
    }

    private static void RecordFeeling(User student, SystemData systemData)
    {
      bool recording = true;
      while (recording)
      {
        Console.Write("How are you feeling today? (1-10): ");
        if (int.TryParse(Console.ReadLine(), out int feeling) && feeling >= 1 && feeling <= 10)
        {
          Console.WriteLine($"\nYou entered: {feeling}/10");
          Console.Write("Would you like to add a comment? (yes/no): ");
          string addComment = Console.ReadLine().ToLower();
          string comment = "";

          if (addComment == "yes" || addComment == "y")
          {
            Console.Write("Enter your comment: ");
            comment = Console.ReadLine();
          }

          Console.Write("Do you need to speak with your supervisor? (yes/no): ");
          bool needsSupport = Console.ReadLine().ToLower().StartsWith("y");

          if (Utils.Confirm("Save this record?"))
          {
            var record = new StudentWellbeing
            {
              StudentId = student.Id,
              Date = DateTime.Now,
              FeelingScore = feeling,
              Comment = comment,
              NeedsSupport = needsSupport
            };

            systemData.AddWellbeingRecord(record);

            if (needsSupport)
            {
              systemData.AddCommunication(new Communication
              {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                SenderId = student.Id,
                ReceiverId = student.SupervisorId,
                Message = $"Student has requested support. Feeling score: {feeling}/10. Comment: {comment}",
                IsRead = false,
                Type = CommunicationType.Alert
              });
            }

            Console.WriteLine("Record saved successfully!");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            recording = false;
          }
        }
        else
        {
          Console.WriteLine("Invalid input. Please enter a number between 1 and 10.");
          Console.WriteLine("Press any key to continue...");
          Console.ReadKey();
        }
      }
    }

    private static void ViewStudentMeetings(User student, SystemData systemData)
    {
      var studentMeetings = systemData.Meetings
          .Where(m => m.StudentId == student.Id)
          .OrderBy(m => m.Date);

      Console.WriteLine("\nYour Meetings:");
      if (!studentMeetings.Any())
      {
        Console.WriteLine("You have no scheduled meetings.");
      }
      else
      {
        foreach (var meeting in studentMeetings)
        {
          Console.WriteLine($"\nDate: {meeting.Date.ToString("dd-MM-yyyy")}");
          Console.WriteLine($"Supervisor: {meeting.SupervisorName}");
          Console.WriteLine($"Note: {meeting.Note}");

          // Add visual indicator for upcoming meetings
          if (meeting.Date.Date == DateTime.Today)
          {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("*** TODAY ***");
            Console.ResetColor();
          }
          else if (meeting.Date.Date > DateTime.Today)
          {
            Console.WriteLine($"(In {(meeting.Date.Date - DateTime.Today).Days} days)");
          }
          Console.WriteLine("-------------------");
        }
      }
      Console.WriteLine("\nPress any key to continue...");
      Console.ReadKey();
    }

    private static void SendMessageToSupervisor(User student, SystemData systemData)
    {
      Console.Clear();
      Console.WriteLine("\nSend Message to Supervisor");

      // Show supervisor name
      var supervisor = systemData.Users.First(u => u.Id == student.SupervisorId);
      Console.WriteLine($"Supervisor: {supervisor.Name}");

      // Show recent message history
      var recentMessages = systemData.Communications
          .Where(c => (c.SenderId == student.Id && c.ReceiverId == student.SupervisorId) ||
                     (c.SenderId == student.SupervisorId && c.ReceiverId == student.Id))
          .OrderByDescending(c => c.Date)
          .Take(5);

      if (recentMessages.Any())
      {
        Console.WriteLine("\nRecent Messages:");
        foreach (var msg in recentMessages)
        {
          string sender = msg.SenderId == student.Id ? "You" : "Supervisor";
          Console.WriteLine($"{sender} ({msg.Date.ToString("dd-MM-yyyy HH:mm")}):");
          Console.WriteLine(msg.Message);
          Console.WriteLine("-------------------");
        }
      }

      Console.Write("\nEnter your message (or type 'cancel' to go back): ");
      string message = Console.ReadLine();

      if (!string.IsNullOrWhiteSpace(message) && message.ToLower() != "cancel")
      {
        Console.WriteLine("\nYour message:");
        Console.WriteLine(message);

        if (Utils.Confirm("Send this message?"))
        {
          systemData.AddCommunication(new Communication
          {
            Id = Guid.NewGuid().ToString(),
            Date = DateTime.Now,
            SenderId = student.Id,
            ReceiverId = student.SupervisorId,
            Message = message,
            IsRead = false,
            Type = CommunicationType.Message
          });

          Console.WriteLine("Message sent successfully!");
        }
      }

      Console.WriteLine("Press any key to continue...");
      Console.ReadKey();
    }

    private static void ViewStudentProgress(User student, SystemData systemData)
    {
      var stats = systemData.StudentStats[student.Id];
      var recentWellbeing = systemData.WellbeingRecords
          .Where(w => w.StudentId == student.Id)
          .OrderByDescending(w => w.Date)
          .Take(5);

      Console.WriteLine("\nYour Progress Report");
      Console.WriteLine($"Average Wellbeing Score: {stats.AverageFeelingScore:F1}/10");
      Console.WriteLine($"Meetings Attended: {stats.MeetingAttendanceCount}");
      Console.WriteLine($"Last Engagement: {stats.LastEngagement.ToString("dd-MM-yyyy")}");

      Console.WriteLine("\nRecent Wellbeing Records:");
      foreach (var record in recentWellbeing)
      {
        Console.WriteLine($"Date: {record.Date.ToString("dd-MM-yyyy")}");
        Console.WriteLine($"Score: {record.FeelingScore}/10");
        if (!string.IsNullOrEmpty(record.Comment))
          Console.WriteLine($"Comment: {record.Comment}");
        Console.WriteLine("-------------------");
      }

      Console.WriteLine("\nPress any key to continue...");
      Console.ReadKey();
    }

    private static void BookMeeting(User student, SystemData systemData)
    {
      bool booking = true;
      while (booking)
      {
        Console.Clear();
        Console.Write("\nEnter meeting date (dd-MM-yyyy): ");
        if (DateTime.TryParseExact(Console.ReadLine(), "dd-MM-yyyy", null,
            System.Globalization.DateTimeStyles.None, out DateTime meetingDate))
        {
          if (meetingDate.Date < DateTime.Now.Date)
          {
            Console.WriteLine("Cannot book meetings in the past!");
            Console.WriteLine("Press any key to try again...");
            Console.ReadKey();
            continue;
          }

          Console.WriteLine($"\nSelected date: {meetingDate.ToString("dd-MM-yyyy")}");
          if (Utils.Confirm("Is this correct?"))
          {
            Console.Write("\nEnter reason for meeting: ");
            string note = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(note))
            {
              Console.WriteLine("Note cannot be empty!");
              Console.WriteLine("Press any key to try again...");
              Console.ReadKey();
              continue;
            }

            var meeting = new Meeting
            {
              Date = meetingDate,
              StudentId = student.Id,
              StudentName = student.Name,
              SupervisorId = student.SupervisorId,
              SupervisorName = systemData.Users.First(u => u.Id == student.SupervisorId).Name,
              Note = note
            };

            // Final confirmation
            Console.Clear();
            Console.WriteLine("Meeting Details:");
            Console.WriteLine($"Date: {meeting.Date.ToString("dd-MM-yyyy")}");
            Console.WriteLine($"Supervisor: {meeting.SupervisorName}");
            Console.WriteLine($"Note: {meeting.Note}");

            if (Utils.Confirm("\nBook this meeting?"))
            {
              systemData.AddMeeting(meeting);

              // Notify supervisor
              var notification = new Communication
              {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                SenderId = student.Id,
                ReceiverId = student.SupervisorId,
                Message = $"Meeting request for {meetingDate.ToString("dd-MM-yyyy")}. Reason: {note}",
                IsRead = false,
                Type = CommunicationType.MeetingRequest
              };
              systemData.AddCommunication(notification);

              Console.WriteLine("\nMeeting booked successfully!");
              Console.WriteLine("Press any key to continue...");
              Console.ReadKey();
              booking = false;
            }
          }
        }
        else
        {
          Console.WriteLine("Invalid date format. Please use dd-MM-yyyy (e.g., 25-12-2024)");
          Console.WriteLine("Press any key to try again...");
          Console.ReadKey();
        }
      }
    }
  }
}