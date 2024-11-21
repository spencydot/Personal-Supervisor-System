using StudentEngagementSystem.Models;
using System;
using System.Linq;

namespace StudentEngagementSystem.Interfaces
{
  public static class SeniorTutorInterface
  {
    public static void HandleSeniorTutor(User seniorTutor, SystemData systemData)
    {
      while (true)
      {
        Console.Clear();
        Console.WriteLine($"Welcome, {seniorTutor.Name}!");
        Console.WriteLine("1. View All Student Feelings");
        Console.WriteLine("2. View All Meetings");
        Console.WriteLine("3. Logout");

        var choice = Console.ReadLine();

        switch (choice)
        {
          case "1":
            ViewAllStudentFeelings(systemData);
            break;

          case "2":
            ViewAllMeetings(systemData);
            break;

          case "3":
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

    private static void ViewAllStudentFeelings(SystemData systemData)
    {
      Console.WriteLine("\nAll Student Feelings and Engagement:");
      var students = systemData.Users.Where(u => u.Role == UserRole.Student).ToList();

      foreach (var student in students)
      {
        var latestFeeling = systemData.WellbeingRecords
            .Where(w => w.StudentId == student.Id)
            .OrderByDescending(w => w.Date)
            .FirstOrDefault();

        var stats = systemData.StudentStats[student.Id];

        Console.WriteLine($"\nStudent: {student.Name}");
        Console.WriteLine($"Current Feeling: {(latestFeeling?.FeelingScore ?? 0)}/10");
        Console.WriteLine($"Average Feeling: {stats.AverageFeelingScore:F1}/10");
        Console.WriteLine($"Last Engagement: {stats.LastEngagement.ToString("dd-MM-yyyy")}");
        Console.WriteLine($"Meetings Attended: {stats.MeetingAttendanceCount}");

        if (stats.RequiresAttention)
        {
          Console.WriteLine("*** REQUIRES ATTENTION ***");
        }
        Console.WriteLine("----------------------------------------");
      }

      Console.WriteLine("\nPress any key to continue...");
      Console.ReadKey();
    }

    private static void ViewAllMeetings(SystemData systemData)
    {
      Console.WriteLine("\nAll Meetings:");
      if (!systemData.Meetings.Any())
      {
        Console.WriteLine("No meetings scheduled in the system.");
      }
      else
      {
        foreach (var meeting in systemData.Meetings.OrderBy(m => m.Date))
        {
          Console.WriteLine($"\nDate: {meeting.Date.ToString("dd-MM-yyyy")}");
          Console.WriteLine($"Student: {meeting.StudentName}");
          Console.WriteLine($"Supervisor: {meeting.SupervisorName}");
          Console.WriteLine($"Note: {meeting.Note}");
          Console.WriteLine("----------------------------------------");
        }
      }
      Console.WriteLine("\nPress any key to continue...");
      Console.ReadKey();
    }
  }
}