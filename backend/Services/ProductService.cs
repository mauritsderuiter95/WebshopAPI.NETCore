using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace backend.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService(IConfiguration config)
        {
            string connectionString = config.GetConnectionString("MONGODB_CONNECTION");

            // var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            var client = new MongoClient(connectionString);

            var database = client.GetDatabase("wrautomaten");
            _products = database.GetCollection<Product>("Products");
        }

        public List<Product> GetList(string category)
        {
            if (string.IsNullOrEmpty(category))
                return _products.Find(product => true).ToList();
            else
            {
                try
                {
                    List<Product> products = _products.Find(product => true).ToList();
                    //.Where(x => x.Category.Contains(category)).ToList();

                    products = products.Where(x => x.Category != null && x.Category.Any(i => i.ToLower() == category.ToLower())).ToList();
                    return products;
                }
                catch(Exception e)
                {
                    return null;
                }
            }
        }

        public Product Get(string id)
        {
            return _products.Find<Product>(product => product.Id == id).FirstOrDefault();
        }

        public List<string> GetCat()
        {
            List<Product> products = GetList(null);

            return products.Where(x => x.Category != null).SelectMany(x => x.Category).Distinct().ToList();
        }

        public Product Create(Product product)
        {
            _products.InsertOne(product);
            return product;
        }

        public Product Update(string id, Product productIn)
        {
            productIn.Id = id;
            _products.ReplaceOne(product => product.Id == id, productIn);
            return productIn;
        }

        public void Remove(Product productIn)
        {
            _products.DeleteOne(product => product.Id == productIn.Id);
        }

        public void Remove(string id)
        {
            _products.DeleteOne(product => product.Id == id);
        }
    }
}
