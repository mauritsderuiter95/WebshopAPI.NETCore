using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Mails.OrderConfirmation
{
    public class ProductsProduct
    {
        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }
    }
}
