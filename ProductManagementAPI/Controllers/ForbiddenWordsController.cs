using Microsoft.AspNetCore.Mvc;
using ZtpLab1.DataTransferObjects;
using ZtpLab1.Services;

namespace ZtpLab1.Controllers
{
    /// <summary>
    /// Kontroler do zarządzania zabronionymi słowami.
    /// Implementuje REST API zgodnie z wzorcami N-Layer Architecture.
    /// </summary>
    [ApiController]
    [Route("api/v1/forbidden-words")]
    public class ForbiddenWordsController : ControllerBase
    {
        private readonly IForbiddenWordService _forbiddenWordService;

        /// <summary>
        /// Konstruktor kontrolera zabronionych słów.
        /// </summary>
        public ForbiddenWordsController(IForbiddenWordService forbiddenWordService)
        {
            _forbiddenWordService = forbiddenWordService;
        }

        /// <summary>
        /// GET /api/v1/forbidden-words
        /// Pobiera listę wszystkich zabronionych słów.
        /// </summary>
        /// <returns>Lista zabronionych słów</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Models.ForbiddenWord>>>> GetForbiddenWords()
        {
            var result = await _forbiddenWordService.GetAllAsync();
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// GET /api/v1/forbidden-words/active
        /// Pobiera listę aktywnych zabronionych słów.
        /// </summary>
        /// <returns>Lista aktywnych zabronionych słów</returns>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Models.ForbiddenWord>>>> GetActiveForbiddenWords()
        {
            var result = await _forbiddenWordService.GetActiveAsync();
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// GET /api/v1/forbidden-words/{id}
        /// Pobiera szczegóły pojedynczego zakazanego słowa.
        /// </summary>
        /// <param name="id">ID zakazanego słowa</param>
        /// <returns>Szczegóły zakazanego słowa lub 404 Not Found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Models.ForbiddenWord>>> GetForbiddenWord(string id)
        {
            var result = await _forbiddenWordService.GetByIdAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// POST /api/v1/forbidden-words
        /// Dodaje nowe zabronione słowo z walidacją przy użyciu wzorca Specification.
        /// </summary>
        /// <param name="createDto">Dane nowego zakazanego słowa</param>
        /// <returns>201 Created z utworzonym słowem lub odpowiedni kod błędu</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Models.ForbiddenWord>>> CreateForbiddenWord(CreateForbiddenWordDto createDto)
        {
            var result = await _forbiddenWordService.CreateAsync(createDto);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return CreatedAtAction(
                nameof(GetForbiddenWord),
                new { id = result.Data!.Id },
                result
            );
        }

        /// <summary>
        /// PUT /api/v1/forbidden-words/{id}
        /// Aktualizuje istniejące zabronione słowo z walidacją przy użyciu wzorca Specification.
        /// </summary>
        /// <param name="id">ID słowa do aktualizacji</param>
        /// <param name="updateDto">Nowe dane słowa</param>
        /// <returns>200 OK z zaktualizowanym słowem lub odpowiedni kod błędu</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Models.ForbiddenWord>>> UpdateForbiddenWord(string id, UpdateForbiddenWordDto updateDto)
        {
            var result = await _forbiddenWordService.UpdateAsync(id, updateDto);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// DELETE /api/v1/forbidden-words/{id}
        /// Usuwa zabronione słowo z bazy danych.
        /// </summary>
        /// <param name="id">ID słowa do usunięcia</param>
        /// <returns>200 OK z komunikatem sukcesu lub odpowiedni kod błędu</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteForbiddenWord(string id)
        {
            var result = await _forbiddenWordService.DeleteAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// PATCH /api/v1/forbidden-words/{id}/toggle
        /// Przełącza status aktywności zakazanego słowa.
        /// </summary>
        /// <param name="id">ID słowa do przełączenia</param>
        /// <returns>200 OK z nowym statusem lub odpowiedni kod błędu</returns>
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult<ApiResponse<Models.ForbiddenWord>>> ToggleForbiddenWordStatus(string id)
        {
            var result = await _forbiddenWordService.ToggleActiveStatusAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }
    }
}