using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part2_CMCS.Data;
using Part2_CMCS.Models;

namespace Part2_CMCS.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ClaimsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /Claims
        [HttpGet]
        public IActionResult Index()
        {
            // redirect default /Claims to /Claims/MyClaims
            return RedirectToAction(nameof(MyClaims));
        }

        // GET: /Claims/SubmitClaim
        public IActionResult SubmitClaim()
        {
            // initialize required fields for the form
            var claim = new Claim
            {
                LecturerUsername = User.Identity?.Name ?? string.Empty,
                LecturerName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value
                                ?? User.Identity?.Name ?? string.Empty
            };

            return View(claim);
        }

        // POST: /Claims/SubmitClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim model, IFormFile? upload)
        {
            if (!ModelState.IsValid)
                return View(model);

            // attach lecturer information & timestamp
            model.LecturerUsername = User.Identity!.Name!;
            model.LecturerName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value
                                  ?? User.Identity.Name!;
            model.DateSubmitted = DateTime.UtcNow;

            // save claim first to generate ID
            _db.Claims.Add(model);
            await _db.SaveChangesAsync();

            // optional file upload
            if (upload != null && upload.Length > 0)
            {
                var maxBytes = 5 * 1024 * 1024; // 5 MB
                var allowed = new[] { ".pdf", ".doc", ".docx", ".xlsx", ".xls", ".png", ".jpg", ".jpeg" };
                var ext = Path.GetExtension(upload.FileName).ToLowerInvariant();

                if (allowed.Contains(ext) && upload.Length <= maxBytes)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFilename = $"claim_{model.Id}_{Guid.NewGuid()}{ext}";
                    var physicalPath = Path.Combine(uploadsFolder, uniqueFilename);

                    using (var stream = System.IO.File.Create(physicalPath))
                        await upload.CopyToAsync(stream);

                    var doc = new ClaimDocument
                    {
                        ClaimId = model.Id,
                        FileName = upload.FileName,
                        FilePath = $"/uploads/{uniqueFilename}"
                    };

                    _db.ClaimDocuments.Add(doc);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    TempData["UploadError"] = "File type not allowed or file too large (max 5 MB).";
                }
            }

            return RedirectToAction(nameof(MyClaims));
        }

        // GET: /Claims/MyClaims
        public async Task<IActionResult> MyClaims()
        {
            var username = User.Identity!.Name!;
            var myClaims = await _db.Claims
                .Include(c => c.Documents)
                .Where(c => c.LecturerUsername == username)
                .OrderByDescending(c => c.DateSubmitted)
                .ToListAsync();

            return View(myClaims);
        }

        // GET: /Claims/ClaimDetails/5
        public async Task<IActionResult> ClaimDetails(int id)
        {
            var claim = await _db.Claims
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
                return NotFound();

            // only the lecturer who submitted it can view it (PC/Admin views elsewhere)
            if (User.IsInRole("Lecturer") && claim.LecturerUsername != User.Identity!.Name)
                return Forbid();

            return View(claim);
        }
    }
}
