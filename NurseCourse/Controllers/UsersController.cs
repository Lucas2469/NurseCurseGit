using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NurseCourse.Models;
using NurseCourse.ViewModels;

namespace NurseCourse.Controllers
{
    public class UsersController : Controller
    {
        private readonly ModularCourseDbContext _context;

        public UsersController(ModularCourseDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        public IActionResult RegisterUser(RegistrationViewModel model)
        {
            var userEmail = User.Identity.Name;

            if (ModelState.IsValid)
            {
                var newUser = new User
                {
                    Email = userEmail,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Age = model.Age,
                    Gender = model.Gender,
                    IdNumber = model.IdNumber,
                    CountryOfOrigin = model.CountryOfOrigin,
                    StateOfOrigin = model.StateOfOrigin,
                    CityOfOrigin = model.CityOfOrigin,
                    BirthDate = DateOnly.FromDateTime(Convert.ToDateTime(model.BirthDate)),
                    Occupation = model.Occupation,
                    SpecifyOccupation = model.SpecifyOccupation,
                    HealthProfession = model.HealthProfession,
                    EducationLevel = model.EducationLevel,
                    Institution = model.Institution,
                    Workplace = model.Workplace
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                HttpContext.Session.SetInt32("UserId", newUser.Id);
                HttpContext.Session.SetString("UserEmail", newUser.Email);

                return RedirectToAction("Index", "ExamAttempts");
            }

            ViewBag.ShowRegistrationModal = true;
            return View("Index");
        }

    }
}
