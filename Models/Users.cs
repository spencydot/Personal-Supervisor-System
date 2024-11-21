using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentEngagementSystem.Models
{
    public enum UserRole
    {
        Student,
        PersonalSupervisor,
        SeniorTutor
    }

    public class User
    {
      public string Id { get; set; }
      public string Name { get; set; }
      public string Password { get; set; }
      public UserRole Role { get; set; }
      public string SupervisorId { get; set; }  // For students: links to their supervisor
    }
}
