using MongoDB.Driver;
using ZtpLab1.Data;
using ZtpLab1.Models;

namespace ZtpLab1.Repositories
{
    /// <summary>
    /// Implementacja repozytorium zabronionych słów dla MongoDB.
    /// </summary>
    public class ForbiddenWordRepository : IForbiddenWordRepository
    {
        private readonly IMongoCollection<ForbiddenWord> _collection;

        public ForbiddenWordRepository(MongoDbContext context)
        {
            _collection = context.ForbiddenWords;
        }

        public async Task<IEnumerable<ForbiddenWord>> GetAllAsync()
        {
            return await _collection.Find(_ => true).SortBy(fw => fw.Word).ToListAsync();
        }

        public async Task<IEnumerable<ForbiddenWord>> GetActiveAsync()
        {
            var filter = Builders<ForbiddenWord>.Filter.Eq(fw => fw.IsActive, true);
            return await _collection.Find(filter).SortBy(fw => fw.Word).ToListAsync();
        }

        public async Task<ForbiddenWord?> GetByIdAsync(string id)
        {
            return await _collection.Find(fw => fw.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ForbiddenWord> AddAsync(ForbiddenWord forbiddenWord)
        {
            try
            {
                // Normalizacja słowa do małych liter
                forbiddenWord.Word = forbiddenWord.Word.ToLowerInvariant().Trim();
                await _collection.InsertOneAsync(forbiddenWord);
                return forbiddenWord;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                throw new InvalidOperationException($"Zabronione słowo '{forbiddenWord.Word}' już istnieje.", ex);
            }
        }

        public async Task<bool> UpdateAsync(string id, ForbiddenWord forbiddenWord)
        {
            try
            {
                // Normalizacja słowa do małych liter
                forbiddenWord.Word = forbiddenWord.Word.ToLowerInvariant().Trim();
                var result = await _collection.ReplaceOneAsync(fw => fw.Id == id, forbiddenWord);
                return result.MatchedCount > 0;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                throw new InvalidOperationException($"Zabronione słowo '{forbiddenWord.Word}' już istnieje.", ex);
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(fw => fw.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _collection.CountDocumentsAsync(fw => fw.Id == id);
            return count > 0;
        }

        public async Task<bool> IsWordExistsAsync(string word, string? excludeId = null)
        {
            var normalizedWord = word.ToLowerInvariant().Trim();
            var filter = Builders<ForbiddenWord>.Filter.Eq(fw => fw.Word, normalizedWord);
            
            if (!string.IsNullOrEmpty(excludeId))
            {
                var excludeFilter = Builders<ForbiddenWord>.Filter.Ne(fw => fw.Id, excludeId);
                filter = Builders<ForbiddenWord>.Filter.And(filter, excludeFilter);
            }

            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<ForbiddenWord?> ToggleActiveStatusAsync(string id)
        {
            var forbiddenWord = await GetByIdAsync(id);
            if (forbiddenWord == null)
                return null;

            forbiddenWord.IsActive = !forbiddenWord.IsActive;
            await _collection.ReplaceOneAsync(fw => fw.Id == id, forbiddenWord);
            return forbiddenWord;
        }
    }
}