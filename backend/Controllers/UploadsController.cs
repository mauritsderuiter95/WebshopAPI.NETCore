using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly UploadService _uploadService;
        private readonly HttpContext _httpContext;

        public UploadsController(IHostingEnvironment hostingEnvironment, UploadService uploadService, IHttpContextAccessor contextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _uploadService = uploadService;
            _httpContext = contextAccessor.HttpContext;

        }

        [HttpGet("{id:length(24)}", Name = "GetPhoto")]
        public ActionResult<Photo> Get(string id)
        {
            var photo = _uploadService.Get(id);

            if (photo == null)
            {
                return NotFound();
            }

            return photo;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpPost, DisableRequestSizeLimit]
        [DisableFormValueModelBinding]
        public ActionResult<Object> UploadFile()
        {
            //if (Request.ContentLength == 0)
            //    return BadRequest();
            try
            {
                var file = _httpContext.Request.Form.Files[0];
                string path = _hostingEnvironment.WebRootPath;
                Photo photo = _uploadService.Upload(file, path);
                if (photo != null)
                    return CreatedAtRoute("GetPhoto", new { id = photo.Id.ToString() }, photo);
                else
                    return BadRequest();
            }
            catch(Exception ex)
            {
                var result = new { error = ex };
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return result;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpPut("{id:length(24)}")]
        public ActionResult<Photo> Edit(string id, Photo photo)
        {
            Photo newPhoto = _uploadService.Update(id, photo);

            return CreatedAtRoute("GetPhoto", new { id = photo.Id.ToString() }, newPhoto);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpDelete("{id:length(24)}")]
        public ActionResult<Photo> Delete(string id)
        {
            _uploadService.Remove(id);

            return NoContent();
        }
    }
}