using System.Diagnostics;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class HomeController : Controller
    {
        private readonly SchoolContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            SchoolContext context,
            ILogger<HomeController> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> About()
        {
            //var students = await _context.Students
            //    .GroupBy(s => s.EnrollmentDate)
            //    .Select(dateGroup => new EnrollmentDateGroup()
            //    {
            //        EnrollmentDate = dateGroup.Key,
            //        StudentCount = dateGroup.Count()
            //    })
            //    .AsNoTracking()
            //    .ToListAsync();

            string query = "SELECT EnrollmentDate, COUNT(*) AS StudentCount " +
                "FROM Person " +
                "WHERE Discriminator = 'Student' " +
                "GROUP BY EnrollmentDate";

            IEnumerable<EnrollmentDateGroup> data = await _context.Database
                .SqlQueryRaw<EnrollmentDateGroup>(query)
                .AsNoTracking()
                .ToListAsync();

            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
