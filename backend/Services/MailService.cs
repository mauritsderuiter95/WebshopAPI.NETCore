using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using backend.Models;
using backend.Models.Mails;
using backend.Models.Mails.OrderConfirmation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace backend.Services
{
    public class MailService
    {
        private static readonly HttpClient client = new HttpClient();
        private Mail settings = new Mail();
        HttpRequestMessage requestMessage;

        public MailService(IConfiguration config)
        {
            EmailAddress ownMailAddress = new EmailAddress();
            ownMailAddress.Email = "info@wrautomaten.nl";

            settings.Sender = ownMailAddress;
            settings.ReplyTo = ownMailAddress;

            string apikey = config.GetSection("Apikeys")["Sendinblue"];

            requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.sendinblue.com/v3/smtp/email");
            requestMessage.Headers.Add("api-key", apikey);
        }

        public async Task<bool> SendConfirmation(Verification verification, User user)
        {
            var serialized = JsonConvert.SerializeObject(settings);
            var confirmationMail = JsonConvert.DeserializeObject<LinkMail>(serialized);

            LinkParam linkParam = new LinkParam();
            linkParam.Link = $"https://www.wrautomaten.nl/account/confirmation/{verification.Id}";
            confirmationMail.JsonParams = linkParam;
            confirmationMail.TemplateId = 1;

            EmailAddress toEmail = new EmailAddress();
            toEmail.Email = user.Username;
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(toEmail);

            confirmationMail.To = emailAddresses;

            var jsonContent = JsonConvert.SerializeObject(confirmationMail);

            requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(requestMessage);

            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Content.ToString());
            Console.WriteLine(response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

        public async void SendOrderConfirmation(Order order, User user)
        {
            var serialized = JsonConvert.SerializeObject(settings);
            var confirmationMail = JsonConvert.DeserializeObject<OrderConfirmationMail>(serialized);

            confirmationMail.TemplateId = 2;

            confirmationMail.Params = new OrderConfirmationParams();

            confirmationMail.Params.FirstName = user.FirstName;

            confirmationMail.Params.DeliveryAddress = new Models.Mails.OrderConfirmation.Address();
            confirmationMail.Params.DeliveryAddress.Company = user.Company;
            confirmationMail.Params.DeliveryAddress.Name = $"{user.FirstName} {user.LastName}";
            confirmationMail.Params.DeliveryAddress.Street = user.Street;
            confirmationMail.Params.DeliveryAddress.Street2 = user.Street2;
            confirmationMail.Params.DeliveryAddress.Zipcode = user.Zipcode;
            confirmationMail.Params.DeliveryAddress.City = user.City;


            EmailAddress toEmail = new EmailAddress();
            toEmail.Email = user.Username;
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(toEmail);

            confirmationMail.Params.Products = new List<ProductsProduct>();

            foreach (CartProduct product in order.Products)
            {
                ProductsProduct productsProduct = new ProductsProduct();
                productsProduct.ProductName = product.ProductName;
                productsProduct.Image = product.Photo.Url;
                productsProduct.ShortDescription = $"€{product.ProductPrice.ToString("0.00")}";

                confirmationMail.Params.Products.Add(productsProduct);
            }

            confirmationMail.Params.OrderNumber = order.Ordernumber.ToString();
            confirmationMail.Params.Payment = order.orderPayment._links.Checkout.Href.Replace("https://", "");

            confirmationMail.To = emailAddresses;

            var jsonContent = JsonConvert.SerializeObject(confirmationMail);

            requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(requestMessage);

            Console.WriteLine(response.StatusCode);
            string receiveStream = await response.Content.ReadAsStringAsync();
            Console.WriteLine(receiveStream);
        }

        public async Task<bool> SendPasswordReset(User user)
        {
            var serialized = JsonConvert.SerializeObject(settings);
            var resetMail = JsonConvert.DeserializeObject<LinkMail>(serialized);

            LinkParam linkParam = new LinkParam();
            linkParam.Link = $"https://www.wrautomaten.nl/account/passwordreset/{user.ResetKey}";
            resetMail.JsonParams = linkParam;
            resetMail.TemplateId = 3;

            EmailAddress toEmail = new EmailAddress();
            toEmail.Email = user.Username;
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(toEmail);

            resetMail.To = emailAddresses;

            var jsonContent = JsonConvert.SerializeObject(resetMail);

            requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(requestMessage);

            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Content.ToString());
            Console.WriteLine(response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
    }
}
