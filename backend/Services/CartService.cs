﻿using System;
using System.Collections.Generic;
using System.Configuration;
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
            //string connectionString = ConfigurationExtensions.GetConnectionString(config, "MONGODB_CONNECTION");

            var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            //var client = new MongoClient(connectionString);

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
            return cart;
        }

        public Cart Create(CartProduct cartProduct)
        {
            Cart cart = new Cart();
            cart.Products = new List<CartProduct>();
            cart.Products.Add(cartProduct);

            if (cart.Products.Count > 0)
            {
                foreach (CartProduct cP in cart.Products)
                {
                    Product product = _products.Find<Product>(x => x.Id == cP.ProductId).FirstOrDefault();
                    cP.ProductName = product.ProductName;
                    cP.ProductPrice = product.Price;
                    cP.Photo = product.Photo;
                }
            }

            _carts.InsertOne(cart);
            
            return cart;
        }

        public Cart Update(string id, CartProduct cartProduct)
        {
            Cart cart = Get(id);
            CartProduct exists = cart.Products.Where(x => x.ProductId == cartProduct.ProductId).FirstOrDefault();
            if (exists != null)
            {
                exists.Count = exists.Count + cartProduct.Count;
            }
            else
                cart.Products.Add(cartProduct);

            if (cart.Products.Count > 0)
            {
                foreach (CartProduct cP in cart.Products)
                {
                    Product product = _products.Find<Product>(x => x.Id == cP.ProductId).FirstOrDefault();
                    cP.ProductName = product.ProductName;
                    cP.ProductPrice = product.Price;
                    cP.Photo = product.Photo;
                }
            }

            _carts.ReplaceOne(c => c.Id == id, cart);

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
                    cP.ProductPrice = product.Price;
                    cP.Photo = product.Photo;
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
