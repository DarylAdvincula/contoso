
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Models;
using ContosoUniversity.Data;
using ContosoUniversity.Models.SchoolViewModels;
using ContosoUniversity;

public class InstructorsController : Controller
{
    private readonly SchoolContext _context;

    public InstructorsController(SchoolContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        int? id, 
        int? courseId, 
        int? page,
        string? SearchString
    )
    {
        ViewData["SearchString"] = SearchString;

        var viewModel = new InstructorIndexData();

        // store the instructors first
        IQueryable<Instructor> instructorsQuery = _context.Instructors
            .Include(i => i.OfficeAssignment)
            .Include(i => i.CourseAssignments)
                .ThenInclude(ca => ca.Course)
                    .ThenInclude(c => c.Department);

        if (!String.IsNullOrEmpty(SearchString))
        {
            instructorsQuery = instructorsQuery
                .Where(i =>
                    i.LastName.Contains(SearchString) || 
                    i.FirstMidName.Contains(SearchString
                )
            );
        }

        var pageNumber = page ?? 1;
        pageNumber = page < 1 ? 1 : pageNumber;
        int totalItems = await instructorsQuery.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)PageOptions.pageSizeMin);

        ViewBag.Id = id;
        ViewBag.CourseId = courseId;
        ViewBag.PageNumber = pageNumber;
        ViewBag.TotalPages = totalPages;
        ViewBag.HasPreviousPage = pageNumber > 1;
        ViewBag.HasNextPage = pageNumber < totalPages;

        instructorsQuery = instructorsQuery
            .OrderBy(i => i.LastName)
            .Skip((pageNumber - 1) * PageOptions.pageSizeMin)
            .Take(PageOptions.pageSizeMin);

        viewModel.Instructors = await instructorsQuery
            .ToListAsync();

        if (
            id != null &&
            !viewModel.Instructors.Any(i => i.Id == id)
        )
            return NotFound();

        // requires a selected instuctor
        if (id != null)
        {
            // store provided id
            ViewData["InstructorId"] = id;

            // access the instructor with the matching id
            Instructor instructor = viewModel.Instructors
                .Single(i => i.Id == id);

            // get the instructor's taught courses
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
        if (id == null)
            return BadRequest();

        var instructor = await _context.Instructors
            .Include(i => i.CourseAssignments)
                .ThenInclude(ca => ca.Course)
            .Include(i => i.OfficeAssignment)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instructor == null)
            return NotFound();

        return View(instructor);
    }

    public IActionResult Create()
    {
        var newInstructor = new Instructor();
        newInstructor.HireDate = DateTime.Now;

        PopulateAssignedCourseData(newInstructor);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("FirstMidName,LastName,HireDate,OfficeAssignment")]
        Instructor instructor,
        
        string[] selectedCourses
    )
    {
        if (!ModelState.IsValid)
        {
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        if (selectedCourses != null)
        {
            instructor.CourseAssignments = new List<CourseAssignment>();

            foreach (var course in selectedCourses)
            {
                var courseToAdd = new CourseAssignment 
                { 
                    InstructorId = instructor.Id, 
                    CourseId = int.Parse(course) 
                };

                instructor.CourseAssignments.Add(courseToAdd);
            }
        }

        string errorMessage = string.Empty;

        try
        {
            if (String.IsNullOrWhiteSpace(instructor.OfficeAssignment?.Location))
                instructor.OfficeAssignment = null;

            _context.Add(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            errorMessage = "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }
        catch (Exception)
        {
            errorMessage = "An unkown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }

        ModelState.AddModelError("", errorMessage);
        PopulateAssignedCourseData(instructor);
        return View(instructor);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();

        var instructor = await _context.Instructors
            .Include(i => i.OfficeAssignment)
            .Include(i => i.CourseAssignments)
                .ThenInclude(ca => ca.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instructor == null)
            return NotFound();

        PopulateAssignedCourseData(instructor);
        return View(instructor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,

        [Bind("FirstMidName,LastName,HireDate,OfficeAssignment")]
        Instructor instructor,

        string[] selectedCourses
    )
    {
        if (id == null)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        var instructorToUpdate = await _context.Instructors
            .Include(i => i.OfficeAssignment)
            .Include(i => i.CourseAssignments)
                .ThenInclude(i => i.Course)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (instructorToUpdate == null)
            return NotFound();

        string errorMessage = string.Empty;


        try
        {
            instructorToUpdate.FirstMidName = instructor.FirstMidName;
            instructorToUpdate.LastName = instructor.LastName;
            instructorToUpdate.HireDate = instructor.HireDate;
            instructorToUpdate.OfficeAssignment = instructor.OfficeAssignment;

            if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
                instructorToUpdate.OfficeAssignment = null;

            UpdateInstructorCourses(selectedCourses, instructorToUpdate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            errorMessage = "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }
        catch (Exception)
        {
            errorMessage = "An unkown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }

        ModelState.AddModelError("", errorMessage);
        PopulateAssignedCourseData(instructorToUpdate);
        return View(instructorToUpdate);
    }

    private void PopulateAssignedCourseData(Instructor instructor)
    {
        var allCourses = _context.Courses;

        // get all assigned course ids
        var instructorCourses = new HashSet<int>(
            instructor.CourseAssignments
                .Select(ca => ca.CourseId)
        );

        var viewModel = new List<AssignedCourseData>();

        foreach (var course in allCourses)
        {
            viewModel.Add(new AssignedCourseData
            {
                Id = course.Id,
                Title = course.Title,
                Assigned = instructorCourses.Contains(course.Id) // determine if course is assigned
            });
        }

        // store in view
        ViewBag.Courses = viewModel;
    }

    private void UpdateInstructorCourses(
        string[] selectedCourses, 
        Instructor instructorToUpdate
    )
    {
        if (selectedCourses == null)
        {
            // make the instructor's course assignments empty if no courses were selected
            instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
            return;
        }

        // convert selected courses into hash set (prevents duplicate selections)
        var selectedCoursesHS = new HashSet<string>(selectedCourses);

        // get currently assigned course ids
        var instructorCourses = new HashSet<int>(
            instructorToUpdate
                .CourseAssignments.Select(c => c.Course.Id)
        );

        foreach (var course in _context.Courses)
        {
            // determine if the current course's id is in the selected courses ids
            if (selectedCoursesHS.Contains(course.Id.ToString()))
            {
                // make a new course asssignment for every newly selected ids
                if (!instructorCourses.Contains(course.Id))
                {
                    instructorToUpdate.CourseAssignments.Add(new CourseAssignment 
                    { 
                        InstructorId = instructorToUpdate.Id, 
                        CourseId = course.Id 
                    });
                }
            }
            else
            {
                // remove instructor courses that have been unselected
                if (instructorCourses.Contains(course.Id))
                {
                    CourseAssignment courseToRemove = instructorToUpdate.CourseAssignments
                        .First(i => i.CourseId == course.Id);

                    _context.Remove(courseToRemove);
                }
            }
        }
    }

    public async Task<IActionResult> Delete(
        int? id, 
        string? errorMessage
    )
    {
        if (id == null)
            return NotFound();

        var instructor = await _context.Instructors
            .Include(i => i.CourseAssignments)
                .ThenInclude(ca => ca.Course)
            .Include(i => i.OfficeAssignment)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instructor == null)
            return NotFound();

        if (!String.IsNullOrEmpty(errorMessage))
            ViewData["ErrorMessage"] = errorMessage;

        return View(instructor);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
            return NotFound();

        var instructor = await _context.Instructors
            .Include(i => i.CourseAssignments)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (instructor == null)
            return NotFound();

        var departmentWhereAdmin = await _context.Departments
            .FirstOrDefaultAsync(d => d.InstructorId == id);

        if (departmentWhereAdmin != null)
            departmentWhereAdmin.InstructorId = null;

        _context.Instructors.Remove(instructor);

        string errorMessage = string.Empty;

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            errorMessage = "Delete failed. Try again, " +
                "and if the problem persists, " +
                "see your system administrator.";
        }
        catch (Exception)
        {
            errorMessage = "An unknown error occurred. Try again, " +
                "and if the problem persists, " +
                "see your system administrator.";
        }

        return RedirectToAction(
            nameof(Delete),
            new { id, errorMessage }
        );
    }
}
