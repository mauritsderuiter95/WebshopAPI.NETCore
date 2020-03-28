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
  public class CallBacksController : ControllerBase
  {
    private readonly CallBackService _callBackService;

    public CallBacksController(CallBackService callBackService)
    {
      _callBackService = callBackService;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
    [HttpGet]
    public ActionResult<List<CallBack>> GetList()
    {
      return _callBackService.GetList();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
    [HttpGet("{id:length(24)}", Name = "GetCallBack")]
    public ActionResult<CallBack> Get(string id)
    {
      var callBack = _callBackService.Get(id);

      if (callBack == null)
      {
        return NotFound();
      }

      return callBack;
    }

    [AllowAnonymous]
    [HttpPost]
    public ActionResult<CallBack> Create(CallBack callBack)
    {
      if (!_callBackService.CheckToken(callBack))
        return BadRequest();

      _callBackService.Create(callBack);

      return CreatedAtRoute("GetCallBack", new { id = callBack.Id.ToString() }, callBack);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
    [HttpPut("{id:length(24)}")]
    public IActionResult Update(string id, CallBack callBackIn)
    {
      var callBack = _callBackService.Get(id);

      if (callBack == null)
      {
        return NotFound();
      }

      callBack = _callBackService.Update(id, callBackIn);

      return CreatedAtRoute("GetCallBack", new { id = callBack.Id.ToString() }, callBack);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
    [HttpDelete("{id:length(24)}")]
    public IActionResult Delete(string id)
    {
      var callBack = _callBackService.Get(id);

      if (callBack == null)
      {
        return NotFound();
      }

      _callBackService.Remove(callBack.Id);

      return NoContent();
    }
  }
}
