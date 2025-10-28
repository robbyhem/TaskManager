using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
    [Authorize]
    public class HomeController(ILogger<HomeController> logger, ApplicationDbContext context) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly ApplicationDbContext _context = context;

        public async Task<IActionResult> IndexAsync()
        {
            // Retrieve the logged-in user's information
            var userRole = User.IsInRole("Admin") ? "admin" : "user";
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;


            //var model = new HomeViewModel
            //{
            //    UserRole = userRole
            //};

            //if (userRole == "admin")
            //{
            //    // ===== ADMIN DASHBOARD =====
            //    model.TodayDueTasks = await _context.Taskss
            //        .CountAsync(t => t.Deadline.Date == DateTime.Today);
            //    model.OverdueTasks = await _context.Taskss.CountAsync(t => t.Deadline < DateTime.Today);
            //    model.NoDeadlineTasks = await _context.Taskss.CountAsync(t => t.Deadline == null);
            //    model.TotalTasks = await _context.Taskss.CountAsync();
            //    model.TotalUsers = await _context.Users.CountAsync();
            //    model.PendingTasks = await _context.Taskss.CountAsync(t => t.Status == "Pending");
            //    model.InProgressTasks = await _context.Taskss.CountAsync(t => t.Status == "In Progress");
            //    model.CompletedTasks = await _context.Taskss.CountAsync(t => t.Status == "Completed");
            //}
            //else
            //{
            //    // ===== USER DASHBOARD =====
            //    //int parsedUserId = int.Parse(userId);

            //    //model.MyTasks = await _context.Taskss
            //    //    .CountAsync(t => t.AssignedTo == parsedUserId);
            //    //model.OverdueTasks = await _context.Taskss
            //    //    .CountAsync(t => t.AssignedTo == parsedUserId && t.Deadline < DateTime.Today);
            //    //model.NoDeadlineTasks = await _context.Taskss
            //    //    .CountAsync(t => t.AssignedTo == parsedUserId && t.Deadline == null);
            //    //model.PendingTasks = await _context.Taskss
            //    //    .CountAsync(t => t.AssignedTo == parsedUserId && t.Status == "Pending");
            //    //model.InProgressTasks = await _context.Taskss
            //    //    .CountAsync(t => t.AssignedTo == parsedUserId && t.Status == "In Progress");
            //    //model.CompletedTasks = await _context.Taskss
            //    //    .CountAsync(t => t.AssignedTo == parsedUserId && t.Status == "Completed");


            //    model.MyTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId);
            //    model.OverdueTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Deadline < DateTime.Today);
            //    model.NoDeadlineTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Deadline == null);
            //    model.PendingTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Status == "Pending");
            //    model.InProgressTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Status == "In Progress");
            //    model.CompletedTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Status == "Completed");
            //}


            var viewModel = new HomeViewModel();

            // Check if the logged-in user is an Admin
            if (User.IsInRole("Admin"))
            {
                viewModel.UserRole = "Admin";
                viewModel.TotalTasks = await _context.Taskss.CountAsync();
                viewModel.TotalUsers = await _context.Users.CountAsync();

                // Tasks Due Today
                viewModel.TodayDueTasks = await _context.Taskss.CountAsync(t => t.Deadline != null && t.Deadline.Date == DateTime.Today);

                // Overdue Tasks
                viewModel.OverdueTasks = await _context.Taskss.CountAsync(t => t.Deadline < DateTime.Today && t.Status != "Completed");

                // Tasks with no deadline
                viewModel.NoDeadlineTasks = await _context.Taskss.CountAsync(t => t.Deadline == null);

                // Task status categories
                viewModel.PendingTasks = await _context.Taskss.CountAsync(t => t.Status == "Pending");
                viewModel.InProgressTasks = await _context.Taskss.CountAsync(t => t.Status == "In Progress");
                viewModel.CompletedTasks = await _context.Taskss.CountAsync(t => t.Status == "Completed");
            }
            else
            {
                // Regular user dashboard
                viewModel.UserRole = "User";
                var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

                viewModel.MyTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId);
                viewModel.OverdueTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Deadline < DateTime.Today);
                viewModel.NoDeadlineTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Deadline == null);
                viewModel.PendingTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Status == "Pending");
                viewModel.InProgressTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Status == "In Progress");
                viewModel.CompletedTasks = await _context.Taskss.CountAsync(t => t.AssignedTo == userId && t.Status == "Completed");
            }

            return View(viewModel);
        }
    }
}
