using MongoDB.Driver;
using ZtpLab1.Data;
using ZtpLab1.Models;

namespace ZtpLab1.Repositories
{
    /// <summary>
    /// Implementacja repozytorium historii produktów dla MongoDB.
    /// Implementuje wzorzec Repository Pattern dla dostępu do danych historii zmian.
    /// </summary>
    public class ProductHistoryRepository : IProductHistoryRepository
    {
        private readonly IMongoCollection<ProductHistory> _collection;

        public ProductHistoryRepository(MongoDbContext context)
        {
            _collection = context.ProductHistory;
        }

        public async Task<IEnumerable<ProductHistory>> GetAllAsync()
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(h => h.ChangedAt)
                .ToListAsync();
        }

        public async Task<ProductHistory?> GetByIdAsync(string id)
        {
            return await _collection.Find(h => h.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ProductHistory> AddAsync(ProductHistory history)
        {
            await _collection.InsertOneAsync(history);
            return history;
        }

        public async Task<bool> UpdateAsync(string id, ProductHistory history)
        {
            var result = await _collection.ReplaceOneAsync(h => h.Id == id, history);
            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(h => h.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _collection.CountDocumentsAsync(h => h.Id == id);
            return count > 0;
        }

        public async Task<IEnumerable<ProductHistory>> GetHistoryByProductIdAsync(string productId)
        {
            return await _collection
                .Find(h => h.ProductId == productId)
                .SortByDescending(h => h.ChangedAt)
                .ToListAsync();
        }

        public async Task<ProductHistory> AddHistoryAsync(ProductHistory history)
        {
            await _collection.InsertOneAsync(history);
            return history;
        }

        public async Task<ProductHistory?> GetLatestHistoryByProductIdAsync(string productId)
        {
            return await _collection
                .Find(h => h.ProductId == productId)
                .SortByDescending(h => h.ChangedAt)
                .FirstOrDefaultAsync();
        }
    }
}