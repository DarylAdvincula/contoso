
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Models;
using ContosoUniversity.Data;
using ContosoUniversity.Models.SchoolViewModels;

public class InstructorsController : Controller
{
    private readonly SchoolContext _context;

    public InstructorsController(SchoolContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? id, int? courseId)    
    {
        var viewModel = new InstructorIndexData();

        // store the instructors first
        viewModel.Instructors = await _context.Instructors
            .Include(i => i.OfficeAssignment)
            .Include(i => i.CourseAssignments)
                .ThenInclude(ca => ca.Course)
                    .ThenInclude(c => c.Department)
            .OrderBy(i => i.LastName)
            .ToListAsync();

        // requires a selected instuctor
        if (id != null)
        {
            // store provided id
            ViewData["InstructorId"] = id;

            // access the instructor with the matching id
            Instructor instructor = viewModel.Instructors
                .Single(i => i.Id == id);

            // get the instructor's course
            viewModel.Courses = instructor.CourseAssignments
                .Select(ca => ca.Course);
        }

        // requires a selected course
        // explicit loading - loading data only when needed
        if (courseId != null)
        {
            ViewData["CourseId"] = courseId;

            var selectedCourse = viewModel.Courses
                .Single(c => c.Id == courseId);

            await _context
                .Entry(selectedCourse)
                .Collection(c => c.Enrollments)
                .LoadAsync();

            foreach (var enrollment in selectedCourse.Enrollments)
            {
                await _context
                    .Entry(enrollment)
                    .Reference(x => x.Student)
                    .LoadAsync();
            }

            viewModel.Enrollments = selectedCourse.Enrollments;
        }

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int? id)
    {

        return View();
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,LastName,FirstMidName,HireDate,FullName,CourseAssignments,OfficeAssignment")] Instructor instructor)
    {

        return View(instructor);
    }

    public async Task<IActionResult> Edit(int? id)
    {

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,LastName,FirstMidName,HireDate,FullName,CourseAssignments,OfficeAssignment")] Instructor instructor)
    {

        return View(instructor);
    }

    public async Task<IActionResult> Delete(int? id)
    {

        return View();
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {

        return RedirectToAction(nameof(Index));
    }
}
