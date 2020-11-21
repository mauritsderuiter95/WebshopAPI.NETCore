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
    public class MachinesController : ControllerBase
    {
        private readonly MachineService _machineService;

        public MachinesController(MachineService machineService)
        {
            _machineService = machineService;
        }

        [HttpGet]
        public ActionResult<List<Machine>> GetList([FromQuery]string type)
        {
            return _machineService.GetList(type);
        }

        [HttpGet("{id}", Name = "GetMachine")]
        public ActionResult<Machine> Get(string id)
        {
            id = id.Replace('-', ' ');
            var machine = _machineService.GetByName(id);

            if (machine == null)
                machine = _machineService.Get(id);

            if (machine == null)
                return NotFound();

            return machine;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpPost]
        public ActionResult<Machine> Create(Machine machine)
        {
            _machineService.Create(machine);

            return CreatedAtRoute("GetMachine", new { id = machine.Id.ToString() }, machine);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Machine machineIn)
        {
            var machine = _machineService.Get(id);

            if (machine == null)
            {
                return NotFound();
            }

            machine = _machineService.Update(id, machineIn);

            return CreatedAtRoute("GetMachine", new { id = machine.Id.ToString() }, machine);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var machine = _machineService.Get(id);

            if (machine == null)
            {
                return NotFound();
            }

            _machineService.Remove(machine.Id);

            return NoContent();
        }
    }
}