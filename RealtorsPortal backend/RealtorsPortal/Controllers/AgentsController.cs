using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealtorsPortal.Data;
using RealtorsPortal.Models.Entities;
using RealtorsPortal.Models.ViewModels.Agents;

namespace RealtorsPortal.Controllers
{
    public class AgentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AgentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Agents
        public async Task<IActionResult> Index(string? search)
        {
            var query = _db.Users
                .Where(u => u.AccountType == "Agent")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) ||
                                        (u.Email != null && u.Email.Contains(search)));

            var agentUsers = await query.OrderBy(u => u.FullName).ToListAsync();

            var agentIds = agentUsers.Select(u => u.Id).ToList();
            var listingCounts = await _db.Properties
                .Where(p => p.SellerId != null && agentIds.Contains(p.SellerId))
                .GroupBy(p => p.SellerId!)
                .Select(g => new { Id = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Id, x => x.Count);

            var agents = agentUsers.Select(u => new AgentCardViewModel
            {
                Id           = u.Id,
                FullName     = u.FullName ?? u.Email ?? "Agent",
                Email        = u.Email,
                Phone        = u.PhoneNumber,
                PhotoUrl     = u.PhotoUrl,
                ListingCount = listingCounts.GetValueOrDefault(u.Id, 0),
                JoinedAt     = u.CreatedAt
            }).ToList();

            ViewBag.ActivePage = "Agents";
            ViewBag.Search     = search;
            return View(new AgentListViewModel { Agents = agents });
        }

        // GET: /Agents/Profile/{id}
        public async Task<IActionResult> Profile(string id)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.AccountType == "Agent");
            if (user is null) return NotFound();

            var properties = await _db.Properties
                .Where(p => p.SellerId == id && p.ApprovalStatus == "Active")
                .Include(p => p.Images)
                .OrderByDescending(p => p.ListedDate)
                .Take(6)
                .ToListAsync();

            var vm = new AgentProfileViewModel
            {
                FullName = user.FullName ?? user.Email ?? "Agent",
                Email    = user.Email,
                Phone    = user.PhoneNumber,
                PhotoUrl = user.PhotoUrl,
            };

            ViewBag.ActivePage      = "Agents";
            ViewBag.AgentProperties = properties;
            ViewBag.AgentId         = id;
            return View("ProfileDynamic", vm);
        }

        // POST: /Agents/ContactAgent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactAgent(string AgentId, string FullName, string Email, string? Phone, string? Message)
        {
            if (string.IsNullOrWhiteSpace(AgentId) || string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email))
                return BadRequest();

            _db.PropertyEnquiries.Add(new PropertyEnquiry
            {
                FullName    = FullName,
                Email       = Email,
                Phone       = Phone,
                Message     = Message ?? string.Empty,
                OwnerUserId = AgentId,
                SubmittedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["EnquirySent"] = "Your message has been sent to the agent.";
            return RedirectToAction(nameof(Profile), new { id = AgentId });
        }
    }
}
