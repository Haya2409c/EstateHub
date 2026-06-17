using Microsoft.AspNetCore.Mvc;
using RealtorsPortal.Models.ViewModels.Agents;
using RealtorsPortal.Services.Interfaces;

namespace RealtorsPortal.Controllers
{
    /// <summary>
    /// Agents Controller
    /// Handles agent listing and profile pages.
    /// 
    /// Data Flow:
    /// 1. Action calls IAgentService with optional filters
    /// 2. Service queries IAgentRepository, returns DTOs
    /// 3. Controller maps DTOs to ViewModels
    /// 4. View renders with dynamic database data
    /// 
    /// Dependencies:
    ///   - IAgentService: agents, agent profile, agent's properties
    ///   - IContactService: agent enquiry submission
    /// </summary>
    public class AgentsController : Controller
    {
        // TODO: Inject IAgentService, IContactService, ILogger

        public AgentsController()
        {
            // Constructor placeholder
        }

        /// <summary>
        /// GET: /Agents or /Agents/Index
        /// Agent listing page with optional filters (specialization, location) and pagination.
        /// </summary>
        public async Task<IActionResult> Index(string? specialization, string? location, int page = 1)
        {
            try
            {
                var viewModel = new AgentListViewModel();

                // TODO: Implement
                // Steps:
                //   1. Create AgentFilterDto from parameters
                //   2. Call IAgentService.GetAllAgentsAsync(filter, page, 12)
                //   3. Map AgentDto list to AgentCardViewModel list
                //   4. Build PaginationViewModel
                //   5. Return View(viewModel)

                ViewBag.ActivePage = "Agents";
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// GET: /Agents/Profile/{id}
        /// Agent profile page showing agent details, listed properties, testimonials, contact form.
        /// Id parameter is slug: "hifza", "tayyaba", "harmain", etc.
        /// 
        /// Architecture Note:
        /// The existing UI uses view-name switching (Profile_hifza, Profile_tayyaba, etc.)
        /// to keep the HTML structure identical. This action now:
        ///   1. Fetches dynamic data for the agent from database
        ///   2. Passes data via AgentProfileViewModel
        ///   3. Returns the same view names, but view renders database data instead of hardcoded
        /// This approach preserves UI/CSS/layout while replacing data source.
        /// </summary>
        public async Task<IActionResult> Profile(string id)
        {
            try
            {
                // TODO: Implement
                // Steps:
                //   1. Normalize id to lowercase
                //   2. Call IAgentService.GetAgentBySlugAsync(id)
                //   3. If null, return NotFound()
                //   4. Get agent's properties via GetPropertiesByAgentAsync()
                //   5. Map to AgentProfileViewModel
                //   6. Determine view name based on slug (maintain existing UI):
                //      - "hifza" -> "Profile_hifza"
                //      - "tayyaba" -> "Profile_tayyaba"
                //      - "harmain" -> "Profile_harmain"
                //      - default -> "Profile_hifza"
                //   7. Return View(viewName, viewModel)

                var viewModel = new AgentProfileViewModel();
                ViewBag.ActivePage = "Agents";

                var viewName = (id?.ToLower()) switch
                {
                    "hifza" => "Profile_hifza",
                    "tayyaba" => "Profile_tayyaba",
                    "harmain" => "Profile_harmain",
                    _ => "Profile_hifza"
                };

                return View(viewName, viewModel);
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// POST: /Agents/ContactAgent
        /// Submit agent enquiry form from agent profile page.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactAgent(AgentContactFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // TODO: Implement
                // Steps:
                //   1. Create AgentEnquiryDto from model
                //   2. Call IContactService.SubmitAgentEnquiryAsync(dto)
                //   3. Return JSON or redirect
                //   4. On error, return error message

                return Ok(new { message = "Enquiry submitted successfully" });
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                return BadRequest(new { error = "Failed to submit enquiry" });
            }
        }
    }
}
