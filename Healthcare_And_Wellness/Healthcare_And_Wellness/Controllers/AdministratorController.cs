using Healthcare_And_Wellness.Data;
using Healthcare_And_Wellness.Models;
using Healthcare_And_Wellness.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Healthcare_And_Wellness.Controllers
{
    public class AdministratorController : Controller
    {
        private ManagementContext _managementContext;

        public AdministratorController(ManagementContext managementContext)
        {
            _managementContext = managementContext;
        }

        private void PopulateViewBag()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Role = HttpContext.Session.GetString("Role");
        }

        public IActionResult ListJobs()
        {
            PopulateViewBag();
            List<Job> jobs = _managementContext.jobs.ToList();
            return View(jobs);
        }

        [HttpGet]
        public IActionResult AddJobs(int id)
        {
            PopulateViewBag();
            return View(new Job() { jobID = id });
        }

        [HttpPost]
        public IActionResult AddJobs(Job job)
        {
            if (ModelState.IsValid)
            {
                _managementContext.jobs.Add(job);
                _managementContext.SaveChanges();
                return RedirectToAction("ListJobs", "Administrator");
            }
            return View(job);
        }

        public IActionResult JobPage(int id)
        {
            PopulateViewBag();
            List<Applicant> users = _managementContext.applicants.Include(m => m.Job).Where(m => m.jobID == id).ToList();
            List<Job> jobs = _managementContext.jobs.Where(m => m.jobID == id).ToList();
            var jobViewModel = new JobViewModel() { Job = jobs.FirstOrDefault(), applicants = users };

            return View(jobViewModel);
        }

        public IActionResult SendGmailAccept(int id)
        {
            List<Applicant> users = _managementContext.applicants.Include(m => m.Job).Where(m => m.userID == id).ToList();
            if (!users.Any()) return NotFound();

            var user = users.First();
            SendEmail(user.emailUser, "Congratulations!", "You have been accepted.");
            user.statusUser = "Accepted";
            _managementContext.applicants.Update(user);
            _managementContext.SaveChanges();

            return RedirectToAction("JobPage", "Administrator", new { id = user.jobID });
        }

        public IActionResult SendGmailDeny(int id)
        {
            List<Applicant> users = _managementContext.applicants.Include(m => m.Job).Where(m => m.userID == id).ToList();
            if (!users.Any()) return NotFound();

            var user = users.First();
            SendEmail(user.emailUser, "Job Application Update", "Unfortunately, you have been denied.");
            user.statusUser = "Denied";
            _managementContext.applicants.Update(user);
            _managementContext.SaveChanges();

            return RedirectToAction("JobPage", "Administrator", new { id = user.jobID });
        }

        private bool SendEmail(string toAddress, string subject, string body)
        {
            try
            {
                string fromAddress = "your-email@gmail.com"; 
                string appPassword = "your-app-password"; 

                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential(fromAddress, appPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;

                    var mailMessage = new MailMessage()
                    {
                        From = new MailAddress(fromAddress),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toAddress);
                    smtpClient.Send(mailMessage);
                }

                Console.WriteLine("Email sent successfully to: " + toAddress);
                return true;
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP Error: {ex.StatusCode} - {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
            }

            return false;
        }
    }
}
