using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace backend.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _carts;

        public CartService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            var database = client.GetDatabase("wrautomaten");
            _carts = database.GetCollection<Cart>("Carts");
        }

        public List<Cart> Get()
        {
            return _carts.Find(cart => true).ToList();
        }

        public Cart Get(string id)
        {
            return _carts.Find<Cart>(cart => cart.Id == id).FirstOrDefault();
        }

        public Cart Create(CartProduct cartProduct)
        {
            Cart cart = new Cart();
            cart.Products = new List<CartProduct>();
            cart.Products.Add(cartProduct);

            _carts.InsertOne(cart);
            return cart;
        }

        public Cart Update(string id, CartProduct cartProduct)
        {
            Cart cart = Get(id);
            CartProduct exists = cart.Products.Where(x => x.ProductId == cartProduct.Id).FirstOrDefault();
            if (exists != null)
                exists.Count = exists.Count + cartProduct.Count;
            else
                cart.Products.Add(cartProduct);

            _carts.ReplaceOne(c => c.Id == id, cart);

            return cart;

            //cartIn.Id = id;
            //_carts.ReplaceOne(cart => cart.Id == id, cartIn);
            //return cartIn;
        }

        public void Remove(Cart cartIn)
        {
            _carts.DeleteOne(cart => cart.Id == cartIn.Id);
        }

        public void Remove(string id)
        {
            _carts.DeleteOne(cart => cart.Id == id);
        }
    }
}
