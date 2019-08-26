﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        [HttpGet("{id}", Name = "Get")]
        public Order Get(string id)
        {
            return _orderService.Get(id);
        }

        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public ActionResult<string> Post(Cart cart)
        {
            string user = User.Identity.Name;
            return _orderService.Create(cart.Id, user).Result;
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