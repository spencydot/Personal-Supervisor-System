using StudentEngagementSystem.Models;
using StudentEngagementSystem.Interfaces;
using System;

namespace StudentEngagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var systemData = new SystemData();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════╗");
                Console.WriteLine("║   Student Engagement System        ║");
                Console.WriteLine("╚════════════════════════════════════╝");
                Console.WriteLine("\n1. Student Login");
                Console.WriteLine("2. Personal Supervisor Login");
                Console.WriteLine("3. Senior Tutor Login");
                Console.WriteLine("4. Exit");

                var choice = Console.ReadLine();

                if (choice == "4")
                {
                    if (Utils.Confirm("\nAre you sure you want to exit?"))
                        break;
                    continue;
                }

                if (choice == "1" || choice == "2" || choice == "3")
                {
                    HandleLogin(choice, systemData);
                }
                else
                {
                    Console.WriteLine("Invalid option!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }

            Console.WriteLine("\nThank you for using the Student Engagement System!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void HandleLogin(string choice, SystemData systemData)
        {
            bool loginAttempt = true;
            while (loginAttempt)
            {
                Console.Write("Enter ID: ");
                string id = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(id))
                {
                    Console.WriteLine("ID cannot be empty!");
                    Console.WriteLine("Press any key to try again...");
                    Console.ReadKey();
                    continue;
                }

                Console.Write("Enter Password: ");
                string password = Console.ReadLine();

                var user = systemData.Users.Find(u => u.Id == id && u.Password == password);
                if (user != null)
                {
                    switch (user.Role)
                    {
                        case UserRole.Student:
                            StudentInterface.HandleStudent(user, systemData);
                            break;
                        case UserRole.PersonalSupervisor:
                            SupervisorInterface.HandleSupervisor(user, systemData);
                            break;
                        case UserRole.SeniorTutor:
                            SeniorTutorInterface.HandleSeniorTutor(user, systemData);
                            break;
                    }
                    loginAttempt = false;
                }
                else
                {
                    Console.WriteLine("Invalid credentials!");
                    if (!Utils.Confirm("Would you like to try again?"))
                    {
                        loginAttempt = false;
                    }
                }
            }
        }
    }
}
