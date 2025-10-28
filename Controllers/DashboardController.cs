using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.ViewModels;

namespace TaskManager.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        
    }
}
