using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ZtpLab1.Data;
using ZtpLab1.Models;

namespace ZtpLab1.Controllers
{
    /// <summary>
    /// Kontroler do zarządzania zabronionymi słowami.
    /// Umożliwia administratorom dodawanie, usuwanie i modyfikację listy zabronionych słów.
    /// </summary>
    [ApiController]
    [Route("api/v1/forbidden-words")]
    public class ForbiddenWordsController : ControllerBase
    {
        private readonly IMongoCollection<ForbiddenWord> _forbiddenWords;

        /// <summary>
        /// Konstruktor kontrolera zabronionych słów.
        /// </summary>
        public ForbiddenWordsController(MongoDbContext context)
        {
            _forbiddenWords = context.ForbiddenWords;
        }

        /// <summary>
        /// GET /api/v1/forbidden-words
        /// Pobiera listę wszystkich zabronionych słów.
        /// </summary>
        /// <returns>Lista zabronionych słów</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ForbiddenWord>>> GetForbiddenWords()
        {
            var words = await _forbiddenWords
                .Find(_ => true)
                .SortBy(fw => fw.Word)
                .ToListAsync();
            return Ok(words);
        }

        /// <summary>
        /// GET /api/v1/forbidden-words/active
        /// Pobiera listę aktywnych zabronionych słów.
        /// </summary>
        /// <returns>Lista aktywnych zabronionych słów</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ForbiddenWord>>> GetActiveForbiddenWords()
        {
            var words = await _forbiddenWords
                .Find(fw => fw.IsActive)
                .SortBy(fw => fw.Word)
                .ToListAsync();
            return Ok(words);
        }

        /// <summary>
        /// GET /api/v1/forbidden-words/{id}
        /// Pobiera szczegóły pojedynczego zakazanego słowa.
        /// </summary>
        /// <param name="id">ID zakazanego słowa</param>
        /// <returns>Szczegóły zakazanego słowa lub 404 Not Found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ForbiddenWord>> GetForbiddenWord(string id)
        {
            var word = await _forbiddenWords.Find(fw => fw.Id == id).FirstOrDefaultAsync();

            if (word == null)
            {
                return NotFound($"Zabronione słowo o ID {id} nie zostało znalezione.");
            }

            return Ok(word);
        }

        /// <summary>
        /// POST /api/v1/forbidden-words
        /// Dodaje nowe zabronione słowo.
        /// </summary>
        /// <param name="forbiddenWord">Dane nowego zakazanego słowa</param>
        /// <returns>201 Created z utworzonym słowem lub odpowiedni kod błędu</returns>
        [HttpPost]
        public async Task<ActionResult<ForbiddenWord>> CreateForbiddenWord(ForbiddenWord forbiddenWord)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Normalizacja - konwersja do małych liter dla spójności
                forbiddenWord.Word = forbiddenWord.Word.ToLowerInvariant().Trim();

                await _forbiddenWords.InsertOneAsync(forbiddenWord);

                return CreatedAtAction(
                    nameof(GetForbiddenWord),
                    new { id = forbiddenWord.Id },
                    forbiddenWord
                );
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                return Conflict(new { message = $"Zabronione słowo '{forbiddenWord.Word}' już istnieje." });
            }
        }

        /// <summary>
        /// PUT /api/v1/forbidden-words/{id}
        /// Aktualizuje istniejące zabronione słowo.
        /// </summary>
        /// <param name="id">ID słowa do aktualizacji</param>
        /// <param name="forbiddenWord">Nowe dane słowa</param>
        /// <returns>204 No Content lub odpowiedni kod błędu</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateForbiddenWord(string id, ForbiddenWord forbiddenWord)
        {
            try
            {
                if (id != forbiddenWord.Id)
                {
                    return BadRequest(new { message = "ID w ścieżce nie zgadza się z ID w ciele żądania." });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingWord = await _forbiddenWords.Find(fw => fw.Id == id).FirstOrDefaultAsync();
                if (existingWord == null)
                {
                    return NotFound($"Zabronione słowo o ID {id} nie zostało znalezione.");
                }

                // Normalizacja
                forbiddenWord.Word = forbiddenWord.Word.ToLowerInvariant().Trim();

                var result = await _forbiddenWords.ReplaceOneAsync(fw => fw.Id == id, forbiddenWord);
                
                return NoContent();
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                return Conflict(new { message = $"Zabronione słowo '{forbiddenWord.Word}' już istnieje." });
            }
        }

        /// <summary>
        /// DELETE /api/v1/forbidden-words/{id}
        /// Usuwa zabronione słowo z bazy danych.
        /// </summary>
        /// <param name="id">ID słowa do usunięcia</param>
        /// <returns>204 No Content lub 404 Not Found</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForbiddenWord(string id)
        {
            var result = await _forbiddenWords.DeleteOneAsync(fw => fw.Id == id);

            if (result.DeletedCount == 0)
            {
                return NotFound($"Zabronione słowo o ID {id} nie zostało znalezione.");
            }

            return NoContent();
        }

        /// <summary>
        /// PATCH /api/v1/forbidden-words/{id}/toggle
        /// Przełącza status aktywności zakazanego słowa.
        /// </summary>
        /// <param name="id">ID słowa do przełączenia</param>
        /// <returns>200 OK z nowym statusem lub odpowiedni kod błędu</returns>
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult<ForbiddenWord>> ToggleForbiddenWordStatus(string id)
        {
            var word = await _forbiddenWords.Find(fw => fw.Id == id).FirstOrDefaultAsync();
            
            if (word == null)
            {
                return NotFound($"Zabronione słowo o ID {id} nie zostało znalezione.");
            }

            word.IsActive = !word.IsActive;
            await _forbiddenWords.ReplaceOneAsync(fw => fw.Id == id, word);

            return Ok(word);
        }
    }
}