using backend.Models.Mails.CallBack;
using Newtonsoft.Json;

namespace backend.Models.Mails
{
  public class CallBackMail : Mail
  {
    [JsonProperty("params")]
    public CallBackParams Params { get; set; }
  }
}