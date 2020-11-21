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
    public class ContactsController : ControllerBase
    {
        private readonly ContactService _contactService;

        public ContactsController(ContactService contactService)
        {
            _contactService = contactService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpGet]
        public ActionResult<List<Contact>> GetList()
        {
            return _contactService.GetList();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpGet("{id:length(24)}", Name = "GetContact")]
        public ActionResult<Contact> Get(string id)
        {
            var contact = _contactService.Get(id);

            if (contact == null)
            {
                return NotFound();
            }

            return contact;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult<Contact> Create(Contact contact)
        {
            if (!_contactService.CheckToken(contact))
                return BadRequest();

            _contactService.Create(contact);

            return CreatedAtRoute("GetContact", new { id = contact.Id.ToString() }, contact);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Contact contactIn)
        {
            var contact = _contactService.Get(id);

            if (contact == null)
            {
                return NotFound();
            }

            contact = _contactService.Update(id, contactIn);

            return CreatedAtRoute("GetCallBack", new { id = contact.Id.ToString() }, contact);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var contact = _contactService.Get(id);

            if (contact == null)
                return NotFound();

            _contactService.Remove(contact.Id);

            return NoContent();
        }
    }
}
