using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace backend.Models.Mails.Contact
{
    public class ContactParams
    {
        [JsonProperty("NAME")]
        public string Name { get; set; }

        [JsonProperty("PHONE")]
        public string Phone { get; set; }

        [JsonProperty("EMAIL")]
        public string Email { get; set; }

        [JsonProperty("COMPANY")]
        public string Company { get; set; }

        [JsonProperty("MACHINE")]
        public string Machine { get; set; }

        [JsonProperty("MESSAGE")]
        public string Message { get; set; }
    }
}