using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NurseCourse.DTO;
using NurseCourse.Models;

namespace NurseCourse.Controllers
{
    public class ExamsController : Controller
    {
        private readonly ModularCourseDbContext _context;

        public ExamsController(ModularCourseDbContext context)
        {
            _context = context;
        }

        // GET: Exams
        public async Task<IActionResult> Index()
        {
            var modularCourseDbContext = _context.Exams.Include(e => e.Module).Include(e => e.OriginalExam);
            return View(await modularCourseDbContext.ToListAsync());
        }

        // GET: Exams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @module = await _context.Modules
                .Include(x => x.Course)
                .Include(x => x.Exams) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@module == null)
            {
                return NotFound();
            }

            return View(@module);
        }


        // GET: Exams/Create
        public IActionResult Create()
        {
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Id");
            ViewData["OriginalExamId"] = new SelectList(_context.Exams, "Id", "Id");
            return View();
        }

        // POST: Exams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        public IActionResult CreateExam([FromBody] Exam exam)
        {
            try
            {
                if (exam.Questions == null || !exam.Questions.Any())
                {
                    return Json(new { success = false, message = "El examen debe tener al menos una pregunta." });
                }

                _context.Exams.Add(exam);
                _context.SaveChanges();

                return Json(new { success = true, message = "Examen creado exitosamente." });

            }catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> UpdateExam([FromBody] Exam exam)
        {
            try
            {
                var existingExam = await _context.Exams
                    .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(e => e.Id == exam.Id);

                if (existingExam == null)
                {
                    return Json(new { success = false, message = "Examen no encontrado." });
                }

                existingExam.Name = exam.Name;

                var newOrUpdatedQuestions = new List<Question>();

                foreach (var question in exam.Questions)
                {
                    if (string.IsNullOrWhiteSpace(question.QuestionText)) continue;

                    var existingQuestion = existingExam.Questions.FirstOrDefault(q => q.Id == question.Id);

                    if (existingQuestion != null)
                    {
                        // Si la pregunta ya existe, actualizamos sus campos
                        existingQuestion.QuestionText = question.QuestionText;
                        existingQuestion.QuestionType = question.QuestionType;
                        existingQuestion.CorrectAnswer = question.QuestionType == 1 ? question.CorrectAnswer : null;

                        var newOrUpdatedOptions = new List<Option>();

                        if (question.QuestionType == 2) 
                        {
                            foreach (var option in question.Options)
                            {
                                if (string.IsNullOrWhiteSpace(option.OptionText)) continue;

                                var existingOption = existingQuestion.Options.FirstOrDefault(o => o.Id == option.Id);

                                if (existingOption != null)
                                {
                                    // Actualizar opción existente
                                    existingOption.OptionText = option.OptionText;
                                    existingOption.IsCorrect = option.IsCorrect;
                                    newOrUpdatedOptions.Add(existingOption); 
                                }
                                else
                                {
                                    newOrUpdatedOptions.Add(new Option
                                    {
                                        OptionText = option.OptionText,
                                        IsCorrect = option.IsCorrect
                                    });
                                }
                            }
                            existingQuestion.Options = newOrUpdatedOptions;
                        }

                        newOrUpdatedQuestions.Add(existingQuestion);
                    }
                    else
                    {
                        var newQuestion = new Question
                        {
                            QuestionText = question.QuestionText,
                            QuestionType = question.QuestionType,
                            CorrectAnswer = question.QuestionType == 1 ? question.CorrectAnswer : null,
                            Options = question.Options
                                .Where(o => !string.IsNullOrWhiteSpace(o.OptionText)) 
                                .Select(o => new Option
                                {
                                    OptionText = o.OptionText,
                                    IsCorrect = o.IsCorrect
                                }).ToList()
                        };

                        newOrUpdatedQuestions.Add(newQuestion);
                    }
                }

                existingExam.Questions = newOrUpdatedQuestions;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Examen actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }



        [HttpGet]
        public IActionResult GetExamDetails(int id)
        {
            var exam = _context.Exams
                .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefault(e => e.Id == id);

            if (exam == null)
            {
                return Json(new { success = false, message = "Examen no encontrado." });
            }

            var examDTO = new ExamDTO
            {
                Id = exam.Id,
                Name = exam.Name,
                Questions = exam.Questions.Select(q => new QuestionDTO
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    CorrectAnswer = q.CorrectAnswer,
                    Options = q.Options.Select(o => new OptionDTO
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return Json(new { success = true, data = examDTO });
        }


        // GET: Exams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Id", exam.ModuleId);
            ViewData["OriginalExamId"] = new SelectList(_context.Exams, "Id", "Id", exam.OriginalExamId);
            return View(exam);
        }

        // POST: Exams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CreationDate,ModuleId,IsMakeup,OriginalExamId")] Exam exam)
        {
            if (id != exam.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exam);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamExists(exam.Id))
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
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Id", exam.ModuleId);
            ViewData["OriginalExamId"] = new SelectList(_context.Exams, "Id", "Id", exam.OriginalExamId);
            return View(exam);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var exam = await _context.Exams
                .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return NotFound(new { success = false, message = "Examen no encontrado." });
            }

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Examen eliminado exitosamente." });
        }


        // GET: Exams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exam = await _context.Exams
                .Include(e => e.Module)
                .Include(e => e.OriginalExam)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }

        // POST: Exams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExamExists(int id)
        {
            return _context.Exams.Any(e => e.Id == id);
        }
    }
}
