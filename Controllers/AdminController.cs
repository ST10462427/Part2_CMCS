using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part2_CMCS.Data;
using Part2_CMCS.Models;

namespace Part2_CMCS.Controllers
{
    [Authorize(Roles = "PC,Manager")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ClaimDetails(int id)
        {
            var claim = await _db.Claims
                                 .Include(c => c.Documents)
                                 .FirstOrDefaultAsync(c => c.Id == id);
            if (claim == null) return NotFound();

            // Admin/PC/Manager should be allowed to view any claim
            return View("ClaimDetails", claim); // re-use the ClaimDetails view
        }

        // View submissions waiting for approval
        public async Task<IActionResult> Index()
        {
            var claims = await _db.Claims
                .Where(c => c.Status == ClaimStatus.Pending)   // ✅ correct filter
                .Include(c => c.Documents)
                .OrderBy(c => c.DateSubmitted)
                .ToListAsync();

            return View(claims);
        }

        // Approve or Reject Claim
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string actionType)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            if (actionType == "approve")
                claim.Status = ClaimStatus.ApprovedByPC;
            else if (actionType == "reject")
                claim.Status = ClaimStatus.RejectedByPC;

            claim.LastStatusChanged = DateTime.UtcNow;
            claim.LastChangedBy = User.Identity?.Name ?? "System"; // ✅ correct user tracking

            _db.Claims.Update(claim);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
