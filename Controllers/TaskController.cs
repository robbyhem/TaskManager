using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Threading.Tasks;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
    public class TaskController(ApplicationDbContext context, UserManager<Users> userManager) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<Users> _userManager = userManager;

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string deadline, string status, string assignedTo, string priority, string sortOrder, string search)
        {
            var query = _context.Taskss.Include(t => t.AssignedToUser).AsQueryable();
            string headerText = "All Tasks";

            // --- FILTER BY DEADLINE ---
            if (!string.IsNullOrEmpty(deadline))
            {
                switch (deadline)
                {
                    case "Due Date":
                        query = query.Where(t => t.Deadline != null &&
                                                 t.Deadline.Date > DateTime.Now);
                        headerText = "Tasks Due Today";
                        break;

                    case "Overdue":
                        query = query.Where(t => t.Deadline != null &&
                                                 t.Deadline.Date < DateTime.Today);
                        headerText = "Overdue Tasks";
                        break;

                    case "No Deadline":
                        query = query.Where(t => t.Deadline == null);
                        headerText = "Tasks Without Deadline";
                        break;
                }
            }

            // --- FILTER BY STATUS ---
            if (!string.IsNullOrEmpty(status) && status != "All")
                query = query.Where(t => t.Status == status);

            // --- FILTER BY ASSIGNED USER ---
            if (!string.IsNullOrEmpty(assignedTo) && assignedTo != "All")
                query = query.Where(t => t.AssignedTo == assignedTo);

            // --- FILTER BY PRIORITY ---
            if (!string.IsNullOrEmpty(priority) && priority != "All")
                query = query.Where(t => t.Priority == priority);

            // --- SEARCH BY TITLE OR DESCRIPTION ---
            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

            // --- SORTING ---
            ViewBag.TitleSort = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewBag.DateSort = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewBag.StatusSort = sortOrder == "status_asc" ? "status_desc" : "status_asc";

            switch (sortOrder)
            {
                case "title_desc":
                    query = query.OrderByDescending(t => t.Title);
                    break;
                case "title_asc":
                    query = query.OrderBy(t => t.Title);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(t => t.Deadline);
                    break;
                case "date_asc":
                    query = query.OrderBy(t => t.Deadline);
                    break;
                case "status_desc":
                    query = query.OrderByDescending(t => t.Status);
                    break;
                case "status_asc":
                    query = query.OrderBy(t => t.Status);
                    break;
                default:
                    query = query.OrderBy(t => t.Deadline);
                    break;
            }

            var tasks = await query.ToListAsync();

            var taskViewModels = tasks.Select((t, index) => new TaskViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Deadline = t.Deadline,
                Priority = t.Priority,
                Status = t.Status,
                AssignedTo = t.AssignedToUser != null ? t.AssignedToUser.FullName : "Unassigned",
            }).ToList();

            // Pass lists for filters
            ViewBag.StatusList = new List<string> { "All", "Pending", "In Progress", "Completed" };
            ViewBag.PriorityList = new List<string> { "All", "Critical", "High", "Medium", "Low" };
            ViewBag.UserList = new SelectList(await _context.Users.ToListAsync(), "Id", "FullName");

            ViewBag.HeaderText = headerText;
            ViewBag.TaskCount = tasks.Count;

            return View(taskViewModels);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Fetch users from database for dropdown list
            //var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
            var users = await _userManager.GetUsersInRoleAsync("User");
            ViewBag.Users = new SelectList(users, "Id", "FullName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tasks tasks )
        {
            if (ModelState.IsValid)
            {
                //var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
                var users = await _userManager.GetUsersInRoleAsync("User");
                ViewBag.Users = new SelectList(users, "Id", "FullName", tasks.AssignedTo);
                return View(tasks);
            }

            tasks.Status = "Pending";
            tasks.CreatedAt = DateTime.Now;

            _context.Taskss.Add(tasks);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task created successfully.";
            return RedirectToAction(nameof(Create));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.Taskss.FindAsync(id);

            var users = await _userManager.GetUsersInRoleAsync("User");
            ViewBag.Users = new SelectList(users, "Id", "FullName");

            //return View(task);


            var model = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                Deadline = task.Deadline,
                AssignedTo = task.AssignedToUser != null ? task.AssignedToUser.FullName : "Unassigned",
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskViewModel modifiedTask)
        {
            if (ModelState.IsValid)
            {
                var users = await _userManager.GetUsersInRoleAsync("User");
                ViewBag.Users = new SelectList(users, "Id", "FullName");

                return View(modifiedTask);
            }

            var task = await _context.Taskss.FirstOrDefaultAsync(t => t.Id == modifiedTask.Id);
            if (task == null)
            {
                return RedirectToAction("Index");
            }

            // Update properties
            task.Title = modifiedTask.Title;
            task.Description = modifiedTask.Description;
            task.AssignedTo = modifiedTask.AssignedTo;
            task.Priority = modifiedTask.Priority;
            task.Deadline = modifiedTask.Deadline;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction(nameof(Edit), new { id = modifiedTask.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Invalid task ID.";
                return RedirectToAction("Index");
            }

            var task = await _context.Taskss
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            var assignedUser = await _userManager.FindByIdAsync(task.AssignedTo);

            var viewModel = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                AssignedToUser = task.AssignedToUser,
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Taskss.FindAsync(id);
            if (task == null)
            {
                TempData["Error"] = "Task not found.";
                return RedirectToAction("Index");
            }

            _context.Taskss.Remove(task);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EmployeeReport(string? userId, DateTime? startDate, DateTime? endDate)
        {
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized();
            }

            var query = _context.Taskss.AsQueryable();

            // Filter by user
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(t => t.AssignedTo == userId);
            }

            // Filter by date range
            if (startDate.HasValue)
                query = query.Where(t => t.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.CreatedAt <= endDate.Value);

            var tasks = await query.ToListAsync();

            // Summary counts
            var total = tasks.Count;
            var completed = tasks.Count(t => t.Status == "Completed");
            var pending = tasks.Count(t => t.Status == "Pending");
            var inProgress = tasks.Count(t => t.Status == "In Progress");
            var overdue = tasks.Count(t => t.Deadline < DateTime.Today && t.Status != "Completed");

            // Performance per user (bar chart)
            var userPerformance = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    FullName = u.FullName,
                    CompletedTasks = _context.Taskss.Count(t => t.AssignedTo == u.Id && t.Status == "Completed")
                })
                .ToListAsync();

            var model = new EmployeePerformanceViewModel
            {
                TotalTasks = total,
                CompletedTasks = completed,
                PendingTasks = pending,
                InProgressTasks = inProgress,
                OverdueTasks = overdue,
                ChartLabels = new List<string> { "Pending", "In Progress", "Completed", "Overdue" },
                ChartData = new List<int> { pending, inProgress, completed, overdue },
                UserNames = userPerformance.Select(x => x.FullName).ToList(),
                WeeklyCompletedTasks = userPerformance.Select(x => x.CompletedTasks).ToList(),
                UserList = await _context.Users
                    .Select(u => new Users { Id = u.Id, FullName = u.FullName })
                    .ToListAsync(),
                SelectedUserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };

            return View(model);
        }

        /*--------------------------------------------------------------------
          --------------------------------------------------------------------*/

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyTasks()
        {
            // Get logged-in user ID
            var userId = _userManager.GetUserId(User);

            // Fetch all tasks assigned to this employee
            var tasks = await _context.Taskss
                .Where(t => t.AssignedTo == userId)
                .OrderByDescending(t => t.Deadline)
                .ToListAsync();

            var taskViewModels = tasks.Select(t => new TaskViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                Deadline = t.Deadline
            }).ToList();

            // If no tasks found
            if (taskViewModels == null || tasks.Count == 0)
            {
                TempData["Info"] = "You currently have no assigned tasks.";
            }

            return View(taskViewModels);
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> UpdateTask(int id)
        {
            var task = await _context.Taskss.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var model = new TaskViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Deadline = task.Deadline
            };

            return View(model);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTask(TaskViewModel updatedTask)
        {
            if (ModelState.IsValid)
            {
                return View(updatedTask);
            }

            var existingTask = await _context.Taskss.FindAsync(updatedTask.Id);
            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Status = updatedTask.Status;
            _context.Taskss.Update(existingTask);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task updated successfully!";

            if (User.IsInRole("Admin"))
            {
                // If Admin updated the task, redirect to the Admin’s task table
                return RedirectToAction("Index", "Task");
            }
            else if (User.IsInRole("User"))
            {
                // If Employee updated their task, redirect to their personal tasks
                return RedirectToAction("MyTasks", "Task");
            }
            else
            {
                // Fallback redirect
                return RedirectToAction("Index", "Home");
            }
        }


        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> EmployeeOverview()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Retrieve user tasks
            var tasks = await _context.Taskss
                .Where(t => t.AssignedTo == userId)
                .ToListAsync();

            if (tasks == null || !tasks.Any())
            {
                return View(new EmployeePerformanceViewModel
                {
                    FullName = User.Identity.Name,
                    ChartLabels = new List<string> { "No Data" },
                    ChartData = new List<int> { 0 },
                    WeeklyLabels = new List<string>(),
                    WeeklyCompletedTasks = new List<int>()
                });
            }

            var total = tasks.Count;
            var completed = tasks.Count(t => t.Status == "Completed");
            var inProgress = tasks.Count(t => t.Status == "In Progress");
            var pending = tasks.Count(t => t.Status == "Pending");
            var overdue = tasks.Count(t => t.Deadline < DateTime.Today && t.Status != "Completed");

            var completionRate = total == 0 ? 0 : Math.Round((double)completed / total * 100, 1);
            //double completionRate = total > 0 ? (double)completed / total * 100 : 0;


            // Calculate weekly completion stats (for the last 5 weeks)
            var today = DateTime.Today;
            var weeklyLabels = new List<string>();
            var weeklyCompletedTasks = new List<int>();

            for (int i = 4; i >= 0; i--)
            {
                var startOfWeek = today.AddDays(-7 * i);
                var endOfWeek = startOfWeek.AddDays(6);

                var weekLabel = $"{startOfWeek:MMM dd}";
                weeklyLabels.Add(weekLabel);

                var completedCount = tasks
                    .Where(t => t.Status == "Completed" && t.Deadline >= startOfWeek && t.Deadline <= endOfWeek)
                    .Count();

                weeklyCompletedTasks.Add(completedCount);
            }

            var model = new EmployeePerformanceViewModel
            {
                FullName = User.Identity.Name,
                TotalTasks = total,
                CompletedTasks = completed,
                InProgressTasks = inProgress,
                PendingTasks = pending,
                OverdueTasks = overdue,
                ChartLabels = new List<string> { "Pending", "In Progress", "Completed", "Overdue" },
                ChartData = new List<int> { pending, inProgress, completed, overdue },
                CompletionRate = completionRate,
                WeeklyLabels = weeklyLabels,
                WeeklyCompletedTasks = weeklyCompletedTasks
            };

            return View(model);
        }
    }
}
