using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using StudentEngagementSystem.Models;

namespace StudentEngagementSystem
{
  public class SystemData
  {
    public List<User> Users { get; private set; }
    public List<Meeting> Meetings { get; private set; }
    public List<StudentWellbeing> WellbeingRecords { get; private set; }
    public List<Communication> Communications { get; private set; }
    public Dictionary<string, StudentStatistics> StudentStats { get; private set; }
    private const string DATA_PATH = "studentdata.json";

    public SystemData()
    {
      LoadData();
      UpdateStatistics();
    }

    private void InitializeDefaultData()
    {
      Users = new List<User>
            {
                new User { Id = "S1", Name = "Student A", Password = "pass1", Role = UserRole.Student, SupervisorId = "PS1" },
                new User { Id = "S2", Name = "Student B", Password = "pass2", Role = UserRole.Student, SupervisorId = "PS1" },
                new User { Id = "PS1", Name = "Supervisor 1", Password = "pass3", Role = UserRole.PersonalSupervisor },
                new User { Id = "ST1", Name = "Senior Tutor", Password = "pass4", Role = UserRole.SeniorTutor }
            };
      Meetings = new List<Meeting>();
      WellbeingRecords = new List<StudentWellbeing>();
      Communications = new List<Communication>();
      StudentStats = new Dictionary<string, StudentStatistics>();
      SaveDataToFile();
    }

    private void LoadData()
    {
      if (File.Exists(DATA_PATH))
      {
        string jsonString = File.ReadAllText(DATA_PATH);
        var saveData = JsonSerializer.Deserialize<SaveData>(jsonString);
        Users = saveData.Users;
        Meetings = saveData.Meetings;
        WellbeingRecords = saveData.WellbeingRecords ?? new List<StudentWellbeing>();
        Communications = saveData.Communications ?? new List<Communication>();
        StudentStats = saveData.StudentStats ?? new Dictionary<string, StudentStatistics>();
      }
      else
      {
        InitializeDefaultData();
      }
    }

    private void SaveDataToFile()
    {
      var saveData = new SaveData
      {
        Users = Users,
        Meetings = Meetings,
        WellbeingRecords = WellbeingRecords,
        Communications = Communications,
        StudentStats = StudentStats
      };

      string jsonString = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(DATA_PATH, jsonString);
    }

    public void UpdateStatistics()
    {
      foreach (var student in Users.Where(u => u.Role == UserRole.Student))
      {
        var stats = CalculateStudentStatistics(student.Id);
        StudentStats[student.Id] = stats;

        // Check if intervention is needed
        if (stats.RequiresAttention)
        {
          CreateAlertForSupervisor(student.Id, student.SupervisorId);
        }
      }
      SaveDataToFile();
    }

    private StudentStatistics CalculateStudentStatistics(string studentId)
    {
      var recentWellbeing = WellbeingRecords
          .Where(w => w.StudentId == studentId)
          .OrderByDescending(w => w.Date)
          .ToList();

      var stats = new StudentStatistics
      {
        StudentId = studentId,
        AverageFeelingScore = recentWellbeing.Any() ?
              recentWellbeing.Take(5).Average(w => w.FeelingScore) : 0,
        MeetingAttendanceCount = Meetings
              .Count(m => m.StudentId == studentId && m.Date < DateTime.Now),
        LastEngagement = recentWellbeing.Any() ?
              recentWellbeing.First().Date : DateTime.MinValue,
        ConsecutiveLowScores = CountConsecutiveLowScores(recentWellbeing),
        RequiresAttention = false
      };

      stats.RequiresAttention =
          stats.AverageFeelingScore < 5 ||
          stats.ConsecutiveLowScores >= 3 ||
          (DateTime.Now - stats.LastEngagement).TotalDays > 14;

      return stats;
    }

    private int CountConsecutiveLowScores(List<StudentWellbeing> records)
    {
      int count = 0;
      foreach (var record in records)
      {
        if (record.FeelingScore < 5)
          count++;
        else
          break;
      }
      return count;
    }

    private void CreateAlertForSupervisor(string studentId, string supervisorId)
    {
      var student = Users.First(u => u.Id == studentId);
      var stats = StudentStats[studentId];

      string reason = "";
      if (stats.AverageFeelingScore < 5)
        reason = "Low average wellbeing score";
      else if (stats.ConsecutiveLowScores >= 3)
        reason = "Multiple consecutive low wellbeing scores";
      else if ((DateTime.Now - stats.LastEngagement).TotalDays > 14)
        reason = "No engagement for over 2 weeks";

      var alert = new Communication
      {
        Id = Guid.NewGuid().ToString(),
        Date = DateTime.Now,
        SenderId = "SYSTEM",
        ReceiverId = supervisorId,
        Message = $"Alert: Student {student.Name} requires attention. Reason: {reason}",
        IsRead = false,
        Type = CommunicationType.Alert
      };

      Communications.Add(alert);
      SaveDataToFile();
    }

    public void AddWellbeingRecord(StudentWellbeing record)
    {
      WellbeingRecords.Add(record);
      UpdateStatistics();
    }

    public void AddCommunication(Communication communication)
    {
      Communications.Add(communication);
      SaveDataToFile();
    }

    public void AddMeeting(Meeting meeting)
    {
      Meetings.Add(meeting);
      SaveDataToFile();
    }

    public List<Communication> GetUnreadMessages(string userId)
    {
      return Communications
          .Where(c => c.ReceiverId == userId && !c.IsRead)
          .OrderByDescending(c => c.Date)
          .ToList();
    }

    public void MarkMessageAsRead(string messageId)
    {
      var message = Communications.First(c => c.Id == messageId);
      message.IsRead = true;
      SaveDataToFile();
    }
  }
}