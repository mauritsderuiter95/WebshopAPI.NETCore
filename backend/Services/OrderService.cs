using backend.Models;
using Microsoft.Extensions.Configuration;
using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Models;
using Mollie.Api.Models.Payment.Request;
using Mollie.Api.Models.Payment.Response;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Payment> _payments;
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<Cart> _carts;
        IPaymentClient paymentClient = new PaymentClient("live_6trGNpabstqksMGvKvpexn2zDCTJQT");


        public OrderService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            var database = client.GetDatabase("wrautomaten");
            _orders = database.GetCollection<Order>("Orders");
            _payments = database.GetCollection<Payment>("Payments");
            _carts = database.GetCollection<Cart>("Carts");
        }

        public List<Order> Get(int? limit)
        {
            if(limit == null)
                return _orders.Find<Order>(Order => true).SortByDescending(x => x.Created).ToList();
            else
            {
                int limitInt = limit.Value;
                return _orders.Find<Order>(Order => true).SortByDescending(x => x.Created).ToList().Take(limitInt).ToList();
            } 
        }

        public Order Get(string id)
        {
            return _orders.Find<Order>(x => x.Id == id).FirstOrDefault();
        }

        public List<Order> Get(User user, int? limit)
        {
            if (limit == null)
                return _orders.Find<Order>(x => x.User == user.Id).SortByDescending(x => x.Created).ToList();
            else
            {
                int limitInt = limit.Value;
                return _orders.Find<Order>(x => x.User == user.Id).SortByDescending(x => x.Created).ToList().Take(limitInt).ToList();
            }
        }

        public async Task<string> Create(string cartId, string user)
        {
            Order order = new Order();
            Cart cart = _carts.Find<Cart>(x => x.Id == cartId).FirstOrDefault();

            order.Products = cart.Products;

            if (!string.IsNullOrEmpty(user))
                order.User = user;

            //_carts.DeleteOne(x => x.Id == cartId);

            order.Created = DateTime.Now.ToString("d-M-yyy");
            
            order.Ordernumber = 1;

            order.Status = "Wachten op betaling";

            _orders.InsertOne(order);

            order.Ordernumber = GetNextSequenceValue();

            order = await Payment(order);

            _orders.ReplaceOne(x => x.Id == order.Id, order);

            return order.orderPayment._links.Checkout.Href;
        }

        public async Task<Order> Payment(Order order)
        {
            decimal total = 0;

            foreach (CartProduct product in order.Products)
            {
                total += (product.Count * product.ProductPrice);
            }

            PaymentRequest paymentRequest = new PaymentRequest()
            {
                Amount = new Amount(Currency.EUR, total.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)),
                Description = "Test payment of the example project",
                RedirectUrl = $"http://localhost:8081/orders/{ order.Id }",
                Metadata = "{\"order_id\":\"" + order.Id + "\"}",
                WebhookUrl = "http://fbfca914.ngrok.io/api/orders/webhook"
            };

            PaymentResponse paymentResponse = await paymentClient.CreatePaymentAsync(paymentRequest);

            Payment payment = JsonConvert.DeserializeObject<Payment>(JsonConvert.SerializeObject(paymentResponse));

            order.orderPayment = payment;

            return order;
        }

        internal int GetNextSequenceValue()
        {
            Order order = _orders.Find<Order>(x => true).SortByDescending(x => x.Ordernumber).FirstOrDefault();

            if (order != null)
                return order.Ordernumber + 1;
            else
                return DateTime.Now.Year * 10000;
        }

        public Order Put(string id, Order order)
        {
            Order newOrder = new Order();

            newOrder = order;

            newOrder.Id = id;

            _orders.ReplaceOne(x => x.Id == id, newOrder);

            return newOrder;
        }

        public async Task<bool> Verify(string mollieId)
        {
            mollieId = mollieId.Substring(3);

            PaymentResponse result = await paymentClient.GetPaymentAsync(mollieId);

            Payment payment = JsonConvert.DeserializeObject<Payment>(JsonConvert.SerializeObject(result));

            Order order = Get(payment.Metadata.Order_id);

            order.orderPayment = payment;

            if (payment.Status == "Paid")
                order.Status = "Betaald";
            if (payment.Status != "Open" && payment.Status != "Paid")
                order.Status = "Betaling is mislukt";

            Put(payment.Metadata.Order_id, order);

            return true;
        }
    }
}
