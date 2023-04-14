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
            //Test: 
            //1. create a new dept with different subject abbrev
            //2. create a dpt with same subject abbrev should not allow
            //BUG:
            //1. Exception error when sub. abbrev. is longer than 4 char.(fixed)

            //tzhou: done
            if (IsSubjectExist(subject) || subject.Length>4)
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
            var query = from f in db.Professors
                        join d in db.Departments
                        on f.WorksIn equals d.Subject
                        select new
                        {
                            lname = f.LName,
                            fname = f.FName,
                            uid = f.UId
                        };

            return Json(query.ToArray());
            
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

            uint newId = GetNewCourseID();

            Course course = new Course();
            course.CatalogId = newId;
            course.Department = subject;
            course.Number = (uint)number;
            course.Name = name; 
            db.Courses.Add(course);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Checks if a course exist
        /// </summary>
        /// <param name="number">Course number </param>
        /// <param name="subject">Course's departemnt abbrev.</param>
        /// <returns>True, if it exist; otherwise false</returns>
        private bool IsCourseExist(int number, string subject)
        {
            var query = from course in db.Courses
                        where course.Number == number && course.Department == subject
                        select course;
            return query.Any();
        }

        /// <summary>
        /// Gets a new course ID to use
        /// </summary>
        /// <returns>A new course ID</returns>
        private uint GetNewCourseID()
        {
            var query = from course in db.Courses
                        orderby course.CatalogId descending
                        select course.CatalogId;

            uint highest = query.FirstOrDefault();
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
            //tzhou: done

            //course not exist
            if (!IsCourseExist(number, subject))
                return Json(new { success = false }); 

            //Course exist,get course ID
            uint courseID = GetCourseID(number, subject);
            if(IsClassExist(season, year, courseID))
                return Json(new { success = false });


            uint classID = GetNewClassID();

            Class c = new Class();
            c.ClassId = classID;
            c.Season = season;
            c.Year = (uint)year;
            c.Location = location;
            c.StartTime = TimeOnly.FromDateTime(start);
            c.EndTime = TimeOnly.FromDateTime(end);
            c.Listing = courseID;
            c.TaughtBy = instructor; 

            db.Add(c);
            db.SaveChanges();
        
            return Json(new { success = true });
        }

        /// <summary>
        /// Checks if a class exists
        /// </summary>
        /// <param name="season">The season of the class offering</param>
        /// <param name="year">The year the class is offering</param>
        /// <param name="Listing"></param>
        /// <returns>True, if class exist; otherwise false</returns>
        private bool IsClassExist(string season, int year, uint Listing)
        {
            //  UNIQUE KEY `Season` (`Season`,`Year`,`Listing`)
            var query = from c in db.Classes
                        where c.Season.Equals(season) && c.Year == year && c.Listing == Listing
                        select c;
            return query.Any();

        }

        /// <summary>
        /// Gets the course id of a given course
        /// </summary>
        /// <param name="number">the course number</param>
        /// <param name="subject">the course department</param>
        /// <returns></returns>
        private uint GetCourseID(int number, string subject)
        {
            var query = from course in db.Courses
                        where course.Number == number && course.Department.Equals(subject)
                        select course.CatalogId;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Gets the new ID of class to add
        /// </summary>
        /// <returns>A new id</returns>
        private uint GetNewClassID()
        {
            //thzou: done
            var query = from c in db.Classes
                        orderby c.ClassId descending
                        select c.ClassId; 
            uint highest =  query.FirstOrDefault();
            return ++highest; 
        }

        /*******End code to modify********/

    }

}

