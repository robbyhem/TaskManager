using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            var users = _context.Users.ToList();

            var viewModel = users.Select(u => new RegisterViewModel
            {
                FullName = u.FullName,
                Email = u.Email,
            }).ToList();

            return View(viewModel);
        }
    }
}
