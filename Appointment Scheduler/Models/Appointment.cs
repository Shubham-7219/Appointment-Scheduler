using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_Scheduler.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [Required]// Unique identifier for the appointment
        public string Title { get; set; }
        [Required]// Brief description or title of the appointment
        public DateTime StartTime { get; set; }
        [Required]// Start time of the appointment
        [CompareDates("StartTime", ErrorMessage = "End Time must be greater than Start Time.")]
        public DateTime EndTime { get; set; }     // End time of the appointment
        [ForeignKey("UserId")]
        public string? UserId { get; set; }

        [BindNever]
        [NotMapped]
        public IdentityUser User { get; set; }
    }
}
