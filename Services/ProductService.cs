using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDbApp.Models;
using MongoDbApp.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MongoDbApp.Services
{
    public class ProductService
    {
        private readonly IGridFSBucket _gridFS;
        private readonly IMongoCollection<Product> _products;

        public ProductService(IConfiguration configuration)
        {
            string mongoConnectionString = configuration.GetConnectionString("MongoDbConnectionString");
            MongoUrlBuilder connection = new(mongoConnectionString);
            MongoClient mongoClient = new(mongoConnectionString);

            IMongoDatabase mongoDatabase = mongoClient.GetDatabase(connection.DatabaseName);

            _gridFS = new GridFSBucket(mongoDatabase);
            _products = mongoDatabase.GetCollection<Product>("Products");
        }

        public async Task<IEnumerable<Product>> GetProducts(int? minPrice, int? maxPrice, string name)
        {
            FilterDefinitionBuilder<Product> builder = new();
            FilterDefinition<Product> filter = builder.Empty;

            if (string.IsNullOrWhiteSpace(name) is false)
            {
                filter &= builder.Regex("Name", new BsonRegularExpression(name));
            }
            if (minPrice.HasValue is true)  // фильтр по минимальной цене
            {
                filter &= builder.Gte("Price", minPrice.Value);
            }
            if (maxPrice.HasValue is true)  // фильтр по максимальной цене
            {
                filter &= builder.Lte("Price", maxPrice.Value);
            }

            return await _products.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProducts(FilterViewModel viewModel)
        {
            FilterDefinitionBuilder<Product> builder = new();
            FilterDefinition<Product> filter = builder.Empty;

            if (string.IsNullOrWhiteSpace(viewModel.Name) is false)
            {
                filter &= builder.Regex("Name", new BsonRegularExpression(viewModel.Name));
            }
            if (viewModel.MinPrice.HasValue is true)  // фильтр по минимальной цене
            {
                filter &= builder.Gte("Price", viewModel.MinPrice);
            }
            if (viewModel.MaxPrice.HasValue is true)  // фильтр по максимальной цене
            {
                filter &= builder.Lte("Price", viewModel.MaxPrice);
            }

            return await _products.Find(filter).ToListAsync();
        }

        public async Task<Product> GetProduct(string id) => await _products.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();

        public async Task Create(Product product) => await _products.InsertOneAsync(product);

        public async Task Update(Product product) => await _products.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(product.Id)), product);

        public async Task Remove(string id) => await _products.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));

        public async Task<byte[]> GetImage(string id) => await _gridFS.DownloadAsBytesAsync(new ObjectId(id));

        public async Task StoreImage(string id, Stream imageStream, string imageName)
        {
            Product p = await GetProduct(id);

            //If product already has image delete it
            if (p.HasImage() is true) await _gridFS.DeleteAsync(new ObjectId(p.ImageId));

            ObjectId imageId = await _gridFS.UploadFromStreamAsync(imageName, imageStream);

            p.ImageId = imageId.ToString();

            var filter = Builders<Product>.Filter.Eq("_id", new ObjectId(p.Id));
            var update = Builders<Product>.Update.Set("ImageId", p.ImageId);

            await _products.UpdateOneAsync(filter, update);
        }

        public async Task DeleteImage(string id)
        {
            Product p = await GetProduct(id);

            //If product already has image delete it
            if (p.HasImage() is true) await _gridFS.DeleteAsync(new ObjectId(p.ImageId));

            var filter = Builders<Product>.Filter.Eq("_id", new ObjectId(p.Id));
            var update = Builders<Product>.Update.Unset("ImageId");

            await _products.UpdateOneAsync(filter, update);
        }
    }
}