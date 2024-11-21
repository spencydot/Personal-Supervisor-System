using StudentEngagementSystem.Models;
using System;
using System.Linq;

namespace StudentEngagementSystem.Interfaces
{
  public static class SupervisorInterface
  {
    public static void HandleSupervisor(User supervisor, SystemData systemData)
    {
      while (true)
      {
        Console.Clear();
        ShowUnreadMessages(supervisor, systemData);

        Console.WriteLine($"Welcome, {supervisor.Name}!");
        Console.WriteLine("1. View Student Feelings");
        Console.WriteLine("2. Book Meeting");
        Console.WriteLine("3. View All Meetings");
        Console.WriteLine("4. View Student Progress");
        Console.WriteLine("5. Logout");

        var choice = Console.ReadLine();

        switch (choice)
        {
          case "1":
            ViewStudentFeelings(supervisor, systemData);
            break;

          case "2":
            BookMeeting(supervisor, systemData);
            break;

          case "3":
            ViewMeetings(supervisor, systemData);
            break;

          case "4":
            ViewStudentProgress(supervisor, systemData);
            break;

          case "5":
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

    private static void ShowUnreadMessages(User supervisor, SystemData systemData)
    {
      var unreadMessages = systemData.GetUnreadMessages(supervisor.Id);
      if (unreadMessages.Any())
      {
        Console.WriteLine("\nUnread Messages and Alerts:");
        foreach (var msg in unreadMessages)
        {
          if (msg.Type == CommunicationType.Alert)
          {
            Console.ForegroundColor = ConsoleColor.Red;
          }

          string sender = msg.SenderId == "SYSTEM" ? "SYSTEM" :
              systemData.Users.First(u => u.Id == msg.SenderId).Name;

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

    private static void ViewStudentFeelings(User supervisor, SystemData systemData)
    {
      Console.WriteLine("\nStudent Feelings:");
      var supervisorStudents = systemData.Users
          .Where(u => u.Role == UserRole.Student && u.SupervisorId == supervisor.Id)
          .ToList();

      if (!supervisorStudents.Any())
      {
        Console.WriteLine("No students assigned.");
      }
      else
      {
        foreach (var student in supervisorStudents)
        {
          var latestFeeling = systemData.WellbeingRecords
              .Where(w => w.StudentId == student.Id)
              .OrderByDescending(w => w.Date)
              .FirstOrDefault();

          var stats = systemData.StudentStats[student.Id];
          string warningMessage = stats.RequiresAttention ? " *** REQUIRES ATTENTION ***" : "";

          Console.WriteLine($"\nStudent: {student.Name}");
          Console.WriteLine($"Current Feeling: {(latestFeeling?.FeelingScore ?? 0)}/10{warningMessage}");
          if (latestFeeling?.NeedsSupport ?? false)
          {
            Console.WriteLine("Student has requested support!");
          }
          if (!string.IsNullOrEmpty(latestFeeling?.Comment))
          {
            Console.WriteLine($"Comment: {latestFeeling.Comment}");
          }
        }
      }
      Console.WriteLine("\nPress any key to continue...");
      Console.ReadKey();
    }

    private static void ViewStudentProgress(User supervisor, SystemData systemData)
    {
      var supervisorStudents = systemData.Users
          .Where(u => u.Role == UserRole.Student && u.SupervisorId == supervisor.Id)
          .ToList();

      foreach (var student in supervisorStudents)
      {
        var stats = systemData.StudentStats[student.Id];
        var recentWellbeing = systemData.WellbeingRecords
            .Where(w => w.StudentId == student.Id)
            .OrderByDescending(w => w.Date)
            .Take(5);

        Console.WriteLine($"\nProgress Report for {student.Name}");
        Console.WriteLine($"Average Wellbeing Score: {stats.AverageFeelingScore:F1}/10");
        Console.WriteLine($"Meetings Attended: {stats.MeetingAttendanceCount}");
        Console.WriteLine($"Last Engagement: {stats.LastEngagement.ToString("dd-MM-yyyy")}");
        Console.WriteLine($"Consecutive Low Scores: {stats.ConsecutiveLowScores}");

        if (stats.RequiresAttention)
        {
          Console.WriteLine("*** REQUIRES ATTENTION ***");
        }

        Console.WriteLine("\nRecent Wellbeing Records:");
        foreach (var record in recentWellbeing)
        {
          Console.WriteLine($"Date: {record.Date.ToString("dd-MM-yyyy")}");
          Console.WriteLine($"Score: {record.FeelingScore}/10");
          if (!string.IsNullOrEmpty(record.Comment))
            Console.WriteLine($"Comment: {record.Comment}");
          Console.WriteLine("-------------------");
        }
        Console.WriteLine("========================================");
      }

      Console.WriteLine("\nPress any key to continue...");
      Console.ReadKey();
    }

    private static void ViewMeetings(User supervisor, SystemData systemData)
    {
      var supervisorMeetings = systemData.Meetings
          .Where(m => m.SupervisorId == supervisor.Id)
          .OrderBy(m => m.Date);

      Console.WriteLine("\nYour Meetings:");
      if (!supervisorMeetings.Any())
      {
        Console.WriteLine("You have no scheduled meetings.");
      }
      else
      {
        foreach (var meeting in supervisorMeetings)
        {
          Console.WriteLine($"\nDate: {meeting.Date.ToString("dd-MM-yyyy")}");
          Console.WriteLine($"Student: {meeting.StudentName}");
          Console.WriteLine($"Note: {meeting.Note}");
        }
      }
      Console.WriteLine("\nPress any key to continue...");
      Console.ReadKey();
    }

    private static void BookMeeting(User supervisor, SystemData systemData)
    {
      bool booking = true;
      while (booking)
      {
        Console.Clear();
        Console.WriteLine("Available Students:");
        var students = systemData.Users
            .Where(u => u.Role == UserRole.Student && u.SupervisorId == supervisor.Id)
            .ToList();

        for (int i = 0; i < students.Count; i++)
        {
          Console.WriteLine($"{i + 1}. {students[i].Name}");
        }

        Console.Write("\nSelect student number (or 0 to cancel): ");
        if (int.TryParse(Console.ReadLine(), out int studentIndex))
        {
          if (studentIndex == 0)
          {
            return;
          }

          if (studentIndex > 0 && studentIndex <= students.Count)
          {
            var selectedStudent = students[studentIndex - 1];
            Console.WriteLine($"\nSelected student: {selectedStudent.Name}");
            if (Utils.Confirm("Is this correct?"))
            {
              bool dateValid = false;
              DateTime meetingDate = DateTime.Now;

              while (!dateValid)
              {
                Console.Write("\nEnter meeting date (dd-MM-yyyy): ");
                string dateInput = Console.ReadLine();

                if (DateTime.TryParseExact(dateInput, "dd-MM-yyyy", null,
                    System.Globalization.DateTimeStyles.None, out meetingDate))
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
                    dateValid = true;
                  }
                }
                else
                {
                  Console.WriteLine("Invalid date format. Please use dd-MM-yyyy (e.g., 25-12-2024)");
                  Console.WriteLine("Press any key to try again...");
                  Console.ReadKey();
                }
              }

              bool noteValid = false;
              string note = "";

              while (!noteValid)
              {
                Console.Write("\nEnter meeting note: ");
                note = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(note))
                {
                  Console.WriteLine("Note cannot be empty!");
                  Console.WriteLine("Press any key to try again...");
                  Console.ReadKey();
                  continue;
                }

                Console.WriteLine($"\nEntered note: {note}");
                if (Utils.Confirm("Is this correct?"))
                {
                  noteValid = true;
                }
              }

              // Final confirmation before booking
              Console.Clear();
              Console.WriteLine("Meeting Details:");
              Console.WriteLine($"Student: {selectedStudent.Name}");
              Console.WriteLine($"Date: {meetingDate.ToString("dd-MM-yyyy")}");
              Console.WriteLine($"Note: {note}");
              if (Utils.Confirm("\nBook this meeting?"))
              {
                var meeting = new Meeting
                {
                  Date = meetingDate,
                  StudentId = selectedStudent.Id,
                  StudentName = selectedStudent.Name,
                  SupervisorId = supervisor.Id,
                  SupervisorName = supervisor.Name,
                  Note = note
                };

                systemData.AddMeeting(meeting);
                Console.WriteLine("\nMeeting booked successfully!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                booking = false;
              }
              else
              {
                Console.WriteLine("\nBooking cancelled. Starting over...");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
              }
            }
          }
          else
          {
            Console.WriteLine("Invalid student number!");
            Console.WriteLine("Press any key to try again...");
            Console.ReadKey();
          }
        }
        else
        {
          Console.WriteLine("Invalid input!");
          Console.WriteLine("Press any key to try again...");
          Console.ReadKey();
        }
      }
    }
  }
}