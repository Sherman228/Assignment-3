using Healthcare_And_Wellness.Data;
using Healthcare_And_Wellness.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare_And_Wellness.Controllers
{
    public class ApplicantController : Controller
    {
        private ManagementContext _managementContext;

        public ApplicantController(ManagementContext managementContext)
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
        public IActionResult AddUser(int id)
        {
            PopulateViewBag();
            return View(new Applicant() { jobID = id });
        }

        [HttpPost]
        public IActionResult AddUser(Applicant user)
        {
            PopulateViewBag();
            if (ModelState.IsValid)
            {
                _managementContext.applicants.Add(user);
                _managementContext.SaveChanges();

                var job = _managementContext.jobs.Find(user.jobID);
                if (job != null)
                {
                    job.statusJob = "Applied";
                    _managementContext.jobs.Update(job);
                    _managementContext.SaveChanges();
                }

                return RedirectToAction("ListJobs", "Applicant");
            }
            return View(user);
        }
    }
}
