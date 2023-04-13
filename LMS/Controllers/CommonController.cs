using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            //tzhou: done
            var query = from dpt in db.Departments
                        select new
                        {
                            name = dpt.Name,
                            subject = dpt.Subject
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            //tzhou: done
            //This method is called: student page, catalog


            //CONSTRAINT `Courses_ibfk_1` FOREIGN KEY(`Department`) REFERENCES `Departments` (`Subject`)
            var query = from dpt in db.Departments
                        where dpt.Courses.Any()//course is not empty
                        select new
                        {
                            subject = dpt.Subject,
                            dname = dpt.Name,

                            //this is from lecture 17 video 6 Nested Data
                            courses = from c in dpt.Courses
                                      select new
                                      {
                                          number = c.Number,
                                          cname = c.Name
                                      }

                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {

            var query = from course in db.Courses
                        where course.Classes.Any() && course.Department.Equals(subject) && course.Number == number
                        from cls in course.Classes
                        join p in db.Professors on cls.TaughtBy equals p.UId
                        select new
                        {
                            season = cls.Season,
                            year = cls.Year,
                            location = cls.Location,
                            start = cls.StartTime,
                            end = cls.EndTime,
                            fname = p.FName,
                            lname = p.LName
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
           //Test: 
           //passed: text longer than 8192 char

            var query = from course in db.Courses
                        where course.Number == num && course.Department == subject
                        from cls in course.Classes
                        where cls.Season == season && cls.Year == year 
                        from assignCate in cls.AssignmentCategories
                        where assignCate.Name == category
                        from assignment in assignCate.Assignments
                        where assignment.Name == asgname
                        select new { assignment.Contents};

            
            string content = query.First().Contents;
            return Content(content);

        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            string submission = "";

            var query = from course in db.Courses
                        where course.Number == num && course.Department == subject
                        from cls in course.Classes
                        where cls.Season == season && cls.Year == year
                        from cat in cls.AssignmentCategories
                        where cat.Name == category
                        from asn in cat.Assignments
                        where asn.Name == asgname
                        from sub in asn.Submissions
                        where sub.StudentNavigation.UId == uid
                        select new {sub.SubmissionContents };

            if (query.Any())
                submission = query.First().SubmissionContents;
            
            return Content(submission);
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var adminQuery = from admin in db.Administrators
                             where admin.UId == uid
                             select new
                             {
                                 fname = admin.FName,
                                 lname = admin.LName, 
                                 uid = uid
                             };

            if (adminQuery.Any())
                return Json(adminQuery.First());

            var studentQuery = from stu in db.Students
                               where stu.UId == uid
                               select new
                               {
                                   fname = stu.FName,
                                   lname = stu.LName,
                                   uid = uid,
                                   department = stu.MajorNavigation.Name
                               };

            if (studentQuery.Any())
                return Json(studentQuery.First());

            var professorQuery = from pro in db.Professors
                                 where pro.UId == uid
                                 select new
                                 {
                                     fname = pro.FName,
                                     lname = pro.LName,
                                     uid = uid,
                                     department = pro.WorksInNavigation.Name
                                 };

            if (professorQuery.Any())
                return Json(professorQuery.First());


            return Json(new { success = false });

        }

        /*******End code to modify********/
    }
}

