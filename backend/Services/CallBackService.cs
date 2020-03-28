using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using backend.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace backend.Services
{
  public class CallBackService
  {
    private readonly IMongoCollection<CallBack> _callBacks;
    private readonly MailService _mailService;

    private readonly string _recaptchaKey = string.Empty;

    public CallBackService(IConfiguration config)
    {
      //string connectionString = ConfigurationExtensions.GetConnectionString(config, "MONGODB_CONNECTION");

      var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
      //var client = new MongoClient(connectionString);

      var database = client.GetDatabase("wrautomaten");
      _callBacks = database.GetCollection<CallBack>("CallBacks");

      _recaptchaKey = config.GetSection("Apikeys")["recaptcha"];

      _mailService = new MailService(config);
    }

    public List<CallBack> GetList()
    {
      return _callBacks.Find(callBack => true).ToList();
    }

    public CallBack Get(string id)
    {
      return _callBacks.Find<CallBack>(callBack => callBack.Id == id).FirstOrDefault();
    }

    public bool CheckToken(CallBack callBack)
    {
      HttpClient httpClient = new HttpClient();
      var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={_recaptchaKey}&response={callBack.Token}").Result;
      if (res.StatusCode != HttpStatusCode.OK)
        return false;

      string JSONres = res.Content.ReadAsStringAsync().Result;
      dynamic JSONdata = JsonConvert.DeserializeObject(JSONres);

      if (JSONdata.score > 0.5)
        return true;
      return false;
    }

    public CallBack Create(CallBack callBack)
    {
      _callBacks.InsertOne(callBack);

      _mailService.SendCallBack(callBack);

      return callBack;
    }

    public CallBack Update(string id, CallBack callBackIn)
    {
      callBackIn.Id = id;
      _callBacks.ReplaceOne(callBack => callBack.Id == id, callBackIn);
      return callBackIn;
    }

    public void Remove(string id)
    {
      _callBacks.DeleteOne(callBack => callBack.Id == id);
    }
  }
}
