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
        private readonly IMongoCollection<Product> _products;

        public CartService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            var database = client.GetDatabase("wrautomaten");
            _carts = database.GetCollection<Cart>("Carts");
            _products = database.GetCollection<Product>("Products");
        }

        public List<Cart> Get()
        {
            return _carts.Find(cart => true).ToList();
        }

        public Cart Get(string id)
        {
            Cart cart = _carts.Find<Cart>(x => x.Id == id).FirstOrDefault();
            if (cart == null)
                return null;
            if(cart.Products.Count > 0)
            {
                foreach(CartProduct cartProduct in cart.Products)
                {
                    Product product = _products.Find<Product>(x => x.Id == cartProduct.ProductId).FirstOrDefault();
                    cartProduct.ProductName = product.ProductName;
                }
            }
            return cart;
        }

        public Cart Create(CartProduct cartProduct)
        {
            Cart cart = new Cart();
            cart.Products = new List<CartProduct>();
            cart.Products.Add(cartProduct);

            _carts.InsertOne(cart);
            if (cart.Products.Count > 0)
            {
                foreach (CartProduct cP in cart.Products)
                {
                    Product product = _products.Find<Product>(x => x.Id == cP.ProductId).FirstOrDefault();
                    cP.ProductName = product.ProductName;
                }
            }
            return cart;
        }

        public Cart Update(string id, CartProduct cartProduct)
        {
            Cart cart = Get(id);
            CartProduct exists = cart.Products.Where(x => x.ProductId == cartProduct.ProductId).FirstOrDefault();
            if (exists != null)
            {
                int.TryParse(exists.Count, out int dbCount);
                int.TryParse(cartProduct.Count, out int inputCount);
                exists.Count = (dbCount + inputCount).ToString();
            }
            else
                cart.Products.Add(cartProduct);

            _carts.ReplaceOne(c => c.Id == id, cart);

            if (cart.Products.Count > 0)
            {
                foreach (CartProduct cP in cart.Products)
                {
                    Product product = _products.Find<Product>(x => x.Id == cP.ProductId).FirstOrDefault();
                    cP.ProductName = product.ProductName;
                }
            }

            return cart;

            //cartIn.Id = id;
            //_carts.ReplaceOne(cart => cart.Id == id, cartIn);
            //return cartIn;
        }

        public Cart UpdateCart(string id, Cart cartIn)
        {
            cartIn.Id = id;
            _carts.ReplaceOne(cart => cart.Id == id, cartIn);
            if (cartIn.Products.Count > 0)
            {
                foreach (CartProduct cP in cartIn.Products)
                {
                    Product product = _products.Find<Product>(x => x.Id == cP.ProductId).FirstOrDefault();
                    cP.ProductName = product.ProductName;
                }
            }
            return cartIn;
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
