using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NurseCourse.Models;

namespace NurseCourse.Controllers
{
    public class CertificatesController : Controller
    {
        private readonly ModularCourseDbContext _context;

        public CertificatesController(ModularCourseDbContext context)
        {
            _context = context;
        }

        public IActionResult GenerateCertificate(int courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var user = _context.Users.Find(userId);
            var course = _context.Courses.Find(courseId);

            if (user == null || course == null)
            {
                return NotFound("Usuario o curso no encontrado.");
            }

            using (MemoryStream stream = new MemoryStream())
            {
                // Configurar el documento en formato horizontal
                Document doc = new Document(PageSize.A4.Rotate(), 36, 36, 72, 72);
                PdfWriter writer = PdfWriter.GetInstance(doc, stream);
                writer.CloseStream = false;
                doc.Open();

                // Fuentes y colores
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 32, new BaseColor(33, 37, 41)); // Título más grande
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, new BaseColor(33, 37, 41));
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 16, BaseColor.BLACK);
                var accentColor = new BaseColor(230, 126, 34); // Color de acento para decoraciones

                // Crear borde decorativo alrededor de la página
                PdfContentByte cb = writer.DirectContent;
                cb.SetColorStroke(accentColor);
                cb.SetLineWidth(4f);
                cb.Rectangle(36, 36, doc.PageSize.Width - 72, doc.PageSize.Height - 72);
                cb.Stroke();

                // Añadir título y contenido del certificado
                doc.Add(new Paragraph("Certificado de Reconocimiento", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                });

                doc.Add(new Paragraph("Otorgado a", bodyFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                });

                doc.Add(new Paragraph(user.FullName, subtitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                });

                doc.Add(new Paragraph("por su rendimiento estelar en el curso de", bodyFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                });

                doc.Add(new Paragraph(course.Name, subtitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                });

                // Firma y fecha en la parte inferior
                var footerTable = new PdfPTable(2);
                footerTable.TotalWidth = doc.PageSize.Width - 144;
                footerTable.LockedWidth = true;
                footerTable.HorizontalAlignment = Element.ALIGN_CENTER;
                footerTable.SpacingBefore = 50;

                PdfPCell signatureCell = new PdfPCell(new Phrase("Firma: ____________________", bodyFont));
                signatureCell.Border = Rectangle.NO_BORDER;
                signatureCell.HorizontalAlignment = Element.ALIGN_LEFT;
                signatureCell.PaddingLeft = 20;

                PdfPCell dateCell = new PdfPCell(new Phrase("Fecha: " + DateTime.Now.ToString("dd-MM-yyyy"), bodyFont));
                dateCell.Border = Rectangle.NO_BORDER;
                dateCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                dateCell.PaddingRight = 20;

                footerTable.AddCell(signatureCell);
                footerTable.AddCell(dateCell);

                doc.Add(footerTable);

                doc.Close();

                byte[] pdf = stream.ToArray();
                return File(pdf, "application/pdf", $"Certificado_{user.FullName}_{course.Name}.pdf");
            }
        }


        // GET: Certificates
        public async Task<IActionResult> Index()
        {
            var modularCourseDbContext = _context.Certificates.Include(c => c.Course).Include(c => c.User);
            return View(await modularCourseDbContext.ToListAsync());
        }

        // GET: Certificates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .Include(c => c.Course)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CertificateId == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        // GET: Certificates/Create
        [HttpPost]
        public IActionResult Create(int userId, int courseId)
        {
            var certificate = new Certificate
            {
                UserId = userId,
                CourseId = courseId,
                IssueDate = DateTime.Now
            };

            _context.Certificates.Add(certificate);
            _context.SaveChanges();

            return Ok(new { message = "Inscripción exitosa" });
        }

        // GET: Certificates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", certificate.CourseId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", certificate.UserId);
            return View(certificate);
        }

        // POST: Certificates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CertificateId,UserId,CourseId,IssueDate,CertificatePath")] Certificate certificate)
        {
            if (id != certificate.CertificateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(certificate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificateExists(certificate.CertificateId))
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Id", certificate.CourseId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", certificate.UserId);
            return View(certificate);
        }

        // GET: Certificates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .Include(c => c.Course)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CertificateId == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        // POST: Certificates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate != null)
            {
                _context.Certificates.Remove(certificate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CertificateExists(int id)
        {
            return _context.Certificates.Any(e => e.CertificateId == id);
        }
    }
}
