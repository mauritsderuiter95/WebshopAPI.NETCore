using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using backend.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace backend.Services
{
    public class ContactService
    {
        private readonly IMongoCollection<Contact> _contacts;
        private readonly MailService _mailService;

        private readonly string _recaptchaKey = string.Empty;

        public ContactService(IConfiguration config)
        {
            //string connectionString = ConfigurationExtensions.GetConnectionString(config, "MONGODB_CONNECTION");

            var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            //var client = new MongoClient(connectionString);

            var database = client.GetDatabase("wrautomaten");
            _contacts = database.GetCollection<Contact>("Contacts");

            _recaptchaKey = config.GetSection("Apikeys")["recaptcha"];

            _mailService = new MailService(config);
        }

        public List<Contact> GetList()
        {
            return _contacts.Find(contact => true).ToList();
        }

        public Contact Get(string id)
        {
            return _contacts.Find<Contact>(contact => contact.Id == id).FirstOrDefault();
        }

        public bool CheckToken(Contact contact)
        {
            HttpClient httpClient = new HttpClient();
            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={_recaptchaKey}&response={contact.Token}").Result;
            if (res.StatusCode != HttpStatusCode.OK)
                return false;

            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JsonConvert.DeserializeObject(JSONres);

            if (JSONdata.score > 0.5)
                return true;
            return false;
        }

        public Contact Create(Contact contact)
        {
            _contacts.InsertOne(contact);

            _mailService.SendContact(contact);

            return contact;
        }

        public Contact Update(string id, Contact contactIn)
        {
            contactIn.Id = id;
            _contacts.ReplaceOne(contact => contact.Id == id, contactIn);
            return contactIn;
        }

        public void Remove(string id)
        {
            _contacts.DeleteOne(contact => contact.Id == id);
        }
    }
}
