using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            //use this to see what's the normal output of webpage when false.
            //return Json(new { success = false });

            //tzhou: done
            if (IsSubjectExist(subject))
                return Json(new { success = false});

            Department department = new Department();
            department.Name = name;
            department.Subject = subject;

            db.Departments.Add(department);
            db.SaveChanges();
            return Json(new { success = true });
        }
        
        /// <summary>
        /// Checks is a subject already exist in the database
        /// </summary>
        /// <param name="subject">The subject code</param>
        /// <returns>True, if the subject already exists; otherwise false</returns>
        private bool IsSubjectExist(string subject)
        {
            var query = from dept in db.Departments
                        select dept.Subject;
            foreach (string s in query)
            {
                if (subject.Equals(s))
                {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            //tzhou: done
            var query = from course in db.Courses
                        where course.Department == subject
                        select new
                        {
                            number = course.Number,
                            name  = course.Name
                        }; 

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            
            return Json(null);
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            //To see what's false looks like in webpage
            //return Json(new { success = false });

            //thzou: done
            if (IsCourseExist(number, subject))
                return Json(new { success = false });

            uint newId = GetNewCourseId();

            Course course = new Course();
            course.CatalogId = newId;
            course.Department = subject;
            course.Number = (uint)number;
            course.Name = name; 
            db.Courses.Add(course);
            db.SaveChanges();

            return Json(new { success = true });
        }

        private bool IsCourseExist(int number, string subject)
        {
            var query = from course in db.Courses
                        where course.Number == number && course.Department == subject
                        select course;
            return query.Any();
        }
        private uint GetNewCourseId()
        {
            var query = from course in db.Courses
                        orderby course.CatalogId descending
                        select course.CatalogId;

            uint highest = query.FirstOrDefault();
            if (highest == 0)
                return 1;

            return ++highest;
        }

        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {            
            return Json(new { success = false});
        }


        /*******End code to modify********/

    }
}

