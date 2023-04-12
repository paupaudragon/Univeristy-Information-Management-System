using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            //Andy Tran: Done

            //Departments
            /// "subject" - The subject abbreviation of the class (such as "CS")

            //Courses
            /// "number" - The course number (such as 5530)
            /// "name" - The course name

            //Classes
            /// "season" - The season part of the semester
            /// "year" - The year part of the semester

            //Enrolled
            /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned

            var query = from enrolled in db.Enrolleds
                        join classes in db.Classes on enrolled.Class equals classes.ClassId
                        join courses in db.Courses on classes.Listing equals courses.CatalogId
                        join department in db.Departments on courses.Department equals department.Subject
                        where enrolled.Student == uid
                        select new
                        {
                            subject = department.Subject,
                            number = courses.Number,
                            name = courses.Name,
                            season = classes.Season,
                            year = classes.Year,
                            grade = enrolled.Grade
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            //Andy Tran: Done
            var query = from student in db.Students
                        join enrolled in db.Enrolleds on student.UId equals enrolled.Student
                        join classes in db.Classes on enrolled.Class equals classes.ClassId
                        join courses in db.Courses on classes.Listing equals courses.CatalogId
                        join departments in db.Departments on courses.Department equals departments.Subject
                        join assignmentCategories in db.AssignmentCategories on classes.ClassId equals assignmentCategories.InClass
                        join assignments in db.Assignments on assignmentCategories.CategoryId equals assignments.Category
                        join submissions in db.Submissions.Where(submission => submission.Student == uid)
                             on assignments.AssignmentId equals submissions.Assignment into joinedSubmissions
                        from submission in joinedSubmissions.DefaultIfEmpty()
                        where departments.Subject == subject
                              && courses.Number == num
                              && classes.Season == season
                              && classes.Year == year
                              && enrolled.Student == uid
                        select new
                        {
                            aname = assignments.Name,
                            cname = assignmentCategories.Name,
                            due = assignments.Due,
                            score = submission != null ? submission.Score : (uint?)null
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            //Andy Tran: Done

            // Get the class id
            var classId = (from c in db.Classes
                           where c.ListingNavigation.Department == subject &&
                                 c.ListingNavigation.Number == num &&
                                 c.Season == season &&
                                 c.Year == year
                           select c.ClassId).FirstOrDefault();

            if (classId == 0)
            {
                return Json(new { success = false });
            }

            // Get the student id
            var studentId = (from s in db.Students
                             where s.UId == uid
                             select s.UId).FirstOrDefault();

            // Get the assignment id
            var assignmentId = (from a in db.Assignments
                                join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                                where ac.InClassNavigation.ClassId == classId &&
                                      ac.Name == category &&
                                      a.Name == asgname
                                select a.AssignmentId).FirstOrDefault();

            if (assignmentId == 0)
            {
                return Json(new { success = false });
            }

            // Get the submission
            var submission = (from s in db.Submissions
                              where s.AssignmentNavigation.AssignmentId == assignmentId &&
                                    s.StudentNavigation.UId == studentId
                              select s).FirstOrDefault();

            // If the submission doesn't exist, create a new one
            if (submission == null)
            {
                submission = new Submission
                {
                    Assignment = assignmentId,
                    Student = uid,
                    Score = 0,
                    SubmissionContents = contents,
                    Time = DateTime.Now
                };

                db.Submissions.Add(submission);
            }
            else // If the submission already exists, update it
            {
                submission.SubmissionContents = contents;
                submission.Time = DateTime.Now;
            }

            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            //Andy Tran: Done

            // First, retrieve the CatalogID for the given course using the provided subject and course number
            var course = db.Courses.FirstOrDefault(c => c.DepartmentNavigation.Subject == subject && c.Number == num);
            if (course == null)
            {
                // Course not found, return false to indicate enrollment failed
                return Json(new { success = false });
            }

            // Next, retrieve the ClassID for the given class using the provided season, year, and CatalogID
            var courseClass = db.Classes.FirstOrDefault(c => c.Season == season && c.Year == year && c.Listing == course.CatalogId);
            if (courseClass == null)
            {
                // Class not found, return false to indicate enrollment failed
                return Json(new { success = false });
            }

            // Check if the student is already enrolled in the class
            var existingEnrollment = db.Enrolleds.SingleOrDefault(e => e.Student == uid && e.Class == courseClass.ClassId);
            if (existingEnrollment != null)
            {
                // Student is already enrolled, return true to indicate enrollment succeeded
                return Json(new { success = false });
            }

            // Finally, enroll the student in the class by adding a new Enrolled object to the context
            Enrolled enrollment = new Enrolled();
            enrollment.Student = uid;
            enrollment.Class = courseClass.ClassId;
            enrollment.Grade = "--";

            // Save changes to the database
            db.Enrolleds.Add(enrollment);
            db.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            return Json(null);
        }

        /*******End code to modify********/

    }
}

