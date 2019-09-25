using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models.Mails.DailyUpdate;
using Newtonsoft.Json;

namespace backend.Models.Mails
{
    public class DailyUpdateMail : Mail
    {
        [JsonProperty("params")]
        public DailyUpdateParams Params { get; set; }
    }
}
