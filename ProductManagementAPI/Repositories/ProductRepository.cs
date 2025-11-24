using MongoDB.Driver;
using ZtpLab1.Data;
using ZtpLab1.Models;

namespace ZtpLab1.Repositories
{
    /// <summary>
    /// Implementacja repozytorium produktów dla MongoDB.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _collection;

        public ProductRepository(MongoDbContext context)
        {
            _collection = context.Products;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<object>> GetAllBasicInfoAsync()
        {
            return await _collection
                .Find(_ => true)
                .Project(p => new { p.Id, p.Name })
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Category, category);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<Product> AddAsync(Product product)
        {
            try
            {
                await _collection.InsertOneAsync(product);
                return product;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                throw new InvalidOperationException($"Produkt o nazwie '{product.Name}' już istnieje.", ex);
            }
        }

        public async Task<bool> UpdateAsync(string id, Product product)
        {
            try
            {
                var result = await _collection.ReplaceOneAsync(p => p.Id == id, product);
                return result.MatchedCount > 0;
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Code == 11000)
            {
                throw new InvalidOperationException($"Produkt o nazwie '{product.Name}' już istnieje.", ex);
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _collection.CountDocumentsAsync(p => p.Id == id);
            return count > 0;
        }

        public async Task<bool> IsNameExistsAsync(string name, string? excludeId = null)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Name, name);
            
            if (!string.IsNullOrEmpty(excludeId))
            {
                var excludeFilter = Builders<Product>.Filter.Ne(p => p.Id, excludeId);
                filter = Builders<Product>.Filter.And(filter, excludeFilter);
            }

            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }
    }
}