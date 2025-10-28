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
        public async Task<IActionResult> Index(string deadline)
        {
            var query = _context.Taskss.Include(t => t.AssignedToUser).AsQueryable();
            string headerText = "All Tasks";

            // --- Filtering based on PHP logic ---
            if (!string.IsNullOrEmpty(deadline))
            {
                switch (deadline)
                {
                    case "Due Date":
                        query = query.Where(t => t.Deadline != null &&
                                                 t.Deadline.Date > DateTime.Now);
                        headerText = "Due Today";
                        break;

                    case "Overdue":
                        query = query.Where(t => t.Deadline != null &&
                                                 t.Deadline.Date < DateTime.Today);
                        headerText = "Overdue";
                        break;

                    case "No Deadline":
                        query = query.Where(t => t.Deadline == null);
                        headerText = "No Deadline";
                        break;
                }
            }

            var tasks = await query.ToListAsync();

            var taskViewModels = tasks.Select((t, index) => new TaskViewModel
            {
                Title = t.Title,
                Description = t.Description,
                Deadline = t.Deadline,
                Status = t.Status,
                AssignedTo = t.AssignedToUser != null ? t.AssignedToUser.FullName : "Unassigned",
            }).ToList();

            ViewBag.HeaderText = headerText;
            ViewBag.TaskCount = tasks.Count;

            return View(taskViewModels);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> CreateAsync()
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
                //_context.Taskss.Add(tasks);
                //await _context.SaveChangesAsync();
                //TempData["Success"] = "Task created successfully!";
                //return RedirectToAction(nameof(Create)); // Redirect to clear form

                //var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
                var users = await _userManager.GetUsersInRoleAsync("User");
                ViewBag.Users = new SelectList(users, "Id", "FullName", tasks.AssignedTo);
                return View(tasks);
            }

            //// If validation fails, reload users
            //ViewBag.Users = await _context.Users.ToListAsync();
            //return View(tasks);

            tasks.Status = "Pending";
            tasks.CreatedAt = DateTime.Now;

            _context.Taskss.Add(tasks);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task created successfully.";
            return RedirectToAction(nameof(Create));
        }



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

            //return RedirectToAction("MyTask", "EmployeeTask");
        }
    }
}
