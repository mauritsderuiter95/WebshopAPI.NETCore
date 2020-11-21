using backend.Models.Mails.Contact;
using Newtonsoft.Json;

namespace backend.Models.Mails
{
    public class ContactMail : Mail
    {
        [JsonProperty("params")]
        public ContactParams Params { get; set; }
    }
}