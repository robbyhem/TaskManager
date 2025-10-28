using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models
{
    public class Tasks
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
        [ForeignKey(nameof(AssignedTo))]
        public Users AssignedToUser { get; set; }


        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
