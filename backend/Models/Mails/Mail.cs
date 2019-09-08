using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Mails
{
    public class Mail
    {
        [JsonProperty("sender")]
        public EmailAddress Sender { get; set; }

        [JsonProperty("replyTo")]
        public EmailAddress ReplyTo { get; set; }

        [JsonProperty("to")]
        public List<EmailAddress> To { get; set; }

        [JsonProperty("templateId")]
        public int TemplateId { get; set; }

    }
}
