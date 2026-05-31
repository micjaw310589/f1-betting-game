using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace F1BettingApp.API.Controllers
{
    /// <summary>
    /// Admin controller for managing quest definitions.
    /// Provides CRUD operations and weekly reset functionality.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/admin/quest-definitions")]
    [ApiController]
    public class QuestDefinitionsController : ControllerBase
    {
        private readonly IQuestDefinitionService _questDefinitionService;

        public QuestDefinitionsController(IQuestDefinitionService questDefinitionService)
        {
            _questDefinitionService = questDefinitionService;
        }

        /// <summary>
        /// Lists all quest definitions with optional active filter.
        /// </summary>
        /// <param name="isActive">Optional filter to return only active or inactive quests.</param>
        /// <returns>List of quest definitions.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<QuestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<QuestDto>>> GetAll([FromQuery] bool? isActive = null)
        {
            var quests = await _questDefinitionService.GetAllQuestDefinitionsAsync(isActive);
            return Ok(quests);
        }

        /// <summary>
        /// Gets a paginated, filtered, and searchable list of quest definitions.
        /// </summary>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="isActive">Optional filter for active quests.</param>
        /// <param name="searchTerm">Optional search term to filter by name or quest ID.</param>
        /// <returns>Paginated result of quest definitions.</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<QuestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<QuestDto>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _questDefinitionService.GetPagedQuestDefinitionsAsync(page, pageSize, isActive, searchTerm);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new quest definition.
        /// </summary>
        /// <param name="dto">The quest creation data.</param>
        /// <returns>The created quest DTO.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(QuestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<QuestDto>> Create([FromBody] CreateQuestDto dto)
        {
            try
            {
                var quest = await _questDefinitionService.CreateQuestDefinitionAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { }, quest);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse
                {
                    Error = "QUEST_ID_EXISTS",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an existing quest definition.
        /// </summary>
        /// <param name="id">The quest definition ID.</param>
        /// <param name="dto">The quest update data.</param>
        /// <returns>The updated quest DTO.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(QuestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuestDto>> Update(int id, [FromBody] UpdateQuestDto dto)
        {
            try
            {
                var quest = await _questDefinitionService.UpdateQuestDefinitionAsync(id, dto);
                return Ok(quest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "QUEST_NOT_FOUND",
                    Message = $"Quest with ID {id} not found."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "VALIDATION_ERROR",
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse
                {
                    Error = "QUEST_ID_EXISTS",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a quest definition.
        /// </summary>
        /// <param name="id">The quest definition ID.</param>
        /// <returns>No content on success.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _questDefinitionService.DeleteQuestDefinitionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "QUEST_NOT_FOUND",
                    Message = $"Quest with ID {id} not found."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse
                {
                    Error = "QUEST_HAS_PROGRESS",
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Toggles a quest's active/inactive status.
        /// </summary>
        /// <param name="id">The quest definition ID.</param>
        /// <param name="dto">The active status to set.</param>
        /// <returns>The updated quest DTO.</returns>
        [HttpPatch("{id}/active")]
        [ProducesResponseType(typeof(QuestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<QuestDto>> ToggleActive(int id, [FromBody] ToggleQuestActiveDto dto)
        {
            try
            {
                var quest = await _questDefinitionService.ToggleQuestActiveAsync(id, dto.IsActive);
                return Ok(quest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "QUEST_NOT_FOUND",
                    Message = $"Quest with ID {id} not found."
                });
            }
        }

        /// <summary>
        /// Gets the count of users who have completed a specific quest.
        /// </summary>
        /// <param name="questId">The quest identifier.</param>
        /// <returns>Completion count for the quest.</returns>
        [HttpGet("{questId}/completed-count")]
        [ProducesResponseType(typeof(CompletedCountResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CompletedCountResponseDto>> GetCompletedCount(string questId)
        {
            var count = await _questDefinitionService.GetCompletedCountByQuestIdAsync(questId);
            return Ok(new CompletedCountResponseDto { CompletedCount = count });
        }

        /// <summary>
        /// Forces a reset of all weekly quest progress for the current week.
        /// Useful for testing and debugging.
        /// </summary>
        /// <returns>The number of records reset.</returns>
        [HttpPost("reset-week")]
        [ProducesResponseType(typeof(ResetWeekResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResetWeekResponseDto>> ResetWeek()
        {
            var resetCount = await _questDefinitionService.ResetWeeklyQuestsAsync();
            return Ok(new ResetWeekResponseDto
            {
                ResetCount = resetCount,
                Message = $"Successfully reset {resetCount} weekly quest progress record(s)."
            });
        }
    }

    /// <summary>
    /// Response DTO for the reset week endpoint.
    /// </summary>
    public class ResetWeekResponseDto
    {
        public int ResetCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for the completed count endpoint.
    /// </summary>
    public class CompletedCountResponseDto
    {
        public int CompletedCount { get; set; }
    }
}
