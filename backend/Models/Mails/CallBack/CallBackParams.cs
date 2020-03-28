using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace backend.Models.Mails.CallBack
{
  public class CallBackParams
  {
    [JsonProperty("NAME")]
    public string Name { get; set; }

    [JsonProperty("PHONE")]
    public string Phone { get; set; }
  }
}