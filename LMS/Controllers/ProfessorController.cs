using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            //Andy Tran: Done
            var course = db.Courses.FirstOrDefault(c => c.Department == subject && c.Number == num);
            var courseListing = course!.CatalogId;
            var currentClass = db.Classes.FirstOrDefault(c => c.Listing == courseListing && c.Season == season && c.Year == year);
            var enrolledStudents = db.Enrolleds.Where(e => e.Class == currentClass!.ClassId);

            var studentData = enrolledStudents.Select(e => new
            {
                fname = e.StudentNavigation.FName,
                lname = e.StudentNavigation.LName,
                uid = e.StudentNavigation.UId,
                dob = e.StudentNavigation.Dob.ToString("yyyy-MM-dd"),
                grade = e.Grade
            });

            return Json(studentData);

        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            //Andy Tran: Done
            var query = from course in db.Courses
                        join cls in db.Classes on course.CatalogId equals cls.Listing
                        join department in db.Departments on course.Department equals department.Subject
                        join assignmentCategory in db.AssignmentCategories on cls.ClassId equals assignmentCategory.InClass
                        join assignment in db.Assignments on assignmentCategory.CategoryId equals assignment.Category
                        where department.Subject == subject
                              && course.Number == num
                              && cls.Season == season
                              && cls.Year == year
                              && (category == null || assignmentCategory.Name == category)
                        select new
                        {
                            aname = assignment.Name,
                            cname = assignmentCategory.Name,
                            due = assignment.Due,
                            submissions = assignment.Submissions.Count()
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            //Andy Tran: Done
            var query = from course in db.Courses
                        join cls in db.Classes on course.CatalogId equals cls.Listing
                        join department in db.Departments on course.Department equals department.Subject
                        join category in db.AssignmentCategories on cls.ClassId equals category.InClass
                        where department.Subject == subject
                              && course.Number == num
                              && cls.Season == season
                              && cls.Year == year
                        select new
                        {
                            name = category.Name,
                            weight = category.Weight
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            //Test: 
            //Paased: unique key (category name + class id) 
            //**BUG**: user input on weight: 1% => 0




            //Andy Tran: Done
            // Find the class with the specified subject, number, season, and year.
            var cls = db.Classes.FirstOrDefault(c => c.ListingNavigation.Department == subject && c.ListingNavigation.Number == num && c.Season == season && c.Year == year);

            if (cls == null)
            {
                // Return failure if the class does not exist.
                return Json(new { success = false });
            }

            // Check if a category with the specified name already exists in the class.
            var existingCategory = db.AssignmentCategories.FirstOrDefault(ac => ac.InClass == cls.ClassId && ac.Name == category);

            if (existingCategory != null)
            {
                // Return failure if a category with the same name already exists.
                return Json(new { success = false });
            }

            // Create a new assignment category and add it to the database.
            var newCategory = new AssignmentCategory
            {
                Name = category,
                Weight = (uint)catweight,
                InClass = cls.ClassId
            };

            db.AssignmentCategories.Add(newCategory);
            db.SaveChanges();

            // Return success if the category was created successfully.
            return Json(new { success = true });

        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            //Test: 
            //**BUG**: same assginment name giving error pop-up instead of normal and small "you can't do this" window

            //Andy Tran: TO-DO still need to update the grade
            // Retrieve the class and category objects for the given parameters
            var query = from course in db.Courses
                        join cls in db.Classes on course.CatalogId equals cls.Listing
                        join department in db.Departments on course.Department equals department.Subject
                        join assignmentCategory in db.AssignmentCategories on cls.ClassId equals assignmentCategory.InClass
                        where department.Subject == subject
                              && course.Number == num
                              && cls.Season == season
                              && cls.Year == year
                              && assignmentCategory.Name == category
                        select new { cls, assignmentCategory };

            var result = query.FirstOrDefault();
            if (result == null)
            {
                return Json(new { success = false });
            }

            // Create a new assignment with the given parameters
            var assignment = new Assignment
            {
                Name = asgname,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
                Category = result.assignmentCategory.CategoryId
            };

            // Add the assignment to the database
            db.Assignments.Add(assignment);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            //Test: 
            //Need discussion: maybe bug total pts 10, but professor gave 20

            //CatalogID = Listing
            //ClassId == Inclass
            //CategoryId = Category
            //AssignmentId = Assignment
            //Student = UId

            //Andy Tran: Done
            var query = from c in db.Courses
                        join d in db.Departments on c.Department equals d.Subject
                        join cl in db.Classes on c.CatalogId equals cl.Listing
                        join ac in db.AssignmentCategories on cl.ClassId equals ac.InClass
                        join a in db.Assignments on ac.CategoryId equals a.Category
                        join s in db.Submissions on a.AssignmentId equals s.Assignment
                        join st in db.Students on s.Student equals st.UId
                        where d.Subject == subject && c.Number == num && cl.Season == season && cl.Year == year
                              && ac.Name == category && a.Name == asgname
                        select new
                        {
                            fname = st.FName,
                            lname = st.LName,
                            uid = st.UId,
                            time = s.Time,
                            score = s.Score
                        };

            return Json(query.ToArray());


        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            //Andy Tran: Done
            var course = db.Courses.FirstOrDefault(c => c.Department == subject && c.Number == num);
            if (course == null)
            {
                return NotFound();
            }

            var courseListing = course.CatalogId;
            var currentClass = db.Classes.FirstOrDefault(c => c.Listing == courseListing && c.Season == season && c.Year == year);
            if (currentClass == null)
            {
                return NotFound();
            }

            var assignment = db.Assignments.FirstOrDefault(a => a.Name == asgname && a.CategoryNavigation.Name == category && a.CategoryNavigation.InClass == currentClass.ClassId);
            if (assignment == null)
            {
                return NotFound();
            }

            var submission = db.Submissions.FirstOrDefault(s => s.Assignment == assignment.AssignmentId && s.Student == uid);
            if (submission == null)
            {
                return NotFound();
            }

            submission.Score = (uint)score;
            db.SaveChanges();



            //Andy Tran: TO-DO still need to update grade 
            // Update the student's grade for the class
            var enrolled = db.Enrolleds.FirstOrDefault(e => e.Class == currentClass.ClassId && e.Student == uid);
            if (enrolled == null)
            {
                return NotFound();
            }

            var assignmentsInClass = db.Assignments.Where(a => a.CategoryNavigation.InClass == currentClass.ClassId).ToList();
            var categoriesInClass = db.AssignmentCategories.Where(ac => ac.InClass == currentClass.ClassId).ToList();

            double totalScore = 0.0;
            double totalMaxPoints = 0.0;
            double totalWeightedScore = 0.0;
            double totalWeight = 0.0;

            foreach (var categoryInClass in categoriesInClass)
            {
                var assignmentsInCategory = assignmentsInClass.Where(a => a.CategoryNavigation.CategoryId == categoryInClass.CategoryId).ToList();

                if (assignmentsInCategory.Count > 0)
                {
                    double categoryScore = 0.0;
                    double categoryMaxPoints = 0.0;

                    foreach (var assignmentInCategory in assignmentsInCategory)
                    {
                        var submissionInAssignment = db.Submissions.FirstOrDefault(s => s.Assignment == assignmentInCategory.AssignmentId && s.Student == uid);

                        if (submissionInAssignment != null)
                        {
                            categoryScore += submissionInAssignment.Score;
                        }

                        categoryMaxPoints += assignmentInCategory.MaxPoints;
                    }

                    var categoryPercentage = categoryMaxPoints == 0 ? 0 : categoryScore / categoryMaxPoints;
                    var scaledTotal = categoryPercentage * categoryInClass.Weight;
                    totalWeightedScore += scaledTotal;
                    totalWeight += categoryInClass.Weight;
                }
            }

            var scalingFactor = totalWeight == 0 ? 0 : 100.0 / totalWeight;
            totalScore = totalWeightedScore * scalingFactor;

            enrolled.Grade = ComputeLetterGrade(totalScore);

            db.SaveChanges();

            return Json(new { success = true });

        }


        private string ComputeLetterGrade(double percentageGrade)
        {
            if (percentageGrade >= 93)
            {
                return "A";
            }
            else if (percentageGrade >= 90)
            {
                return "A-";
            }
            else if (percentageGrade >= 87)
            {
                return "B+";
            }
            else if (percentageGrade >= 83)
            {
                return "B";
            }
            else if (percentageGrade >= 80)
            {
                return "B-";
            }
            else if (percentageGrade >= 77)
            {
                return "C+";
            }
            else if (percentageGrade >= 73)
            {
                return "C";
            }
            else if (percentageGrade >= 70)
            {
                return "C-";
            }
            else if (percentageGrade >= 67)
            {
                return "D+";
            }
            else if (percentageGrade >= 63)
            {
                return "D";
            }
            else if (percentageGrade >= 60)
            {
                return "D-";
            }
            else return "E";
        }

        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            //Andy Tran: Done
            var query = from professor in db.Professors
                        join cls in db.Classes on professor.UId equals cls.TaughtBy
                        join course in db.Courses on cls.Listing equals course.CatalogId
                        join department in db.Departments on course.Department equals department.Subject
                        where professor.UId == uid
                        select new
                        {
                            subject = department.Subject,
                            number = course.Number,
                            name = course.Name,
                            season = cls.Season,
                            year = cls.Year
                        };

            return Json(query.ToArray());
        }



        /*******End code to modify********/
    }
}

