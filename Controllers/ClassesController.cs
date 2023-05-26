using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementApp.MVC.Data;
using SchoolManagementApp.MVC.Models;


namespace SchoolManagementApp.MVC.Controllers
{
    [Authorize]
    public class ClassesController : Controller
    {
        private readonly SchoolManagementDbContext _context;

        public ClassesController(SchoolManagementDbContext context)
        {
            _context = context;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            var schoolManagementDbContext = _context.Classes.Include(q => q.Course).Include(q => q.Lecturer);
            return View(await schoolManagementDbContext.ToListAsync());
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Classes == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            CreateSelectList();
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LectureId,CourseId,Time")] Class @class)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CreateSelectList();
            return View(@class);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Classes == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }
            CreateSelectList();
            return View(@class);
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LectureId,CourseId,Time")] Class @class)
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
           CreateSelectList();
           return View(@class);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Classes == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Classes == null)
            {
                return Problem("Entity set 'SchoolManagementDbContext.Classes'  is null.");
            }
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                _context.Classes.Remove(@class);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        //added action against the view added in the view section
        //So when we click maanagementenrollment  button in the UI then it's going to call this action.
        public async Task<ActionResult> ManageEnrollments(int id){
            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .Include(q=>q.Enrollments)
                //Now we trying something similar to sql joins , as we need to show enrollments for students hence we need students which include all the above includes so basically we want "student enrollments with enrolled course,lecture all tied together just like a sql join" and hence we use "Theninclude" feature here.
                    .ThenInclude(q=>q.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            var students = await _context.Students.ToListAsync();
            //the above query get all the student into the system "the ToListAshync() is the one that executes the query"

            var model = new ClassEnrollmentViewModel();
            model.Class = @class; // This mean the model is getting the data from the class which is created above.Not to confuse with the keyword class because here its a name not a c# specfic keyword hence we are using the "@" symbol before it.
            //similar to above additon of student to the system we now add the model for the same purpose.

             foreach (var stu in students)
            {
                model.Students.Add(new StudentEnrollmentViewModel
                {
                    Id = stu.Id,
                    FirstName = stu.FirstName,
                    LastName = stu.LastName,
                    IsEnrolled = (@class?.Enrollments?.Any(q => q.StudentId == stu.Id))
                        .GetValueOrDefault()
                });
            }
            
            return View(model);
            //So model here is after we got the data from the database, then we started to massage our own interpretation and representation of the data for our view.
            //Now that was our first action completed.
            //Now the second action would be the one to actually assign the student.
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> EnrollStudent(int classId, int studentId, bool shouldEnroll)
        {
            var enrollment = new Enrollment();
            if(shouldEnroll == true)
            {
                enrollment.ClassId = classId;
                enrollment.StudentId = studentId;
                await _context.AddAsync(enrollment);
            }
            else 
            {
                enrollment = await _context.Enrollments.FirstOrDefaultAsync(
                    q => q.ClassId ==classId && q.StudentId == studentId);
                if (enrollment != null){
                    _context.Remove(enrollment);
                }                
            }
            await _context.SaveChangesAsync(); //Important to save all the changes to database as well
            return RedirectToAction(nameof(ManageEnrollments),
            new {id = classId});
        }
        
        private bool ClassExists(int id)
        {
          return (_context.Classes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private void CreateSelectList()
        {
            var courses = _context.Courses.Select(q => new 
            {
                Fullcourse = $"{q.Code} - {q.FirstName}({q.Code} credit)",
                q.Id
            });
            ViewData["CourseId"] = new SelectList(courses, "Id", "Fullcourse");
            var lecturers =  _context.Lecturers.Select(q => new 
            {
                Fullname = $"{q.FirstName}  {q.LastName}",
                q.Id
            }         
            );
            ViewData["LectureId"] = new SelectList(lecturers, "Id", "Fullname");
        }
    }
}
