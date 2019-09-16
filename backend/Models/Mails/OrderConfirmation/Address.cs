using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Mails.OrderConfirmation
{
    public class Address
    {
        public string Company { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string Zipcode { get; set; }
        public string City { get; set; }
    }
}
