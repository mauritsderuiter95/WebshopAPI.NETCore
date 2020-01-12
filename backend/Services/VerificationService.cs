using backend.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class VerificationService
    {
        private readonly IMongoCollection<Verification> _verifications;

        public VerificationService(IConfiguration config)
        {
            var connectionKey = "MONGODB_CONNECTION";
            string connectionString = ConfigurationManager.ConnectionStrings[connectionKey].ConnectionString;

            // var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            var client = new MongoClient(connectionString);

            var database = client.GetDatabase("wrautomaten");
            _verifications = database.GetCollection<Verification>("Verifications");
        }

        public List<Verification> Get()
        {
            return _verifications.Find(cart => true).ToList();
        }

        public Verification Get(string id)
        {
            Verification verification = _verifications.Find<Verification>(x => x.Id == id).FirstOrDefault();

            if (verification.Used == true)
                return null;

            verification.Used = true;

            _verifications.ReplaceOne(x => x.Id == id, verification);

            return verification;
        }

        public Verification Create(string userId)
        {
            Verification verification = new Verification();
            verification.UserId = userId;
            verification.Used = false;

            _verifications.InsertOne(verification);

            return verification;
        }

        public void Delete(string id)
        {
            _verifications.DeleteOne(x => x.Id == id);
        }
    }
}
