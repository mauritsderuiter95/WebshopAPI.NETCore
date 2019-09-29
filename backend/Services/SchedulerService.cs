using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Hangfire;
using Microsoft.Extensions.Configuration;

namespace backend.Services
{
    public class SchedulerService
    {
        private readonly MailService _mailService;
        private readonly OrderService _orderService;

        public SchedulerService(IConfiguration config)
        {
            _mailService = new MailService(config);
            _orderService = new OrderService(config);
        }

        public void Startup()
        {
            RecurringJob.AddOrUpdate(
                "SendDailyUpdate", 
                () => _mailService.SendDailyUpdate(_orderService.GetFrom(DateTime.Now.AddDays(-1))), 
                "0 16 * * *", 
                TimeZoneInfo.Local);
        }

        public void List()
        {

        }

        public void Add(Action action, int days)
        {
            BackgroundJob.Schedule(
                () => action(),
                TimeSpan.FromDays(days));
        }

        public void Get()
        {
            
        }

        public void Edit()
        {

        }

        public void Delete()
        {

        }
    }
}
