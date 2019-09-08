using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Mails
{
    public class ConfirmationMail : Mail
    {
        [JsonProperty("params")]
        public LinkParam jsonParams { get; set; }
    }
}
