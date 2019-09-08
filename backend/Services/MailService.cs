using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using backend.Models;
using backend.Models.Mails;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace backend.Services
{
    public class MailService
    {
        private static readonly HttpClient client = new HttpClient();

        private Mail settings = new Mail();

        public MailService(IConfiguration config)
        {
            EmailAddress ownMailAddress = new EmailAddress();
            ownMailAddress.Email = "maurits@wr-automaten.nl";

            settings.Sender = ownMailAddress;
            settings.ReplyTo = ownMailAddress;

            string apikey = config.GetSection("Apikeys")["Sendinblue"];

            client.DefaultRequestHeaders.Add("api-key", apikey);
            //client.DefaultRequestHeaders.Add("accept", "application/json");
        }

        public async Task<bool> SendConfirmation(Verification verification, User user)
        {
            var serialized = JsonConvert.SerializeObject(settings);
            var confirmationMail = JsonConvert.DeserializeObject<ConfirmationMail>(serialized);

            LinkParam linkParam = new LinkParam();
            linkParam.Link = $"https://www.wrautomaten.nl/account/confirmation/{verification.Id}";
            confirmationMail.jsonParams = linkParam;
            confirmationMail.TemplateId = 1;

            EmailAddress toEmail = new EmailAddress();
            toEmail.Email = user.Username;
            List<EmailAddress> emailAddresses = new List<EmailAddress>();
            emailAddresses.Add(toEmail);

            confirmationMail.To = emailAddresses;

            var jsonContent = JsonConvert.SerializeObject(confirmationMail);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.sendinblue.com/v3/smtp/email", content);

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
    }
}
