using backend.Models.Mails.OrderConfirmation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Mails
{
    public class OrderConfirmationMail : Mail
    {
        [JsonProperty("params")]
        public OrderConfirmationParams Params { get; set; }
    }
}
