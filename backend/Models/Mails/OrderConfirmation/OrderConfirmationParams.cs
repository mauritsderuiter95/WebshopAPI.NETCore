using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Mails.OrderConfirmation
{
    public class OrderConfirmationParams
    {
        [JsonProperty("PRODUCTS")]
        public List<ProductsProduct> Products { get; set; }

        [JsonProperty("DATE")]
        public string Date
        {
            get
            {
                return DateTime.Now.AddDays(3).ToString("d-M-yyyy");
            }
        }

        [JsonProperty("ORDER")]
        public string OrderNumber { get; set; }

        [JsonProperty("DELIVERYADDRESS")]
        public Address DeliveryAddress { get; set; }

        [JsonProperty("FIRSTNAME")]
        public string FirstName { get; set; }

        public string Payment { get; set; }
    }
}
