using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.Models;

namespace TaskManager.ViewModels
{
    public class TaskViewModel
    {
        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage = "Task name is required")]
        [Display(Name = "Task Name")]
        public string Title { get; set; }


        [Display(Name = "Task Description")]
        public string Description { get; set; }


        [Required(ErrorMessage = "You must assign the task to a user")]
        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; } // User ID (FK to User table)
        [ForeignKey("AssignedTo")]
        public Users AssignedToUser { get; set; }


        [Required(ErrorMessage = "Status is required")]
        [DataType(DataType.Date)]
        public string Status { get; set; }


        [Required(ErrorMessage = "Deadline is required")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        [ValidateNever]
        public IEnumerable<SelectListItem>? UserList { get; set; }
    }
}
