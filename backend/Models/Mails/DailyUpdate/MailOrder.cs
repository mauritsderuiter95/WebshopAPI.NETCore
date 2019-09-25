using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models.Mails.DailyUpdate
{
    public class MailOrder
    {
        public string Company { get; set; }

        public string LastName { get; set; }

        public string OrderNumber { get; set; }

        public decimal Amount { get; set; }
    }
}
