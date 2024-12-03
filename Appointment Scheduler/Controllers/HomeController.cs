using Appointment_Scheduler.Data;
using Appointment_Scheduler.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Appointment_Scheduler.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index(string searchTerm)
        {
            var appointments = _context.Appointment.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                appointments = appointments.Where(a => a.Title.Contains(searchTerm)); // Format the date to string if necessary
            }

            // Passing searchTerm to the view
            ViewData["SearchTerm"] = searchTerm;

            return View(appointments.ToList());
        }



        [HttpGet]
        public IActionResult IndexEdit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointment = _context.Appointment.Find(id);
            if (appointment == null)
            {
                // If the appointment does not exist, return a NotFound result
                return NotFound();
            }
            if (appointment.UserId != userId)
            {
                return Forbid(); // Current user is not the owner
            }
            // Pass the appointment to the Edit view
            return View(appointment);

        }

        [HttpGet]
        public IActionResult IndexDelete(int id)
        {
            var appointment = _context.Appointment.Find(id);

            if (appointment == null)
            {
                // If the appointment does not exist, return a NotFound result
                return NotFound();
            }

            // Pass the appointment to the view to confirm deletion
            return View(appointment);
        }

        [HttpPost]
        public IActionResult IndexDelete(Appointment appointment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dbAppointment = _context.Appointment.FirstOrDefault(a => a.Id == appointment.Id);

            if (dbAppointment == null)
            {
                return NotFound(); // Appointment not found
            }

            if (dbAppointment.UserId != userId)
            {
                return Forbid(); // Current user is not the owner
            }

            _context.Appointment.Remove(dbAppointment);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult IndexEdit(Appointment appointment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dbAppointment = _context.Appointment.FirstOrDefault(a => a.Id == appointment.Id);

            if (dbAppointment == null)
            {
                return NotFound(); // Appointment not found
            }

            if (dbAppointment.UserId != userId)
            {
                return Forbid(); // Current user is not the owner
            }

            // Update the appointment
            dbAppointment.Title = appointment.Title;
            dbAppointment.StartTime = appointment.StartTime;
            dbAppointment.EndTime = appointment.EndTime;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult CreateAppointment()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateAppointment(Appointment appointment)
        {
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                appointment.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                _context.Add(appointment);
                _context.SaveChanges();
                var subject = "Appointment Created Successfully";
                var body = $"<h1>Hi,</h1><p>Your appointment titled <b>{appointment.Title}</b> has been successfully registered for {appointment.StartTime} to {appointment.EndTime}.</p>";
                _emailService.SendEmail(userEmail, subject, body);
                TempData["SuccessMessage"] = "Appointment created successfully!";
                return RedirectToAction("Index");
            }
            return View(appointment);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
