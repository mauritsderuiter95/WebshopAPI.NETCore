using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace backend.Models.Mails.DailyUpdate
{
    public class DailyUpdateParams
    {
        [JsonProperty("ORDERS")]
        public  List<MailOrder> Orders { get; set; }
    }
}
