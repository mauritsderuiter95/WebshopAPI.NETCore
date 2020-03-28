using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using backend.Models;
using backend.Models.Mails;
using backend.Models.Mails.DailyUpdate;
using backend.Models.Mails.OrderConfirmation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace backend.Services
{
  public class MailService
  {
    private static readonly HttpClient client = new HttpClient();
    private readonly Mail settings = new Mail();
    private readonly HttpRequestMessage requestMessage;
    private readonly EmailAddress _emailAddress = new EmailAddress();
    private readonly string _apikey = string.Empty;

    public MailService(IConfiguration config)
    {
      _emailAddress.Email = "info@wrautomaten.nl";

      settings.Sender = _emailAddress;
      settings.ReplyTo = _emailAddress;

      _apikey = config.GetSection("Apikeys")["Sendinblue"];
      //string apikey = config.GetValue<string>("SENDINBLUE");

      requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.sendinblue.com/v3/smtp/email");
      requestMessage.Headers.Add("api-key", _apikey);
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

      if (response.IsSuccessStatusCode)
        return true;
      else
        return false;
    }

    public void SendOrderConfirmation(Order order, User user)
    {
      var confirmationMail = ConfirmationMail(order, user);

      confirmationMail.TemplateId = 2;

      EmailAddress toEmail = new EmailAddress();
      toEmail.Email = user.Username;
      List<EmailAddress> emailAddresses = new List<EmailAddress> { toEmail };

      if (order.Ideal)
        confirmationMail.Params.Payment = order.orderPayment._links.Checkout.Href.Replace("https://", "");

      confirmationMail.Params.Link = $"www.wrautomaten.nl/orders/{order.Id}?key={order.Key}";

      confirmationMail.To = emailAddresses;

      var jsonContent = JsonConvert.SerializeObject(confirmationMail);

      requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

      client.SendAsync(requestMessage);
    }

    public void SendOrderToWim(Order order, User user)
    {
      var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendinblue.com/v3/smtp/email");

      request.Headers.Add("api-key", _apikey);

      var confirmationMail = ConfirmationMail(order, user);

      confirmationMail.TemplateId = 5;

      EmailAddress toEmail = new EmailAddress();
      toEmail.Email = "wim@wr-automaten.nl";
      List<EmailAddress> emailAddresses = new List<EmailAddress> { toEmail };

      confirmationMail.To = emailAddresses;

      var jsonContent = JsonConvert.SerializeObject(confirmationMail);

      request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

      client.SendAsync(request);
    }

    public void SendCallBack(CallBack callBack)
    {
      var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendinblue.com/v3/smtp/email");

      request.Headers.Add("api-key", _apikey);

      var serialized = JsonConvert.SerializeObject(settings);
      var callBackMail = JsonConvert.DeserializeObject<CallBackMail>(serialized);

      callBackMail.TemplateId = 6;

      callBackMail.Params = new Models.Mails.CallBack.CallBackParams
      {
        Name = callBack.Name,
        Phone = callBack.Phone
      };

      EmailAddress toEmail = new EmailAddress();
      toEmail.Email = "wim@wr-automaten.nl";
      List<EmailAddress> emailAddresses = new List<EmailAddress> { toEmail };

      callBackMail.To = emailAddresses;

      var jsonContent = JsonConvert.SerializeObject(callBackMail);

      request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

      client.SendAsync(request);
    }

    private OrderConfirmationMail ConfirmationMail(Order order, User user)
    {
      var serialized = JsonConvert.SerializeObject(settings);
      var confirmationMail = JsonConvert.DeserializeObject<OrderConfirmationMail>(serialized);

      confirmationMail.Params = new OrderConfirmationParams
      {
        FirstName = user.FirstName,
        DeliveryAddress = new Models.Mails.OrderConfirmation.Address
        {
          Company = user.Company,
          Name = $"{user.FirstName} {user.LastName}",
          Street = user.Street,
          Street2 = user.Street2,
          Zipcode = user.Zipcode,
          City = user.City
        },
        Products = new List<ProductsProduct>(),
        OrderNumber = order.Ordernumber.ToString()
      };

      foreach (CartProduct product in order.Products)
      {
        ProductsProduct productsProduct = new ProductsProduct();
        productsProduct.ProductName = product.ProductName;
        if (product.Photo != null)
          productsProduct.Image = product.Photo.Url;
        productsProduct.ShortDescription = $"€{product.ProductPrice:0.00}";

        confirmationMail.Params.Products.Add(productsProduct);
      }

      return confirmationMail;
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

      return response.IsSuccessStatusCode;
    }

    public void SendDailyUpdate(List<Order> orders)
    {
      if (orders.Count == 0)
        return;

      var serialized = JsonConvert.SerializeObject(settings);
      var dailyUpdateMail = JsonConvert.DeserializeObject<DailyUpdateMail>(serialized);

      dailyUpdateMail.TemplateId = 4;

      List<EmailAddress> toEmailAddresses = new List<EmailAddress>();
      EmailAddress address = new EmailAddress();
      address.Email = "mauritsderuiter95@gmail.com";
      toEmailAddresses.Add(address);
      dailyUpdateMail.To = toEmailAddresses;

      List<MailOrder> mailOrders = new List<MailOrder>();

      foreach (Order order in orders)
      {
        MailOrder mailOrder = new MailOrder();

        mailOrder.Company = order.User.Company;
        mailOrder.LastName = order.User.LastName;
        mailOrder.OrderNumber = order.Ordernumber.ToString();
        mailOrder.Amount = order.Products.Select(x => x.Count * x.ProductPrice).Sum();
        mailOrder.Amount += order.SendingCosts;

        mailOrders.Add(mailOrder);
      }

      dailyUpdateMail.Params = new DailyUpdateParams();
      dailyUpdateMail.Params.Orders = new List<MailOrder>();
      dailyUpdateMail.Params.Orders = mailOrders;

      var jsonContent = JsonConvert.SerializeObject(dailyUpdateMail);
      requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

      client.SendAsync(requestMessage);
    }
  }
}
