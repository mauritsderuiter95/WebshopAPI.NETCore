﻿using backend.Models;
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
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Html2pdf;
using iText.StyledXmlParser.Css.Media;
using static backend.Models.Payment;
using System.Configuration;

namespace backend.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<Cart> _carts;
        private readonly IPaymentClient _paymentClient;
        private readonly MailService _mailService;
        // private readonly SchedulerService _schedulerService;


        public OrderService(IConfiguration config)
        {
            //string connectionString = ConfigurationExtensions.GetConnectionString(config, "MONGODB_CONNECTION");

            var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            //var client = new MongoClient(connectionString);

            var database = client.GetDatabase("wrautomaten");
            _orders = database.GetCollection<Order>("Orders");
            _carts = database.GetCollection<Cart>("Carts");

            var apikey = config.GetSection("Apikeys")["Mollie"];
            //var apikey = config.GetValue<string>("MOLLIE");

            _paymentClient = new PaymentClient(apikey);
            _mailService = new MailService(config);
            // _schedulerService = new SchedulerService(config);
        }

        public List<Order> List()
        {
            return _orders.Find<Order>(Order => true).SortByDescending(x => x.Ordernumber).ToList();
        }

        public List<Order> List(User user)
        {
            return _orders.Find<Order>(x => x.User.Id == user.Id).SortByDescending(x => x.Created).ToList();
        }

        public Order Get(string id)
        {
            return _orders.Find<Order>(x => x.Id == id).FirstOrDefault();
        }

        public List<Order> GetFrom(DateTime date)
        {
            return _orders.Find<Order>(x => x.Created >= date).ToList();
        }

        public async Task<Order> Create(Order order, User user)
        {
            Cart cart = _carts.Find<Cart>(x => x.Id == order.CartId).FirstOrDefault();

            if (user != null)
                order.User = user;

            //_carts.DeleteOne(x => x.Id == cartId);

            order.Created = DateTime.Now;

            order.Ordernumber = 1;

            order.Status = "Wachten op betaling";

            _orders.InsertOne(order);

            order.Ordernumber = GetNextSequenceValue();

            var key = new byte[32];
            RandomNumberGenerator.Create().GetBytes(key);
            string base64Secret = Convert.ToBase64String(key);
            // make safe for url
            order.Key = base64Secret.TrimEnd('=').Replace('+', '-').Replace('/', '_');

            order = await Payment(order);

            _orders.ReplaceOne(x => x.Id == order.Id, order);

            _mailService.SendOrderConfirmation(order, order.User);
            _mailService.SendOrderToWim(order, order.User);

            // _schedulerService.Add(() => _mailService.SendOrderConfirmation(order,user), 7);

            _carts.DeleteOne<Cart>(x => x.Id == order.CartId);

            return order;
        }

        public async Task<Order> Payment(Order order)
        {
            decimal total = 0;

            foreach (CartProduct product in order.Products)
            {
                total += (product.Count * product.ProductPrice);
            }

            if (order.Ideal)
            {
                PaymentRequest paymentRequest = new PaymentRequest()
                {
                    Amount = new Amount(Currency.EUR, total.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)),
                    Description = $"Bestelling { order.Ordernumber }",
                    RedirectUrl = $"https://www.wrautomaten.nl/orders/{ order.Id }?key={ order.Key }",
                    Metadata = "{\"order_id\":\"" + order.Id + "\",\"cart_id\":\"" + order.CartId + "\"}",
                    WebhookUrl = "https://backend.wrautomaten.nl/api/orders/webhook"
                };

                PaymentResponse paymentResponse = await _paymentClient.CreatePaymentAsync(paymentRequest);

                order.orderPayment = JsonConvert.DeserializeObject<Payment>(JsonConvert.SerializeObject(paymentResponse));

                return order;
            }

            order.orderPayment = new Payment();

            AmountClass amount = new AmountClass();

            amount.Currency = "EUR";
            amount.Value = total.ToString("0.00");

            order.orderPayment.Amount = amount;

            return order;
        }

        internal int GetNextSequenceValue()
        {
            Order order = _orders.Find<Order>(x => true).SortByDescending(x => x.Ordernumber).FirstOrDefault();

            return order != null ? order.Ordernumber + 1 ?? 0 : DateTime.Now.Year * 10000;
        }

        public MemoryStream CreatePDF(Order order)
        {
            var stream = new MemoryStream();
            string url = $"https://www.wrautomaten.nl/orders/{order.Id}/factuur?key={order.Key}";

            HttpWebRequest aRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();

            using (PdfWriter writer = new PdfWriter(stream))
            {
                writer.SetCloseStream(false);
                PdfDocument pdf = new PdfDocument(writer);
                PageSize pageSize = new PageSize(PageSize.A4);
                pdf.SetDefaultPageSize(pageSize);
                ConverterProperties properties = new ConverterProperties();
                MediaDeviceDescription mediaDeviceDescription =
                    new MediaDeviceDescription(MediaType.PRINT);
                mediaDeviceDescription.SetWidth(pageSize.GetWidth());
                properties.SetMediaDeviceDescription(mediaDeviceDescription);
                HtmlConverter.ConvertToPdf(aResponse.GetResponseStream(), pdf, properties);
            }

            stream.Position = 0;

            return stream;
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

            PaymentResponse result = await _paymentClient.GetPaymentAsync(mollieId);

            Payment payment = JsonConvert.DeserializeObject<Payment>(JsonConvert.SerializeObject(result));

            Order order = Get(payment.Metadata.Order_id);
            //Cart cart = _carts.Find<Cart>(x => x.Id == payment.Metadata.Cart_id).FirstOrDefault();

            order.orderPayment = payment;

            if (payment.Status == "Paid")
            {
                order.Status = "Betaald";
            }
            if (payment.Status != "Open" && payment.Status != "Paid")
                order.Status = "Betaling is mislukt";

            Put(payment.Metadata.Order_id, order);

            //_carts.DeleteOne(x => x.Id == cart.Id);

            return true;
        }
    }
}
