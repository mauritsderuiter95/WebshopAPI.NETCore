using System;
using System.Collections.Generic;
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
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public ActionResult<List<Product>> GetList([FromQuery]string category)
        {
            return _productService.GetList(category);
        }

        [HttpGet("categories")]
        public ActionResult<List<string>> GetCat()
        {
            return _productService.GetCat();
        }

        [HttpGet("{id}", Name = "GetProduct")]
        public ActionResult<Product> Get(string id)
        {
            id = id.Replace('-', ' ');

            var product = _productService.GetByName(id);

            if (product == null)
                product = _productService.Get(id);

            if (product == null)
                return NotFound();

            return product;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpPost]
        public ActionResult<Product> Create(Product product)
        {
            _productService.Create(product);

            return CreatedAtRoute("GetProduct", new { id = product.Id.ToString() }, product);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Product productIn)
        {
            var product = _productService.Get(id);

            if (product == null)
            {
                return NotFound();
            }

            product = _productService.Update(id, productIn);

            return CreatedAtRoute("GetProduct", new { id = product.Id.ToString() }, product);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var product = _productService.Get(id);

            if (product == null)
            {
                return NotFound();
            }

            _productService.Remove(product.Id);

            return NoContent();
        }
    }
}
