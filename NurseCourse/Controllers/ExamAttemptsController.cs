using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NurseCourse.Models;

namespace NurseCourse.Controllers
{
    public class ExamAttemptsController : Controller
    {
        private readonly ModularCourseDbContext _context;

        public ExamAttemptsController(ModularCourseDbContext context)
        {
            _context = context;
        }

        // GET: ExamAttempts
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                ViewBag.ShowRegistrationModal = true;
            }
            else
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserEmail", user.Email);
            }
            var coursesWithModules = await _context.Courses
                                   .Include(c => c.Modules)
                                        .ThenInclude(c => c.Exams)
                                            .ThenInclude(c => c.ExamAttempts)
                                   .Include(u => u.Certificates)
                                   .ToListAsync();

            return View(coursesWithModules);
        }

        // GET: ExamAttempts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var module = await _context.Modules
                .Include(x => x.Course)
                .Include(x => x.Exams)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Options)
                .Include(x => x.Exams)
                    .ThenInclude(e => e.ExamAttempts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null)
            {
                return NotFound();
            }

            return View(module);
        }

        public IActionResult EvaluateExam(int id)
        {
            var exam = _context.Exams
                .Include(e => e.Questions)
                    .ThenInclude(o => o.Options)
                .FirstOrDefault(e => e.Id == id);

            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }

        [HttpPost]
        public IActionResult SubmitExam(int userId, int examId, Dictionary<int, string> answers)
        {
            var exam = _context.Exams
                .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(e => e.Id == examId);

            if (exam == null)
            {
                return Json(new { success = false, message = "Examen no encontrado." });
            }

            int correctAnswers = 0;
            int totalQuestions = exam.Questions.Count;

            foreach (var question in exam.Questions)
            {
                if (answers.TryGetValue(question.Id, out var userAnswer))
                {
                    if (question.QuestionType == 1) // Pregunta abierta
                    {
                        if (string.Equals(userAnswer.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            correctAnswers++;
                        }
                    }
                    else if (question.QuestionType == 2) 
                    {
                        if (int.TryParse(userAnswer, out int selectedOptionId))
                        {
                            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect == true);
                            if (correctOption != null && correctOption.Id == selectedOptionId)
                            {
                                correctAnswers++;
                            }
                        }
                    }
                }
            }

            double score = (double)correctAnswers / totalQuestions * 100;

            var examAttempt = new ExamAttempt
            {
                UserId = userId,
                ExamId = examId,
                AttemptDate = DateTime.Now,
                Score = score,
                IsMakeup = 1
            };

            _context.ExamAttempts.Add(examAttempt);
            _context.SaveChanges();

            return Json(new { success = true, score = score });
        }


        // GET: ExamAttempts/Create
        public IActionResult Create()
        {
            ViewData["ExamId"] = new SelectList(_context.Exams, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: ExamAttempts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,ExamId,AttemptDate,Score,IsMakeup")] ExamAttempt examAttempt)
        {
            if (ModelState.IsValid)
            {
                _context.Add(examAttempt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ExamId"] = new SelectList(_context.Exams, "Id", "Id", examAttempt.ExamId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", examAttempt.UserId);
            return View(examAttempt);
        }

        // GET: ExamAttempts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examAttempt = await _context.ExamAttempts.FindAsync(id);
            if (examAttempt == null)
            {
                return NotFound();
            }
            ViewData["ExamId"] = new SelectList(_context.Exams, "Id", "Id", examAttempt.ExamId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", examAttempt.UserId);
            return View(examAttempt);
        }

        // POST: ExamAttempts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ExamId,AttemptDate,Score,IsMakeup")] ExamAttempt examAttempt)
        {
            if (id != examAttempt.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(examAttempt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamAttemptExists(examAttempt.Id))
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
            ViewData["ExamId"] = new SelectList(_context.Exams, "Id", "Id", examAttempt.ExamId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", examAttempt.UserId);
            return View(examAttempt);
        }

        // GET: ExamAttempts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examAttempt = await _context.ExamAttempts
                .Include(e => e.Exam)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (examAttempt == null)
            {
                return NotFound();
            }

            return View(examAttempt);
        }

        // POST: ExamAttempts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examAttempt = await _context.ExamAttempts.FindAsync(id);
            if (examAttempt != null)
            {
                _context.ExamAttempts.Remove(examAttempt);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExamAttemptExists(int id)
        {
            return _context.ExamAttempts.Any(e => e.Id == id);
        }
    }
}
