namespace TaskManager.ViewModels
{
    public class HomeViewModel
    {
        // Common properties
        public int OverdueTasks { get; set; }
        public int NoDeadlineTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }

        // Admin-only properties
        public int TotalTasks { get; set; }
        public int TotalUsers { get; set; }
        public int TodayDueTasks { get; set; }

        // Regular user properties
        public int MyTasks { get; set; }

        // The role of the current user
        public string UserRole { get; set; }
    }
}
