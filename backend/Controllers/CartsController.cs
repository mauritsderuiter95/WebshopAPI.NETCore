using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartsController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public ActionResult<List<Cart>> Get()
        {
            return _cartService.Get();
        }

        [HttpGet("{id:length(24)}", Name = "GetCart")]
        public ActionResult<Cart> Get(string id)
        {
            var cart = _cartService.Get(id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        [HttpPost]
        public ActionResult<Cart> Create(CartProduct cartProduct)
        {
            Cart cart = _cartService.Create(cartProduct);

            return CreatedAtRoute("GetCart", new { id = cart.Id.ToString() }, cart);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, CartProduct cartProduct)
        {
            var cart = _cartService.Get(id);

            if (cart == null)
            {
                return NotFound();
            }

            cart = _cartService.Update(id, cartProduct);

            return CreatedAtRoute("GetCart", new { id = cart.Id.ToString() }, cart); ;
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var cart = _cartService.Get(id);

            if (cart == null)
            {
                return NotFound();
            }

            _cartService.Remove(cart.Id);

            return NoContent();
        }
    }
}
