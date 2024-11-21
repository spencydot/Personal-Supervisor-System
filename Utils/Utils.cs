using System;

namespace StudentEngagementSystem
{
    public static class Utils
    {
        public static bool Confirm(string message)
        {
            Console.Write($"{message} (yes/no): ");
            string response = Console.ReadLine().ToLower();
            return response == "yes" || response == "y";
        }
    }
}
