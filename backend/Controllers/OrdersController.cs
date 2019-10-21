using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly UserService _userService;

        public OrdersController(OrderService orderService, UserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // GET: api/Payments
        [HttpGet]
        public IEnumerable<Order> Get([FromQuery]int? limit)
        {
            string currentUserId = User.Identity.Name;

            User user =_userService.Get(currentUserId);

            IEnumerable<Order> orders;

            if (!User.IsInRole(Role.Admin))
                orders = _orderService.Get(user, limit);
            else
                orders = _orderService.Get(limit);

            return orders;
        }

        // GET: api/Payments/5
        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id}", Name = "Get")]
        public ActionResult<Order> Get(string id, [FromQuery]string key)
        {
            User user = new Models.User();

            if(!string.IsNullOrEmpty(User.Identity.Name))
                user = _userService.Get(User.Identity.Name);

            Console.WriteLine(id);
            Order order =_orderService.Get(id);

            if (order.User.Id == user.Id || User.IsInRole(Role.Admin))
            {
                return order;
            }
            else if (order.Key == key)
            {
                return order;
            }
            else
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id}/invoice", Name = "GetInvoice")]
        public IActionResult GetPdf(string id, [FromQuery]string key)
        {
            User user = new Models.User();

            if (!string.IsNullOrEmpty(User.Identity.Name))
                user = _userService.Get(User.Identity.Name);

            Order order = _orderService.Get(id);

            if (order.User.Id == user.Id || User.IsInRole(Role.Admin) || order.Key == key)
            {
                var pdf = _orderService.CreatePDF(order);
                return File(pdf, "application/pdf", $"Factuur{order.Ordernumber}.pdf");
            }
            return BadRequest();
        }

        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public ActionResult<Order> Post(Order order)
        {
            string username = User.Identity.Name;

            User user = _userService.Get(username);

            return _orderService.Create(order, user).Result;
        }

        // PUT: api/Payments/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpPost("webhook")]
        public async void Webhook()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var mollieId = reader.ReadToEnd();
                await _orderService.Verify(mollieId);
            }
        }
    }
}
