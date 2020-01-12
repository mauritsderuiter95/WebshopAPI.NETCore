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
    public class MachineService
    {
        private readonly IMongoCollection<Machine> _machines;

        public MachineService(IConfiguration config)
        {
            string connectionString = ConfigurationExtensions.GetConnectionString(config, "MONGODB_CONNECTION");

            // var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            var client = new MongoClient(connectionString);

            var database = client.GetDatabase("wrautomaten");
            _machines = database.GetCollection<Machine>("Machines");
        }

        public List<Machine> GetList(string type)
        {
            if (string.IsNullOrEmpty(type))
                return _machines.Find(machine => true).ToList();
            else
            {
                try
                {
                    List<Machine> machines = _machines.Find(machine => true).ToList();

                    machines = machines.Where(x => x.Type != null && x.Type.Any(i => i.ToLower() == type.ToLower())).ToList();
                    return machines;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public Machine Get(string id)
        {
            return _machines.Find<Machine>(machine => machine.Id == id).FirstOrDefault();
        }

        public Machine Create(Machine machine)
        {
            _machines.InsertOne(machine);
            return machine;
        }

        public Machine Update(string id, Machine machineIn)
        {
            machineIn.Id = id;
            _machines.ReplaceOne(machine => machine.Id == id, machineIn);
            return machineIn;
        }

        public void Remove(Machine machineIn)
        {
            _machines.DeleteOne(machine => machine.Id == machineIn.Id);
        }

        public void Remove(string id)
        {
            _machines.DeleteOne(machine => machine.Id == id);
        }
    }
}
