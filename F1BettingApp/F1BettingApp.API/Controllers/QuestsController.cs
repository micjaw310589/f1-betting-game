using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Public controller for accessing quest board data.
    /// Returns all active quests with optional progress for authenticated users.
    /// </summary>
    [Route("api/quests")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly IQuestService _questService;
        private readonly IQuestDefinitionService _questDefinitionService;

        public QuestsController(
            IQuestService questService,
            IQuestDefinitionService questDefinitionService)
        {
            _questService = questService;
            _questDefinitionService = questDefinitionService;
        }

        /// <summary>
        /// Returns all active quest definitions.
        /// If user is authenticated, includes their current progress for each quest.
        /// Ordered by Order field, then by QuestId.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<QuestBoardDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<QuestBoardDto>>> GetQuestBoard()
        {
            // Get the current user ID from the authentication middleware
            var userId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : (int?)null;

            // Get all active quest definitions
            var quests = await _questDefinitionService.GetAllQuestDefinitionsAsync(isActive: true);

            var result = new List<QuestBoardDto>();

            foreach (var quest in quests)
            {
                var dto = await _questService.GetQuestBoardProgressAsync(quest.QuestId, userId);
                if (dto != null)
                {
                    result.Add(dto);
                }
            }

            // Order by Order field, then by QuestId
            result = result.OrderBy(q => q.Order).ThenBy(q => q.QuestId).ToList();

            return Ok(result);
        }
    }
}
