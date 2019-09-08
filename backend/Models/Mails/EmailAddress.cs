using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Mails
{
    public class EmailAddress
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
