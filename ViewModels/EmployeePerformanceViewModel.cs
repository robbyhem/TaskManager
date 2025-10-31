using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class EmployeePerformanceViewModel
    {
        public string FullName { get; set; }

        // Task stats
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }

        // Chart data
        public List<int> ChartData { get; set; }
        public List<string> ChartLabels { get; set; }

        // Weekly progress chart (Bar chart)
        public List<int> WeeklyCompletedTasks { get; set; }
        public List<string> WeeklyLabels { get; set; }
        public List<string> UserNames { get; set; }

        // Performance metrics
        public double CompletionRate { get; set; }

        // Filters
        public string SelectedUserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<Users> UserList { get; set; }
    }
}
